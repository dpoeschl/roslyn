﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.PatternMatching;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Roslyn.Utilities;
using EditorCompletion = Microsoft.VisualStudio.Language.Intellisense;
using RoslynCompletionItem = Microsoft.CodeAnalysis.Completion.CompletionItem;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.Completion.EditorImplementation
{
    internal class EditorAsyncCompletionService : EditorCompletion.IAsyncCompletionService
    {
        private readonly IAsyncCompletionBroker _broker;
        private readonly CompletionHelper _completionHelper;

        private const int MaxMRUSize = 10;
        private ImmutableArray<string> _recentItems = ImmutableArray<string>.Empty;
        private static readonly CultureInfo EnUSCultureInfo = new CultureInfo("en-US");

        public EditorAsyncCompletionService(IAsyncCompletionBroker broker)
        {
            _broker = broker;
            _completionHelper = new CompletionHelper(isCaseSensitive: true);
        }

        public Task<ImmutableArray<EditorCompletion.CompletionItem>> SortCompletionListAsync(
            ImmutableArray<EditorCompletion.CompletionItem> initialList, 
            CompletionTriggerReason triggerReason, 
            ITextSnapshot snapshot, 
            ITrackingSpan applicableToSpan, 
            ITextView view, 
            CancellationToken token)
        {
            // TODO: We know that sessions start sort is invoked, but this could be cleaner
            _broker.GetSession(view).ItemCommitted += ItemCommitted;
            _broker.GetSession(view).Dismissed += SessionDismissed;
            return Task.FromResult(initialList.OrderBy(i => i.DisplayText).ToImmutableArray());
        }

        private void ItemCommitted(object sender, EditorCompletion.CompletionItemEventArgs e)
        {
            MakeMostRecentItem(e.Item.DisplayText);
        }

        private void SessionDismissed(object sender, EventArgs e)
        {
            // TODO: Unhook the session's events when the session is available in the args
        }

        public Task<FilteredCompletionModel> UpdateCompletionListAsync(
            ImmutableArray<EditorCompletion.CompletionItem> sortedList, 
            CompletionTriggerReason triggerReason, 
            EditorCompletion.CompletionFilterReason filterReason, 
            ITextSnapshot snapshot, 
            ITrackingSpan applicableToSpan, 
            ImmutableArray<CompletionFilterWithState> filters, 
            ITextView view, 
            CancellationToken cancellationToken)
        {
            var filterText = applicableToSpan.GetText(snapshot);

            // Check if the user is typing a number.  If so, only proceed if it's a number
            // directly after a <dot>.  That's because it is actually reasonable for completion
            // to be brought up after a <dot> and for the user to want to filter completion
            // items based on a number that exists in the name of the item.  However, when 
            // we are not after a dot (i.e. we're being brought up after <space> is typed)
            // then we don't want to filter things.  Consider the user writing:
            //
            //      dim i =<space>
            //
            // We'll bring up the completion list here (as VB has completion on <space>). 
            // If the user then types '3', we don't want to match against Int32.
            if (filterText.Length > 0 && char.IsNumber(filterText[0]))
            {
                if (!IsAfterDot(snapshot, applicableToSpan))
                {
                    return Task.FromResult(new FilteredCompletionModel(ImmutableArray<CompletionItemWithHighlight>.Empty, 0));
                }
            }

            // We need to filter if a non-empty strict subset of filters are selected
            var selectedFilters = filters.Where(f => f.IsSelected).Select(f => f.Filter).ToImmutableArray();
            var needToFilter = selectedFilters.Length > 0 && selectedFilters.Length < filters.Length;

            // Cache PatternMatchers across this round of filtering
            var patternMatcherMap = new Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher>();

            var initialListOfItemsToBeIncluded = new List<FilterResult>();
            foreach (var item in sortedList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (needToFilter && ShouldBeFilteredOutOfCompletionList(item, selectedFilters))
                {
                    continue;
                }

                if (MatchesFilterText(item, filterText, triggerReason, filterReason, patternMatcherMap))
                {
                    initialListOfItemsToBeIncluded.Add(new FilterResult(item, filterText, matchedFilterText: true));
                }
                else
                {
                    if (triggerReason == CompletionTriggerReason.Deletion ||
                        triggerReason == CompletionTriggerReason.Invoke ||
                        filterText.Length <= 1)
                    {
                        initialListOfItemsToBeIncluded.Add(new FilterResult(item, filterText, matchedFilterText: false));
                    }
                }
            }

            if (initialListOfItemsToBeIncluded.Count == 0)
            {
                return Task.FromResult(HandleAllItemsFilteredOut(triggerReason, filters, selectedFilters));
            }

            // If this was deletion, then we control the entire behavior of deletion
            // ourselves.
            if (triggerReason == CompletionTriggerReason.Deletion)
            {
                return HandleDeletionTrigger(sortedList, triggerReason, filters, filterReason, filterText, initialListOfItemsToBeIncluded, patternMatcherMap);
            }

            return HandleNormalFiltering(sortedList, snapshot, filterText, filters, initialListOfItemsToBeIncluded, patternMatcherMap);
        }

        private IEnumerable<CompletionItemWithHighlight> GetHighlightedList(
            List<FilterResult> filterResults, 
            string filterText, 
            Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher> patternMatcherMap)
        {
            var highlightedList = new List<CompletionItemWithHighlight>();
            foreach (var item in filterResults)
            {
                var highlightedSpans = _completionHelper.GetHighlightedSpans(item.CompletionItem.FilterText, filterText, CultureInfo.CurrentCulture);
                highlightedList.Add(new CompletionItemWithHighlight(item.CompletionItem, highlightedSpans.Select(s => s.ToSpan()).ToImmutableArray()));
            }

            return highlightedList;
        }

        private Task<FilteredCompletionModel> HandleDeletionTrigger(
            ImmutableArray<EditorCompletion.CompletionItem> sortedList,
            CompletionTriggerReason triggerReason,
            ImmutableArray<CompletionFilterWithState> filters,
            EditorCompletion.CompletionFilterReason filterReason,
            string filterText, 
            List<FilterResult> filterResults, 
            Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher> patternMatcherMap)
        {
            if (filterReason == EditorCompletion.CompletionFilterReason.Insertion && !filterResults.Any(r => r.MatchedFilterText))
            {
                // The user has typed something, but nothing in the actual list matched what
                // they were typing.  In this case, we want to dismiss completion entirely.
                // The thought process is as follows: we aggressively brough up completion
                // to help them when they typed delete (in case they wanted to pick another
                // item).  However, they're typing something that doesn't seem to match at all
                // The completion list is just distracting at this point.
                return Task.FromResult(new FilteredCompletionModel(ImmutableArray<EditorCompletion.CompletionItemWithHighlight>.Empty, 0));
            }

            FilterResult? bestFilterResult = null;
            int matchCount = 0;
            foreach (var currentFilterResult in filterResults.Where(r => r.MatchedFilterText))
            {
                if (bestFilterResult == null ||
                    IsBetterDeletionMatch(currentFilterResult, bestFilterResult.Value))
                {
                    // We had no best result yet, so this is now our best result.
                    bestFilterResult = currentFilterResult;
                    matchCount++;
                }
            }

            // If we had a matching item, then pick the best of the matching items and
            // choose that one to be hard selected.  If we had no actual matching items
            // (which can happen if the user deletes down to a single character and we
            // include everything), then we just soft select the first item.

            var filteredItems = filterResults.Select(r => r.CompletionItem).AsImmutable();
            var highlightedList = GetHighlightedList(filterResults, filterText, patternMatcherMap).ToImmutableArray();
            var updatedFilters = GetUpdatedFilters(sortedList, filterResults, filters, filterText, patternMatcherMap);

            if (bestFilterResult != null)
            {
                // Only hard select this result if it's a prefix match
                // We need to do this so that 
                // * deleting and retyping a dot in a member access does not change the 
                //   text that originally appeared before the dot
                // * deleting through a word from the end keeps that word selected
                // This also preserves the behavior the VB had through Dev12.
                var hardSelect = bestFilterResult.Value.CompletionItem.FilterText.StartsWith(filterText, StringComparison.CurrentCultureIgnoreCase);

                return Task.FromResult(new FilteredCompletionModel(highlightedList, filteredItems.IndexOf(bestFilterResult.Value.CompletionItem), updatedFilters, matchCount == 1 ? CompletionItemSelection.Selected : CompletionItemSelection.SoftSelected, uniqueItem: null));
            }
            else
            {
                return Task.FromResult(new FilteredCompletionModel(highlightedList, 0, updatedFilters, CompletionItemSelection.SoftSelected, uniqueItem: null));
            }
        }

        private ImmutableArray<CompletionFilterWithState> GetUpdatedFilters(
            ImmutableArray<EditorCompletion.CompletionItem> originalList, 
            List<FilterResult> filteredList, 
            ImmutableArray<CompletionFilterWithState> filters, 
            string filterText, 
            Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher> patternMatcherMap)
        {
            var missingItems = new List<EditorCompletion.CompletionItem>();
            int filteredListIndex = 0;
            var filtersPresentInIncludedItems = new HashSet<CompletionFilter>();
            for (int originalListIndex = 0; originalListIndex < originalList.Length; originalListIndex++)
            {
                if (filteredListIndex < filteredList.Count && originalList[originalListIndex] == filteredList[filteredListIndex].CompletionItem)
                {
                    foreach (var filter in filteredList[filteredListIndex].CompletionItem.Filters)
                    {
                        filtersPresentInIncludedItems.Add(filter);
                    }

                    filteredListIndex++;
                }
                else
                {
                    missingItems.Add(originalList[originalListIndex]);
                }
            }

            var filtersPresentInMissingItems = new HashSet<CompletionFilter>();
            foreach (var missingItem in missingItems)
            {
                if (_completionHelper.MatchesPattern(missingItem.FilterText, filterText, CultureInfo.CurrentCulture))
                {
                    foreach (var filter in missingItem.Filters)
                    {
                        filtersPresentInMissingItems.Add(filter);
                    }
                }
            }

            var resultingFilters = new List<CompletionFilterWithState>();

            foreach (var filter in filters)
            {
                var isAvailable = filter.IsSelected || filtersPresentInIncludedItems.Contains(filter.Filter) || filtersPresentInMissingItems.Contains(filter.Filter);
                resultingFilters.Add(filter.WithAvailability(isAvailable));
            }

            return resultingFilters.ToImmutableArray();
        }

        private bool IsBetterDeletionMatch(FilterResult result1, FilterResult result2)
        {
            var item1 = result1.CompletionItem;
            var item2 = result2.CompletionItem;

            var prefixLength1 = item1.FilterText.GetCaseInsensitivePrefixLength(result1.FilterText);
            var prefixLength2 = item2.FilterText.GetCaseInsensitivePrefixLength(result2.FilterText);

            // Prefer the item that matches a longer prefix of the filter text.
            if (prefixLength1 > prefixLength2)
            {
                return true;
            }

            if (prefixLength1 == prefixLength2)
            {
                // If the lengths are the same, prefer the one with the higher match priority.
                // But only if it's an item that would have been hard selected.  We don't want
                // to aggressively select an item that was only going to be softly offered.

                var item1Priority = item1.Properties.GetProperty<CompletionItemSelectionBehavior>("SelectionBehavior") == CompletionItemSelectionBehavior.HardSelection
                    ? item1.Properties.GetProperty<int>("MatchPriority") : MatchPriority.Default;
                var item2Priority = item2.Properties.GetProperty<CompletionItemSelectionBehavior>("SelectionBehavior") == CompletionItemSelectionBehavior.HardSelection
                    ? item2.Properties.GetProperty<int>("MatchPriority") : MatchPriority.Default;

                if (item1Priority > item2Priority)
                {
                    return true;
                }
            }

            return false;
        }

        private Task<FilteredCompletionModel> HandleNormalFiltering(
            ImmutableArray<EditorCompletion.CompletionItem> sortedList,
            ITextSnapshot snapshot, 
            string filterText,
            ImmutableArray<CompletionFilterWithState> filters,
            List<FilterResult> itemsInList, 
            Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher> patternMatcherMap)
        {
            var service = GetCompletionService(snapshot);
            if (service == null)
            {
                var listWithSelections = GetHighlightedList(itemsInList, filterText, patternMatcherMap);
                return Task.FromResult(new FilteredCompletionModel(listWithSelections.ToImmutableArray(), 0));
            }

            var matchingItems = itemsInList
                .Where(r => r.MatchedFilterText)
                .Select(t => t.CompletionItem.Properties.GetProperty<RoslynCompletionItem>("RoslynItem"))
                .AsImmutable();
            var chosenItems = service.FilterItems(snapshot.GetOpenDocumentInCurrentContextWithChanges(), matchingItems, filterText);

            var recentItems = _recentItems;
            var bestItem = GetBestItemBasedOnMRU(chosenItems, recentItems);
            var highlightedList = GetHighlightedList(itemsInList, filterText, patternMatcherMap).ToImmutableArray();
            var updatedFilters = GetUpdatedFilters(sortedList, itemsInList, filters, filterText, patternMatcherMap);

            if (bestItem == null)
            {
                return Task.FromResult(new FilteredCompletionModel(highlightedList, 0, updatedFilters));
            }

            // TODO: Better conversion between Roslyn/Editor completion items
            var selectedItemIndex = itemsInList.IndexOf(i => i.CompletionItem.DisplayText == bestItem.DisplayText);

            EditorCompletion.CompletionItem uniqueItem = null;
            if (bestItem != null && matchingItems.Length == 1 && filterText.Length > 0)
            {
                uniqueItem = highlightedList[selectedItemIndex].CompletionItem;
            }

            return Task.FromResult(new FilteredCompletionModel(highlightedList, selectedItemIndex, updatedFilters, CompletionItemSelection.NoChange, uniqueItem));
        }

        private static bool IsAfterDot(ITextSnapshot snapshot, ITrackingSpan applicableToSpan)
        {
            var position = applicableToSpan.GetStartPoint(snapshot).Position;
            return position > 0 && snapshot.GetText(position - 1, 1) == ".";
        }

        private void MakeMostRecentItem(string item)
        {
            var updated = false;

            while (!updated)
            {
                var oldItems = _recentItems;
                var newItems = oldItems.Remove(item);

                if (newItems.Length == MaxMRUSize)
                {
                    // Remove the least recent item.
                    newItems = newItems.RemoveAt(0);
                }

                newItems = newItems.Add(item);

                updated = ImmutableInterlocked.InterlockedCompareExchange(ref _recentItems, newItems, oldItems) == oldItems;
            }
        }

        private RoslynCompletionItem GetBestItemBasedOnMRU(ImmutableArray<RoslynCompletionItem> chosenItems, ImmutableArray<string> recentItems)
        {
            if (chosenItems.Length == 0)
            {
                return null;
            }

            // Try to find the chosen item has been most recently used.
            var bestItem = chosenItems.First();

            for (int i = 0, n = chosenItems.Length; i < n; i++)
            {
                var chosenItem = chosenItems[i];
                var mruIndex1 = GetRecentItemIndex(recentItems, bestItem.DisplayText);
                var mruIndex2 = GetRecentItemIndex(recentItems, chosenItem.DisplayText);

                if (mruIndex2 < mruIndex1)
                {
                    bestItem = chosenItem;
                }
            }

            // If our best item appeared in the MRU list, use it
            if (GetRecentItemIndex(recentItems, bestItem.DisplayText) <= 0)
            {
                return bestItem;
            }

            // Otherwise use the chosen item that has the highest matchPriority.
            for (int i = 1, n = chosenItems.Length; i < n; i++)
            {
                var chosenItem = chosenItems[i];
                int.TryParse(ImmutableDictionary.GetValueOrDefault(bestItem.Properties, "MatchPriority"), out var bestItemPriority);
                int.TryParse(ImmutableDictionary.GetValueOrDefault(chosenItem.Properties, "MatchPriority"), out var currentItemPriority);

                if (currentItemPriority > bestItemPriority)
                {
                    bestItem = chosenItem;
                }
            }

            return bestItem;
        }

        private static int GetRecentItemIndex(ImmutableArray<string> recentItems, string itemDisplayText)
        {
            var index = recentItems.IndexOf(itemDisplayText);
            return -index;
        }

        private FilteredCompletionModel HandleAllItemsFilteredOut
            (EditorCompletion.CompletionTriggerReason triggerReason,
            ImmutableArray<CompletionFilterWithState> filters,
            ImmutableArray<CompletionFilter> activeFilters)
        {
            // TODO: DismissIfEmpty?
            // If the user was just typing, and the list went to empty *and* this is a 
            // language that wants to dismiss on empty, then just return a null model
            // to stop the completion session.

            if (triggerReason == CompletionTriggerReason.Insertion)
            {
                // TODO: Stop completion when that API is available
                return new FilteredCompletionModel(ImmutableArray<CompletionItemWithHighlight>.Empty, 0, filters);
            }

            if (activeFilters.Length > 0)
            {
                // If the user has turned on some filtering states, and we filtered down to 
                // nothing, then we do want the UI to show that to them.  That way the user
                // can turn off filters they don't want and get the right set of items.

                return new FilteredCompletionModel(ImmutableArray<CompletionItemWithHighlight>.Empty, 0, filters);
            }
            else
            {
                // If we are going to filter everything out, then just preserve the existing
                // model (and all the previously filtered items), but switch over to soft 
                // selection.

                return new FilteredCompletionModel(ImmutableArray<CompletionItemWithHighlight>.Empty, 0, filters, CompletionItemSelection.SoftSelected, uniqueItem: null);
            }
        }

        private CompletionService GetCompletionService(ITextSnapshot snapshot)
        {
            if (!Workspace.TryGetWorkspace(snapshot.TextBuffer.AsTextContainer(), out var workspace))
            {
                return null;
            }

            return workspace.Services.GetLanguageServices(snapshot.TextBuffer).GetService<CompletionService>();
        }

        private bool MatchesFilterText(
            EditorCompletion.CompletionItem item, 
            string filterText, 
            EditorCompletion.CompletionTriggerReason triggerReason,
            EditorCompletion.CompletionFilterReason filterReason,
            Dictionary<(string pattern, CultureInfo, bool includeMatchedSpans), PatternMatcher> patternMatcherMap)
        {
            // For the deletion we bake in the core logic for how matching should work.
            // This way deletion feels the same across all languages that opt into deletion 
            // as a completion trigger.

            // Specifically, to avoid being too aggressive when matching an item during 
            // completion, we require that the current filter text be a prefix of the 
            // item in the list.

            if (triggerReason == CompletionTriggerReason.Deletion && filterReason == EditorCompletion.CompletionFilterReason.Deletion)
            {
                return item.FilterText.GetCaseInsensitivePrefixLength(filterText) > 0;
            }

            // If the user hasn't typed anything, and this item was preselected, or was in the
            // MRU list, then we definitely want to include it.
            if (filterText.Length == 0)
            {
                // TODO: Need ItemRules.MatchPriority.
                if (item.Properties.GetProperty<int>("MatchPriority") > MatchPriority.Default)
                {
                    return true;
                }

                if (!_recentItems.IsDefault && GetRecentItemIndex(_recentItems, item.DisplayText) <= 0)
                {
                    return true;
                }
            }

            return _completionHelper.MatchesPattern(item.FilterText, filterText, CultureInfo.CurrentCulture);
        }

        private bool ShouldBeFilteredOutOfCompletionList(EditorCompletion.CompletionItem item, ImmutableArray<CompletionFilter> activeFilters)
        {
            foreach (var itemFilter in item.Filters)
            {
                if (activeFilters.Contains(itemFilter))
                {
                    return false;
                }
            }

            return true;
        }

        private struct FilterResult
        {
            public EditorCompletion.CompletionItem CompletionItem;
            public string FilterText;
            public bool MatchedFilterText;

            public FilterResult(EditorCompletion.CompletionItem item, string filterText, bool matchedFilterText)
            {
                CompletionItem = item;
                FilterText = filterText;
                MatchedFilterText = matchedFilterText;
            }
        }
    }
}

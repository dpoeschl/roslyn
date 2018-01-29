﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;
using EditorCompletion = Microsoft.VisualStudio.Language.Intellisense;
using RoslynCompletionItem = Microsoft.CodeAnalysis.Completion.CompletionItem;
using RoslynTrigger = Microsoft.CodeAnalysis.Completion.CompletionTrigger;

namespace RoslynCompletionPrototype
{
    [Export(typeof(IAsyncCompletionItemSource))]
    [Name("C# and Visual Basic Completion Item Source")]
    [ContentType(ContentTypeNames.VisualBasicContentType)]
    [ContentType(ContentTypeNames.CSharpContentType)]
    internal class RoslynCompletionItemSource : IAsyncCompletionItemSource
    {
        private ImmutableArray<char> CommitChars => ImmutableArray.Create(
            ' ', '{', '}', '[', ']', '(', ')', '.', ',', ':',
            ';', '+', '-', '*', '/', '%', '&', '|', '^', '!',
            '~', '=', '<', '>', '?', '@', '#', '\'', '\"', '\\');
        private const string RoslynItem = nameof(RoslynItem);
        private const string TriggerSnapshot = nameof(TriggerSnapshot);

        public async Task<EditorCompletion.CompletionContext> GetCompletionContextAsync(
            EditorCompletion.CompletionTrigger trigger, 
            SnapshotPoint triggerLocation,
            CancellationToken cancellationToken)
        {
            var snapshot = triggerLocation.Snapshot;
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                // TODO: return default;
                return new EditorCompletion.CompletionContext(ImmutableArray<EditorCompletion.CompletionItem>.Empty, new SnapshotSpan(triggerLocation.Snapshot, new Span(0, 0)));
            }

            var completionService = document.GetLanguageService<CompletionService>();

            var completionList = await completionService.GetCompletionsAsync(document, triggerLocation.Position).ConfigureAwait(false);
            if (completionList == null)
            {
                // TODO: return default;
                return new EditorCompletion.CompletionContext(ImmutableArray<EditorCompletion.CompletionItem>.Empty, new SnapshotSpan(triggerLocation.Snapshot, new Span(0, 0)));
            }

            var text = await document.GetTextAsync().ConfigureAwait(false);
            var applicableSpan = completionService.GetDefaultCompletionListSpan(text, triggerLocation.Position).ToSnapshotSpan(triggerLocation.Snapshot);

            var imageIdService = document.Project.Solution.Workspace.Services.GetService<IImageIdService>();

            Dictionary<string, CompletionFilter> filterCache = new Dictionary<string, CompletionFilter>();

            var service = GetCompletionService(triggerLocation.Snapshot.TextBuffer.CurrentSnapshot) as CompletionServiceWithProviders;

            var items = completionList.Items.SelectAsArray(roslynItem =>
            {
                var needsCustomCommit = service.GetProvider(roslynItem) is ICustomCommitCompletionProvider;

                var item = Convert(roslynItem, imageIdService, completionService, filterCache, needsCustomCommit);
                item.Properties.AddProperty(TriggerSnapshot, triggerLocation.Snapshot);
                return item;
            });

            return new EditorCompletion.CompletionContext(
                items,
                applicableSpan, 
                useSoftSelection: false, 
                useSuggestionMode: completionList.SuggestionModeItem != null,
                suggestionModeDescription: completionList.SuggestionModeItem?.DisplayText);
        }

        private EditorCompletion.CompletionItem Convert(
            RoslynCompletionItem roslynItem, 
            IImageIdService imageService, 
            CompletionService completionService, 
            Dictionary<string, CompletionFilter> filterCache,
            bool needsCustomCommit)
        {
            var imageId = imageService.GetImageId(roslynItem.Tags.GetGlyph());
            var insertionText = roslynItem.DisplayText; // TODO
            var filters = GetFilters(roslynItem, imageService, filterCache);
            var item = new EditorCompletion.CompletionItem(
                roslynItem.DisplayText,
                this,
                imageId,
                filters,
                suffix: "Test Suffix",
                useCustomCommit: needsCustomCommit,
                insertText: insertionText,
                roslynItem.SortText,
                roslynItem.FilterText,
                attributeIcons: ImmutableArray<AccessibleImage>.Empty);

            item.Properties.AddProperty(RoslynItem, roslynItem);
            return item;
        }

        private ImmutableArray<CompletionFilter> GetFilters(RoslynCompletionItem item, IImageIdService imageService, Dictionary<string, CompletionFilter> filterCache)
        {
            var result = new List<CompletionFilter>();
            foreach (var filter in CompletionItemFilter.AllFilters)
            {
                if (filter.Matches(item))
                {
                    if (filterCache.ContainsKey(filter.DisplayText))
                    {
                        result.Add(filterCache[filter.DisplayText]);
                    }
                    else
                    {
                        var itemFilter = new CompletionFilter(filter.DisplayText, filter.AccessKey.ToString(), imageService.GetImageId(filter.Tags.GetGlyph()));
                        filterCache[filter.DisplayText] = itemFilter;
                        result.Add(itemFilter);
                    }
                }
            }

            return result.ToImmutableArray();
        }

        public async Task<object> GetDescriptionAsync(EditorCompletion.CompletionItem item, CancellationToken cancellationToken)
        {
            item.Properties.TryGetProperty<RoslynCompletionItem>("RoslynItem", out var roslynItem);
            item.Properties.TryGetProperty<ITextSnapshot>("TriggerSnapshot", out var triggerSnapshot);

            Workspace.TryGetWorkspace(triggerSnapshot.TextBuffer.AsTextContainer(), out var workspace);

            var document = workspace.CurrentSolution.GetDocument(workspace.GetDocumentIdInCurrentContext(triggerSnapshot.TextBuffer.AsTextContainer()));
            var service = document.GetLanguageService<CompletionService>() as CompletionServiceWithProviders;
            var provider = service.GetProvider(roslynItem);

            var description = await provider.GetDescriptionAsync(document, roslynItem, cancellationToken).ConfigureAwait(false);

            // TODO: Tagged Text
            // TODO: Snippet invocation part?

            return description.Text;
        }

        public void CustomCommit(ITextView view, ITextBuffer buffer, EditorCompletion.CompletionItem item, ITrackingSpan applicableSpan, char commitCharacter)
        {
            var service = GetCompletionService(buffer.CurrentSnapshot) as CompletionServiceWithProviders;

            var roslynItem = item.Properties.GetProperty<RoslynCompletionItem>(RoslynItem); // We're using custom data we deposited in GetCompletionContextAsync
            var triggerSnapshot = item.Properties.GetProperty<ITextSnapshot>(TriggerSnapshot);

            var edit = buffer.CreateEdit();
            var provider = service.GetProvider(roslynItem);
            if (provider is ICustomCommitCompletionProvider c)
            {
                c.Commit(roslynItem, view, buffer, triggerSnapshot, null);
            }
            else
            {
                var document = buffer.GetRelatedDocuments().First();
                var roslynChange = service.GetChangeAsync(document, roslynItem, commitCharacter, CancellationToken.None).Result;

                // TODO: Editor to reapply inserted trigger after we commit
                var ts = new SnapshotSpan(triggerSnapshot, roslynChange.TextChange.Span.ToSpan());
                var mapped = ts.TranslateTo(buffer.CurrentSnapshot, SpanTrackingMode.EdgeInclusive);

                edit.Replace(mapped.Span, roslynChange.TextChange.NewText);
            }

            edit.Apply();
        }

        public ImmutableArray<char> GetPotentialCommitCharacters()
        {
            return CommitChars;
        }

        public bool ShouldCommitCompletion(char typedChar, SnapshotPoint location)
        {
            return CommitChars.Contains(typedChar);
        }

        public bool ShouldTriggerCompletion(char edit, SnapshotPoint location)
        {
            var text = SourceText.From(location.Snapshot.GetText());
            var service = GetCompletionService(location.Snapshot);
            return service?.ShouldTriggerCompletion(text, location.Position, RoslynTrigger.CreateInsertionTrigger(edit)) ?? false;
        }

        private CompletionService GetCompletionService(ITextSnapshot snapshot)
        {
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return null;
            }

            var workspace = document.Project.Solution.Workspace;
            return workspace.Services.GetLanguageServices(LanguageNames.CSharp).GetService<CompletionService>();
        }

        public Task HandleViewClosedAsync(ITextView view) => Task.CompletedTask;
    }
}

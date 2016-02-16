// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal partial class NamingStylesOptionPageControlViewModel : AbstractNotifyPropertyChanged
    {
        public NamingRuleTreeItemViewModel _root;
        internal INotificationService _notificationService;

        public ObservableCollection<SymbolSpecificationViewModel> SymbolSpecificationList { get; set; }
        public ObservableCollection<NamingStyleViewModel> NamingStyleList { get; set; }

        internal void AddSymbolSpec(SymbolSpecificationViewModel viewModel)
        {
            SymbolSpecificationList.Add(viewModel);
        }

        internal void AddNamingSpec(NamingStyleViewModel viewModel)
        {
            NamingStyleList.Add(viewModel);
        }

        public void AddNamingPreference(NamingRuleDialogViewModel viewModel)
        {
            var newNode = new NamingRuleTreeItemViewModel(
                viewModel.Title,
                viewModel.SymbolSpecificationList.GetItemAt(viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel,
                viewModel.NamingStyleList.GetItemAt(viewModel.NamingStyleIndex) as NamingStyleViewModel,
                viewModel.EnforcementLevelsList[viewModel.EnforcementLevelIndex],
                this);

            if (viewModel.ParentRuleIndex == 0)
            {
                _root.Children.Add(newNode);
            }
            else
            {
                var parent = viewModel.ParentRuleList.GetItemAt(viewModel.ParentRuleIndex) as NamingRuleTreeItemViewModel;
                parent.Children.Add(newNode);
            }
        }

        internal NamingStylesOptionPageControlViewModel(SerializableNamingStylePreferencesInfo info, INotificationService notificationService)
        {
            this._notificationService = notificationService;
            this.SymbolSpecificationList = new ObservableCollection<SymbolSpecificationViewModel>(info.SymbolSpecifications.Select(s => new SymbolSpecificationViewModel(s, notificationService)));
            this.NamingStyleList = new ObservableCollection<NamingStyleViewModel>(info.NamingStyles.Select(s => new NamingStyleViewModel(s, _notificationService)));
            this._root = CreateRoot(info);
        }

        private NamingRuleTreeItemViewModel CreateRoot(SerializableNamingStylePreferencesInfo info)
        {
            var root = new NamingRuleTreeItemViewModel("Naming Rules:");
            CreateRootHelper(root, info.NamingRules);
            return root;
        }

        private void CreateRootHelper(NamingRuleTreeItemViewModel rule, List<SerializableNamingRule> children)
        {            
            foreach (var child in children)
            {
                var newRule = new NamingRuleTreeItemViewModel(
                    child.Title,
                    SymbolSpecificationList.SingleOrDefault(s => s.ID == child.SymbolSpecificationID),
                    NamingStyleList.SingleOrDefault(s => s.ID == child.NamingStyleID),
                    new EnforcementLevel("Warning", CodeAnalysis.DiagnosticSeverity.Warning, KnownMonikers.StatusWarning), // TODO!
                    this);

                CreateRootHelper(newRule, child.Children);
                rule.Children.Add(newRule);
            }
        }

        public SerializableNamingStylePreferencesInfo GetInfo()
        {
            // TODO!
            var info = new SerializableNamingStylePreferencesInfo();
            info.SymbolSpecifications = SymbolSpecificationList.Select(s => s.GetSymbolSpecification()).ToList();
            info.NamingStyles = NamingStyleList.Select(s => s.GetNamingStyle()).ToList();
            info.NamingRules = CreateNamingRuleTree();
            return info;
        }

        private List<SerializableNamingRule> CreateNamingRuleTree()
        {
            var result = new List<SerializableNamingRule>();
            CreateNamingRuleTreeHelper(result, _root.Children);
            return result;
        }

        private void CreateNamingRuleTreeHelper(List<SerializableNamingRule> result, IList<NamingRuleTreeItemViewModel> children)
        {
            foreach (var child in children)
            {
                var childTree = new SerializableNamingRule
                {
                    Title = child.Title,
                    Children = new List<SerializableNamingRule>(),
                    SymbolSpecificationID = child.symbolSpec?.ID ?? Guid.Empty,
                    NamingStyleID = child.namingStyle?.ID ?? Guid.Empty,
                    EnforcementLevel = child.EnforcementLevel.Value
                };
                CreateNamingRuleTreeHelper(childTree.Children, child.Children);
                result.Add(childTree);
            }
        }

        internal bool TrySubmit()
        {
            return true;
        }

        internal List<NamingRuleTreeItemViewModel> CreateAllowableParentList(NamingRuleTreeItemViewModel excludedSubtree = null)
        {
            var ruleList = new List<NamingRuleTreeItemViewModel>();
            foreach (var child in _root.Children)
            {
                CreateAllowableParentListHelper(child, ruleList, excludedSubtree);
            }

            return ruleList;
        }

        public void KeyCommand(object sender, KeyEventArgs e)
        {
        }

        private void CreateAllowableParentListHelper(NamingRuleTreeItemViewModel ruleToProcess, List<NamingRuleTreeItemViewModel> ruleList, NamingRuleTreeItemViewModel excludedSubtree)
        {
            if (ruleToProcess == excludedSubtree)
            {
                return;
            }

            ruleList.Add(ruleToProcess);
            foreach (var child in ruleToProcess.Children)
            {
                CreateAllowableParentListHelper(child, ruleList, excludedSubtree);
            }
        }

        internal void DeleteSymbolSpec(SymbolSpecificationViewModel a)
        {
            if (!SymbolSpecUsedInTree(a))
            {
                SymbolSpecificationList.Remove(a);
            }
        }

        internal void DeleteNamingStyle(NamingStyleViewModel a)
        {
            if (!NamingStyleUsedInTree(a))
            {
                NamingStyleList.Remove(a);
            }
        }

        internal void DeleteRuleAtIndex(int a)
        {
            var rule = GetRuleAtPosition(a);
            if (rule != null)
            {
                DeleteRule(rule);
            }
        }

        private NamingRuleTreeItemViewModel GetRuleAtPosition(int a)
        {
            Queue<NamingRuleTreeItemViewModel> q = new Queue<NamingRuleTreeItemViewModel>();
            q.Enqueue(_root);

            for (int i = 0; i <= a; i++)
            {
                var elementAtIndex = q.Dequeue();
                if (i == a)
                {
                    return elementAtIndex;
                }

                if (elementAtIndex.HasChildren)
                {
                    foreach (var child in elementAtIndex.Children)
                    {
                        q.Enqueue(child);
                    }
                }
            }

            return null;
        }

        internal void DeleteRule(NamingRuleTreeItemViewModel a)
        {
            if (!a.HasChildren && a.Parent != null)
            {
                a.Parent.Children.Remove(a);
            }
        }


        private bool SymbolSpecUsedInTree(SymbolSpecificationViewModel a)
        {
            return _root.Children.Any(c => c.symbolSpec == a || SymbolSpecUsedInTree(a, c));
        }

        private bool SymbolSpecUsedInTree(SymbolSpecificationViewModel a, NamingRuleTreeItemViewModel c)
        {
            return c.Children.Any(child => child.symbolSpec == a || SymbolSpecUsedInTree(a, child));
        }

        private bool NamingStyleUsedInTree(NamingStyleViewModel a)
        {
            return _root.Children.Any(c => c.namingStyle == a || NamingStyleUsedInTree(a, c));
        }

        private bool NamingStyleUsedInTree(NamingStyleViewModel a, NamingRuleTreeItemViewModel c)
        {
            return c.Children.Any(child => child.namingStyle == a || NamingStyleUsedInTree(a, child));
        }

    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using System.Windows.Input;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using System;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal partial class NamingPreferencesDialogViewModel : AbstractNotifyPropertyChanged
    {
        public NamingRuleTreeViewModel _root;
        public List<SymbolSpecificationViewModel> SymbolSpecificationList { get; set; }
        public List<NamingStyleViewModel> NamingStyleList { get; set; }

        internal void AddSymbolSpec(SymbolSpecificationViewModel viewModel)
        {
            var someList = new List<SymbolSpecificationViewModel>(SymbolSpecificationList);
            someList.Add(viewModel);
            SymbolSpecificationList = someList;
            NotifyPropertyChanged(nameof(SymbolSpecificationList));
        }

        internal void AddNamingSpec(NamingStyleViewModel viewModel)
        {
            var someList = new List<NamingStyleViewModel>(NamingStyleList);
            someList.Add(viewModel);
            NamingStyleList = someList;
            NotifyPropertyChanged(nameof(NamingStyleList));
        }

        public void AddNamingPreference(NamingRuleDialogViewModel viewModel)
        {
            var newNode = new NamingRuleTreeViewModel(
                viewModel.Title,
                viewModel.SymbolSpecificationList.GetItemAt(viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel,
                viewModel.NamingStyleList.GetItemAt(viewModel.NamingStyleIndex) as NamingStyleViewModel,
                this);

            if (viewModel.ParentRuleIndex == 0)
            {
                _root.Children.Add(newNode);
            }
            else
            {
                var parent = viewModel.ParentRuleList.GetItemAt(viewModel.ParentRuleIndex) as NamingRuleTreeViewModel;
                parent.Children.Add(newNode);
            }
        }

        internal NamingPreferencesDialogViewModel(SerializableNamingStylePreferencesInfo info)
        {
            this.SymbolSpecificationList = info.SymbolSpecifications.Select(s => new SymbolSpecificationViewModel(s)).ToList();
            this.NamingStyleList = info.NamingStyles.Select(s => new NamingStyleViewModel(s)).ToList();
            this._root = CreateRoot(info);
        }

        private NamingRuleTreeViewModel CreateRoot(SerializableNamingStylePreferencesInfo info)
        {
            var root = new NamingRuleTreeViewModel("Naming Rules:");
            CreateRootHelper(root, info.NamingRules);
            return root;
        }

        private void CreateRootHelper(NamingRuleTreeViewModel rule, List<SerializableNamingRule> children)
        {            
            foreach (var child in children)
            {
                var newRule = new NamingRuleTreeViewModel(
                    child.Title,
                    SymbolSpecificationList.SingleOrDefault(s => s.ID == child.SymbolSpecificationID),
                    NamingStyleList.SingleOrDefault(s => s.ID == child.NamingStyleID),
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

        private void CreateNamingRuleTreeHelper(List<SerializableNamingRule> result, IList<NamingRuleTreeViewModel> children)
        {
            foreach (var child in children)
            {
                var childTree = new SerializableNamingRule
                {
                    Title = child.Title,
                    Children = new List<SerializableNamingRule>(),
                    SymbolSpecificationID = child.symbolSpec?.ID ?? Guid.Empty,
                    NamingStyleID = child.namingStyle?.ID ?? Guid.Empty
                };
                CreateNamingRuleTreeHelper(childTree.Children, child.Children);
                result.Add(childTree);
            }
        }

        internal bool TrySubmit()
        {
            return true;
        }

        internal List<NamingRuleTreeViewModel> CreateAllowableParentList(NamingRuleTreeViewModel excludedSubtree = null)
        {
            var ruleList = new List<NamingRuleTreeViewModel>();
            foreach (var child in _root.Children)
            {
                CreateAllowableParentListHelper(child, ruleList, excludedSubtree);
            }

            return ruleList;
        }

        public void KeyCommand(object sender, KeyEventArgs e)
        {
        }

        private void CreateAllowableParentListHelper(NamingRuleTreeViewModel ruleToProcess, List<NamingRuleTreeViewModel> ruleList, NamingRuleTreeViewModel excludedSubtree)
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

        internal void DeleteNamingSpec(SymbolSpecificationViewModel a)
        {
            SymbolSpecificationList.Remove(a);
            NotifyPropertyChanged(nameof(SymbolSpecificationList));
        }
    }
}

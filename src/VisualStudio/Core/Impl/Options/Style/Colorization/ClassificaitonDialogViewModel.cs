// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using System.Windows.Input;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using System;
using System.Collections.ObjectModel;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    internal partial class ClassificaitonDialogViewModel : AbstractNotifyPropertyChanged
    {
        public ClassificationStyleTreeViewModel _root;
        public ObservableCollection<SymbolSpecificationViewModel> SymbolSpecificationList { get; set; }
        public ObservableCollection<ClassificationStyleViewModel> NamingStyleList { get; set; }

        internal void AddSymbolSpec(SymbolSpecificationViewModel viewModel)
        {
            SymbolSpecificationList.Add(viewModel);
        }

        internal void AddNamingSpec(ClassificationStyleViewModel viewModel)
        {
            NamingStyleList.Add(viewModel);
        }

        public void AddNamingPreference(ClassificationRuleDialogViewModel viewModel)
        {
            var newNode = new ClassificationStyleTreeViewModel(
                viewModel.Title,
                viewModel.SymbolSpecificationList.GetItemAt(viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel,
                viewModel.NamingStyleList.GetItemAt(viewModel.NamingStyleIndex) as ClassificationStyleViewModel,
                this);

            if (viewModel.ParentRuleIndex == 0)
            {
                _root.Children.Add(newNode);
            }
            else
            {
                var parent = viewModel.ParentRuleList.GetItemAt(viewModel.ParentRuleIndex) as ClassificationStyleTreeViewModel;
                parent.Children.Add(newNode);
            }
        }

        internal ClassificaitonDialogViewModel(SerializableNamingStylePreferencesInfo info)
        {
            this.SymbolSpecificationList = new ObservableCollection<SymbolSpecificationViewModel>(info.SymbolSpecifications.Select(s => new SymbolSpecificationViewModel(s)));
            this.NamingStyleList = new ObservableCollection<ClassificationStyleViewModel>(info.NamingStyles.Select(s => new ClassificationStyleViewModel(s)));
            this._root = CreateRoot(info);
        }

        private ClassificationStyleTreeViewModel CreateRoot(SerializableNamingStylePreferencesInfo info)
        {
            var root = new ClassificationStyleTreeViewModel("Naming Rules:");
            CreateRootHelper(root, info.NamingRules);
            return root;
        }

        private void CreateRootHelper(ClassificationStyleTreeViewModel rule, List<SerializableNamingRule> children)
        {            
            foreach (var child in children)
            {
                var newRule = new ClassificationStyleTreeViewModel(
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

        private void CreateNamingRuleTreeHelper(List<SerializableNamingRule> result, IList<ClassificationStyleTreeViewModel> children)
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

        internal List<ClassificationStyleTreeViewModel> CreateAllowableParentList(ClassificationStyleTreeViewModel excludedSubtree = null)
        {
            var ruleList = new List<ClassificationStyleTreeViewModel>();
            foreach (var child in _root.Children)
            {
                CreateAllowableParentListHelper(child, ruleList, excludedSubtree);
            }

            return ruleList;
        }

        public void KeyCommand(object sender, KeyEventArgs e)
        {
        }

        private void CreateAllowableParentListHelper(ClassificationStyleTreeViewModel ruleToProcess, List<ClassificationStyleTreeViewModel> ruleList, ClassificationStyleTreeViewModel excludedSubtree)
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

        internal void DeleteNamingStyle(ClassificationStyleViewModel a)
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

        private ClassificationStyleTreeViewModel GetRuleAtPosition(int a)
        {
            Queue<ClassificationStyleTreeViewModel> q = new Queue<ClassificationStyleTreeViewModel>();
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

        internal void DeleteRule(ClassificationStyleTreeViewModel a)
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

        private bool SymbolSpecUsedInTree(SymbolSpecificationViewModel a, ClassificationStyleTreeViewModel c)
        {
            return c.Children.Any(child => child.symbolSpec == a || SymbolSpecUsedInTree(a, child));
        }

        private bool NamingStyleUsedInTree(ClassificationStyleViewModel a)
        {
            return _root.Children.Any(c => c.namingStyle == a || NamingStyleUsedInTree(a, c));
        }

        private bool NamingStyleUsedInTree(ClassificationStyleViewModel a, ClassificationStyleTreeViewModel c)
        {
            return c.Children.Any(child => child.namingStyle == a || NamingStyleUsedInTree(a, child));
        }

    }
}

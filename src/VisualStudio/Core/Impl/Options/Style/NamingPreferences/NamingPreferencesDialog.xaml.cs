// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    /// <summary>
    /// Interaction logic for ExtractInterfaceDialog.xaml
    /// </summary>
    internal partial class NamingPreferencesDialog : DialogWindow
    {
        private readonly NamingPreferencesDialogViewModel _viewModel;

        internal NamingPreferencesDialog(NamingPreferencesDialogViewModel viewModel)
            : base(helpTopic: "TODO")
        {
            _viewModel = viewModel;
            // SetCommandBindings();

            InitializeComponent();
            DataContext = viewModel;

            this.RootTreeView.ItemsPath = nameof(NamingRuleTreeViewModel.Children);
            this.RootTreeView.IsExpandablePath = nameof(NamingRuleTreeViewModel.HasChildren);
            this.RootTreeView.FilterParentEvaluator = GetParent;
            this.RootTreeView.RootItemsSource = new ObservableCollection<NamingRuleTreeViewModel>() { viewModel._root };
      }

        private IEnumerable<object> GetParent(object item)
        {
            NamingRuleTreeViewModel viewModel = item as NamingRuleTreeViewModel;
            if (viewModel != null && viewModel.Parent != null)
            {
                yield return viewModel.Parent;
            }
        }

        private void EnsureAncestorsExpanded(NamingRuleTreeViewModel item)
        {
            Stack<NamingRuleTreeViewModel> models = new Stack<NamingRuleTreeViewModel>();
            NamingRuleTreeViewModel iter = item.Parent;
            while (iter != null)
            {
                models.Push(iter);
                iter = iter.Parent;
            }

            while (models.Count > 0)
            {
                NamingRuleTreeViewModel manager = models.Pop();
                IVirtualizingTreeNode managerNode = this.RootTreeView.GetFirstTreeNode(manager);
                managerNode.IsExpanded = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new SymbolSpecificationViewModel();
            var dialog = new SymbolSpecificationDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddSymbolSpec(viewModel);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = new NamingStyleViewModel(new CodeAnalysis.Diagnostics.Analyzers.NamingStyle { ID = Guid.NewGuid() });
            var dialog = new NamingStyleDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddNamingSpec(viewModel);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var selectedSpecification = SymbolSpecificationList.SelectedItem as SymbolSpecificationViewModel;
            var selectedStyle = NamingConventionList.SelectedItem as NamingStyleViewModel;

            var viewModel = new NamingRuleDialogViewModel(
                string.Empty,
                selectedSpecification,
                _viewModel.SymbolSpecificationList,
                selectedStyle,
                _viewModel.NamingStyleList,
                null,
                _viewModel.CreateAllowableParentList(_viewModel._root));
            var dialog = new NamingRuleDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddNamingPreference(viewModel);
            }
        }

        private void Delete_1(object sender, RoutedEventArgs e)
        {
            var a = SymbolSpecificationList.SelectedItem as SymbolSpecificationViewModel;
            if (a != null)
            {
                _viewModel.DeleteNamingSpec(a);
            }
        }

        private void Delete_2(object sender, RoutedEventArgs e)
        {
        }
        private void Delete_3(object sender, RoutedEventArgs e)
        {
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.TrySubmit())
            {
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NamingConventionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as NamingStyleViewModel;
            if (item != null)
            {
                var dialog = new NamingStyleDialog(item);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    // Ugh.
                }
            }
        }

        private void SymbolSpecificationList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as SymbolSpecificationViewModel;
            if (item != null)
            {
                var dialog = new SymbolSpecificationDialog(item);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    // Ugh.
                }
            }
        }

        public void KeyCommand(object sender, KeyEventArgs e)
        {
        }
    }
}

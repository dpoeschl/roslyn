// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    /// <summary>
    /// Interaction logic for NamingRuleDialog.xaml
    /// </summary>
    internal partial class NamingRuleDialog : DialogWindow
    {
        private readonly NamingRuleDialogViewModel _viewModel;
        private readonly NamingStylesOptionPageControlViewModel _outerViewModel;
        private readonly INotificationService _notificationService;

        internal NamingRuleDialog(NamingRuleDialogViewModel viewModel, NamingStylesOptionPageControlViewModel outerViewModel, INotificationService notificationService)
            : base(helpTopic: "TODO")
        {
            _notificationService = notificationService;

            _viewModel = viewModel;
            _outerViewModel = outerViewModel;

            InitializeComponent();
            DataContext = viewModel;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.TrySubmit())
            {
                DialogResult = true;
            }
        }

        private void CreateSymbolSpecification(object sender, RoutedEventArgs e)
        {
            var viewModel = new SymbolSpecificationViewModel(_notificationService);
            var dialog = new SymbolSpecificationDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _outerViewModel.AddSymbolSpec(viewModel);
                _viewModel.SelectedSymbolSpecificationIndex = _viewModel.SymbolSpecificationList.IndexOf(viewModel);
            }
        }

        private void ConfigureSymbolSpecifications(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedSymbolSpecificationIndex >= 0)
            {
                var spec = _viewModel.SymbolSpecificationList.GetItemAt(_viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel;
                var itemClone = new SymbolSpecificationViewModel(spec.GetSymbolSpecification(), _notificationService);

                var dialog = new SymbolSpecificationDialog(itemClone);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    spec.ModifierList = itemClone.ModifierList;
                    spec.SymbolKindList = itemClone.SymbolKindList;
                    spec.AccessibilityList = itemClone.AccessibilityList;
                    spec.SymbolSpecName = itemClone.SymbolSpecName;
                }
            }
        }

        private void CreateNamingStyle(object sender, RoutedEventArgs e)
        {
            var viewModel = new NamingStyleViewModel(new NamingStyle { ID = Guid.NewGuid() }, _notificationService);
            var dialog = new NamingStyleDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _outerViewModel.AddNamingSpec(viewModel);
                _viewModel.NamingStyleIndex = _viewModel.NamingStyleList.IndexOf(viewModel);
            }
        }

        private void ConfigureNamingStyles(object sender, RoutedEventArgs e)
        {
            if (_viewModel.NamingStyleIndex >= 0)
            {
                var namingStyleMutable = _viewModel.NamingStyleList.GetItemAt(_viewModel.NamingStyleIndex) as NamingStyleViewModel;

                var style = namingStyleMutable.GetNamingStyle();
                var styleClone = new NamingStyle
                {
                    ID = style.ID,
                    Name = style.Name,
                    CapitalizationScheme = style.CapitalizationScheme,
                    Prefix = style.Prefix,
                    Suffix = style.Suffix,
                    WordSeparator = style.WordSeparator
                };

                var itemClone = new NamingStyleViewModel(styleClone, _notificationService);

                var dialog = new NamingStyleDialog(itemClone);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    namingStyleMutable.NamingConventionName = itemClone.NamingConventionName;
                    namingStyleMutable.RequiredPrefix = itemClone.RequiredPrefix;
                    namingStyleMutable.RequiredSuffix = itemClone.RequiredSuffix;
                    namingStyleMutable.WordSeparator = itemClone.WordSeparator;
                    namingStyleMutable.FirstWordGroupCapitalization = itemClone.FirstWordGroupCapitalization;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

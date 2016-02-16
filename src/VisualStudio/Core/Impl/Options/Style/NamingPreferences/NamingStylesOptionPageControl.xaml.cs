// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Shell;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    /// <summary>
    /// Interaction logic for NamingPreferencesDialog.xaml
    /// </summary>
    internal partial class NamingStylesOptionPageControl : AbstractOptionPageControl
    {
        private readonly NamingStylesOptionPageControlViewModel _viewModel;
        private string _languageName;
        private readonly INotificationService _notificationService;

        internal NamingStylesOptionPageControl(IServiceProvider serviceProvider, string languageName)
            : base(serviceProvider)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            var workspace = componentModel.GetService<VisualStudioWorkspace>() as VisualStudioWorkspace;
            var notificationService = workspace.Services.GetService<INotificationService>();
            this._notificationService = notificationService;

            _languageName = languageName;

            DataContractSerializer ser = new DataContractSerializer(typeof(SerializableNamingStylePreferencesInfo));
            var currentValue = this.OptionService.GetOption(SimplificationOptions.NamingPreferences, _languageName);
            SerializableNamingStylePreferencesInfo info;

            if (string.IsNullOrEmpty(currentValue))
            {
                info = new SerializableNamingStylePreferencesInfo();
            }
            else
            {
                var reader = XmlReader.Create(new StringReader(currentValue));
                info = ser.ReadObject(reader) as SerializableNamingStylePreferencesInfo;
            }

            var viewModel = new NamingStylesOptionPageControlViewModel(info, notificationService);
            _viewModel = viewModel;

            InitializeComponent();
            this.DataContext = viewModel;

            this.RootTreeView.ItemsPath = nameof(NamingRuleTreeItemViewModel.Children);
            this.RootTreeView.IsExpandablePath = nameof(NamingRuleTreeItemViewModel.HasChildren);
            this.RootTreeView.FilterParentEvaluator = GetParent;
            this.RootTreeView.RootItemsSource = new ObservableCollection<NamingRuleTreeItemViewModel>() { viewModel._root };

            this.AddHandler(UIElementDialogPage.DialogKeyPendingEvent, (RoutedEventHandler)OnDialogKeyPending);
        }

        private void OnDialogKeyPending(object sender, RoutedEventArgs e)
        {
            // Don't let Escape or Enter close the Options page while we're renaming.
            if (this.RootTreeView.IsInRenameMode)
            {
                e.Handled = true;
            }
        }

        private IEnumerable<object> GetParent(object item)
        {
            NamingRuleTreeItemViewModel viewModel = item as NamingRuleTreeItemViewModel;
            if (viewModel != null && viewModel.Parent != null)
            {
                yield return viewModel.Parent;
            }
        }

        private void EnsureAncestorsExpanded(NamingRuleTreeItemViewModel item)
        {
            Stack<NamingRuleTreeItemViewModel> models = new Stack<NamingRuleTreeItemViewModel>();
            NamingRuleTreeItemViewModel iter = item.Parent;
            while (iter != null)
            {
                models.Push(iter);
                iter = iter.Parent;
            }

            while (models.Count > 0)
            {
                NamingRuleTreeItemViewModel manager = models.Pop();
                IVirtualizingTreeNode managerNode = this.RootTreeView.GetFirstTreeNode(manager);
                managerNode.IsExpanded = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = new SymbolSpecificationViewModel(_notificationService);
            var dialog = new SymbolSpecificationDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddSymbolSpec(viewModel);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var viewModel = new NamingStyleViewModel(new CodeAnalysis.Diagnostics.Analyzers.NamingStyle { ID = Guid.NewGuid() }, _notificationService);
            var dialog = new NamingStyleDialog(viewModel);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddNamingSpec(viewModel);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //var selectedSpecification = SymbolSpecificationList.SelectedItem as SymbolSpecificationViewModel;
            //var selectedStyle = NamingConventionList.SelectedItem as NamingStyleViewModel;

            var viewModel = new NamingRuleDialogViewModel(
                string.Empty,
                null,
                _viewModel.SymbolSpecificationList,
                null,
                _viewModel.NamingStyleList,
                null,
                _viewModel.CreateAllowableParentList(_viewModel._root),
                new EnforcementLevel("None", DiagnosticSeverity.Hidden, KnownMonikers.None),
                _notificationService);
            var dialog = new NamingRuleDialog(viewModel, _viewModel, _notificationService);
            var result = dialog.ShowModal();
            if (result == true)
            {
                _viewModel.AddNamingPreference(viewModel);
            }
        }

        private void Delete_3(object sender, RoutedEventArgs e)
        {
            var a = RootTreeView.SelectedIndex; ;
            if (a > 0)
            {
                _viewModel.DeleteRuleAtIndex(a);
            }
        }

        private void NamingConventionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as NamingStyleViewModel;
            if (item != null)
            {
                var style = item.GetNamingStyle();
                var styleClone = new NamingStyle
                {
                    ID = style.ID,
                    Name = style.Name,
                    CapitalizationScheme = style.CapitalizationScheme,
                    Prefix = style.Prefix,
                    Suffix = style.Suffix,
                    WordSeparator = style.WordSeparator
                };

                var itemClone = new NamingStyleViewModel(styleClone, item._notificationService);

                var dialog = new NamingStyleDialog(itemClone);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    item.NamingConventionName = itemClone.NamingConventionName;
                    item.RequiredPrefix = itemClone.RequiredPrefix;
                    item.RequiredSuffix = itemClone.RequiredSuffix;
                    item.WordSeparator = itemClone.WordSeparator;
                    item.FirstWordGroupCapitalization = itemClone.FirstWordGroupCapitalization;
                }
            }
        }

        private void SymbolSpecificationList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as SymbolSpecificationViewModel;
            if (item != null)
            {
                var itemClone = new SymbolSpecificationViewModel(item.GetSymbolSpecification(), _notificationService);

                var dialog = new SymbolSpecificationDialog(itemClone);
                var result = dialog.ShowModal();
                if (result == true)
                {
                    item.ModifierList = itemClone.ModifierList;
                    item.SymbolKindList = itemClone.SymbolKindList;
                    item.AccessibilityList = itemClone.AccessibilityList;
                    item.SymbolSpecName = itemClone.SymbolSpecName;
                }
            }
        }

        public void KeyCommand(object sender, KeyEventArgs e)
        {
        }

        internal override void SaveSettings()
        {
            base.SaveSettings();

            var info = _viewModel.GetInfo();
            DataContractSerializer ser = new DataContractSerializer(typeof(SerializableNamingStylePreferencesInfo));

            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) { Formatting = System.Xml.Formatting.Indented })
            {
                ser.WriteObject(writer, info);
                var resultingXml = output.GetStringBuilder().ToString();
                var oldOptions = OptionService.GetOptions();
                var newOptions = oldOptions.WithChangedOption(SimplificationOptions.NamingPreferences, _languageName, resultingXml);

                OptionService.SetOptions(newOptions);
                OptionLogger.Log(oldOptions, newOptions);
            }
        }

        internal override void LoadSettings()
        {
            base.LoadSettings();
        }
    }
}

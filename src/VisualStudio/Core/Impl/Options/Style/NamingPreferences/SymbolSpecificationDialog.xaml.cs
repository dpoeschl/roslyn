// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal partial class SymbolSpecificationDialog : DialogWindow
    {
        private readonly SymbolSpecificationViewModel _viewModel;

        internal SymbolSpecificationDialog(SymbolSpecificationViewModel viewModel)
            : base(helpTopic: "TODO")
        {
            _viewModel = viewModel;
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

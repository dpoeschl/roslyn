// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    /// <summary>
    /// Interaction logic for NamingStyleDialog.xaml
    /// </summary>
    internal partial class NamingStyleDialog : DialogWindow
    {
        private readonly NamingStyleViewModel _viewModel;

        internal NamingStyleDialog(NamingStyleViewModel viewModel)
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

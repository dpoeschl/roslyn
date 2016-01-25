// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    /// <summary>
    /// Interaction logic for ExtractInterfaceDialog.xaml
    /// </summary>
    internal partial class ClassificationRuleDialog : DialogWindow
    {
        private readonly ClassificationRuleDialogViewModel _viewModel;

        internal ClassificationRuleDialog(ClassificationRuleDialogViewModel viewModel)
            : base(helpTopic: "TODO")
        {
            _viewModel = viewModel;
            // SetCommandBindings();

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

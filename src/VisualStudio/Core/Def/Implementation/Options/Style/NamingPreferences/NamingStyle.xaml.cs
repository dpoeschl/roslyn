// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    /// <summary>
    /// Interaction logic for ExtractInterfaceDialog.xaml
    /// </summary>
    internal partial class NamingStyle : DialogWindow
    {
        private readonly NamingStyleViewModel _viewModel;

        internal NamingStyle(NamingStyleViewModel viewModel)
            : base(helpTopic: "TODO")
        {
            _viewModel = viewModel;
            // SetCommandBindings();

            InitializeComponent();
            DataContext = viewModel;

            Loaded += ExtractInterfaceDialog_Loaded;
        }

        private void ExtractInterfaceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //interfaceNameTextBox.Focus();
            //interfaceNameTextBox.SelectAll();
        }

        //private void SetCommandBindings()
        //{
        //    CommandBindings.Add(new CommandBinding(
        //        new RoutedCommand(
        //            "SelectAllClickCommand",
        //            typeof(ExtractInterfaceDialog),
        //            new InputGestureCollection(new List<InputGesture> { new KeyGesture(Key.S, ModifierKeys.Alt) })),
        //        Select_All_Click));

        //    CommandBindings.Add(new CommandBinding(
        //        new RoutedCommand(
        //            "DeselectAllClickCommand",
        //            typeof(ExtractInterfaceDialog),
        //            new InputGestureCollection(new List<InputGesture> { new KeyGesture(Key.D, ModifierKeys.Alt) })),
        //        Deselect_All_Click));
        //}

        //private void OK_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_viewModel.TrySubmit())
        //    {
        //        DialogResult = true;
        //    }
        //}

        //private void Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}

    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    partial class NamingRuleTreeItemViewModel : IDragDropSourcePattern
    {
        public IDragDropSourceController DragDropSourceController
        {
            get { return NamingRuleTreeItemDragDropSourceController.Instance; }
        }

        private class NamingRuleTreeItemDragDropSourceController : IDragDropSourceController
        {
            private static NamingRuleTreeItemDragDropSourceController instance;
            public static NamingRuleTreeItemDragDropSourceController Instance
            {
                get { return instance ?? (instance = new NamingRuleTreeItemDragDropSourceController()); }
            }

            public bool DoDragDrop(IEnumerable<object> items)
            {
                // Only support drag/drop on a single item for simplicity
                if (items.Count() != 1)
                {
                    return false;
                }

                DependencyObject dragSource = (Keyboard.FocusedElement as DependencyObject) ?? Application.Current.MainWindow;
                DragDrop.DoDragDrop(dragSource, items.Single(), DragDropEffects.All);
                return true;
            }
        }
    }
}

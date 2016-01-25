using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    partial class ClassificationStyleTreeViewModel : IDragDropSourcePattern
    {
        public IDragDropSourceController DragDropSourceController
        {
            get
            {
                return NamingRuleDragDropSourceController.Instance;
            }
        }

        private class NamingRuleDragDropSourceController : IDragDropSourceController
        {
            private static NamingRuleDragDropSourceController instance;

            public static NamingRuleDragDropSourceController Instance
            {
                get { return instance ?? (instance = new NamingRuleDragDropSourceController()); }
            }

            public bool DoDragDrop(IEnumerable<object> items)
            {
                if (items.Count() == 1)
                {
                    DependencyObject dragSource = (Keyboard.FocusedElement as DependencyObject) ?? Application.Current.MainWindow;
                    DragDrop.DoDragDrop(dragSource, items.First(), DragDropEffects.All);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

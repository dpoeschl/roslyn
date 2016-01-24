using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OrgChart.Sample
{
    partial class PersonViewModel : IDragDropSourcePattern
    {
        public IDragDropSourceController DragDropSourceController
        {
            get
            {
                return PersonDragDropSourceController.Instance;
            }
        }

        private class PersonDragDropSourceController : IDragDropSourceController
        {
            private static PersonDragDropSourceController instance;

            public static PersonDragDropSourceController Instance
            {
                get { return instance ?? (instance = new PersonDragDropSourceController()); }
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

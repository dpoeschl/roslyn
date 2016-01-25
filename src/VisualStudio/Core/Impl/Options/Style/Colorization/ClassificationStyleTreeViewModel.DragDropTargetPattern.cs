using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    partial class ClassificationStyleTreeViewModel : IDragDropTargetPattern
    {
        public DirectionalDropArea SupportedAreas
        {
            get
            {
                return DirectionalDropArea.Above | DirectionalDropArea.Below | DirectionalDropArea.On;
            }
        }

        public void OnDragEnter(DirectionalDropArea dropArea, DragEventArgs e)
        {
            UpdateAllowedEffects(dropArea, e);
        }

        private void UpdateAllowedEffects(DirectionalDropArea dropArea, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ClassificationStyleTreeViewModel)))
            {
                var person = e.Data.GetData(typeof(ClassificationStyleTreeViewModel)) as ClassificationStyleTreeViewModel;
                if (person != null && IsDropAllowed(dropArea, this, person))
                {
                    e.Effects = DragDropEffects.All;
                }
            }
        }

        public void OnDragLeave(DirectionalDropArea dropArea, DragEventArgs e)
        {
            
        }

        public void OnDragOver(DirectionalDropArea dropArea, DragEventArgs e)
        {
            UpdateAllowedEffects(dropArea, e);
        }

        public void OnDrop(DirectionalDropArea dropArea, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ClassificationStyleTreeViewModel)))
            {
                var model = e.Data.GetData(typeof(ClassificationStyleTreeViewModel)) as ClassificationStyleTreeViewModel;
                if (!model.IsAncestorOfMe(this))
                {
                    switch (dropArea)
                    {
                        case DirectionalDropArea.On:
                            model.Parent.Children.Remove(model);
                            this.Children.Add(model);
                            break;
                        case DirectionalDropArea.Above:
                            model.Parent.Children.Remove(model);
                            this.Parent.Children.Insert(this.Parent.Children.IndexOf(this), model);
                            break;
                        case DirectionalDropArea.Below:
                            model.Parent.Children.Remove(model);
                            this.Parent.Children.Insert(this.Parent.Children.IndexOf(this) + 1, model);
                            break;
                    }
                }
            }
        }

        private static bool IsDropAllowed(DirectionalDropArea dropArea, ClassificationStyleTreeViewModel target, ClassificationStyleTreeViewModel source)
        {
            switch (dropArea)
            {
                case DirectionalDropArea.On:
                    // People can't be dropped on themselves, or on any of their reports.
                    if (source == target || source.IsAncestorOfMe(target))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case DirectionalDropArea.Above:
                case DirectionalDropArea.Below:
                    // People can't be dropped on themselves, or on any of their reports.
                    // The head of the organization can't have peers.
                    if (source == target || source.IsAncestorOfMe(target))
                    {
                        return false;
                    }

                    // The head of the organization can never have peers
                    if (target.Parent == null)
                    {
                        return false;
                    }

                    // Insertions that would lead to the same order don't make sense
                    int direction = dropArea == DirectionalDropArea.Above ? -1 : 1;
                    if (source.Parent == target.Parent && source.Parent.Children.IndexOf(source) == target.Parent.Children.IndexOf(target) + direction)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }
    }
}

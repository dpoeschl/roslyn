using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OrgChart.Sample
{
    partial class PersonViewModel : IDragDropTargetPattern
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
            if (e.Data.GetDataPresent(typeof(PersonViewModel)))
            {
                PersonViewModel person = e.Data.GetData(typeof(PersonViewModel)) as PersonViewModel;
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
            if (e.Data.GetDataPresent(typeof(PersonViewModel)))
            {
                PersonViewModel model = e.Data.GetData(typeof(PersonViewModel)) as PersonViewModel;
                if (!model.IsInManagementChain(this))
                {
                    switch (dropArea)
                    {
                        case DirectionalDropArea.On:
                            model.Manager.Reports.Remove(model);
                            this.Reports.Add(model);
                            break;
                        case DirectionalDropArea.Above:
                            model.Manager.Reports.Remove(model);
                            this.Manager.Reports.Insert(this.Manager.Reports.IndexOf(this), model);
                            break;
                        case DirectionalDropArea.Below:
                            model.Manager.Reports.Remove(model);
                            this.Manager.Reports.Insert(this.Manager.Reports.IndexOf(this) + 1, model);
                            break;
                    }
                }
            }
        }

        private static bool IsDropAllowed(DirectionalDropArea dropArea, PersonViewModel target, PersonViewModel source)
        {
            switch (dropArea)
            {
                case DirectionalDropArea.On:
                    // People can't be dropped on themselves, or on any of their reports.
                    if (source == target || source.IsInManagementChain(target))
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
                    if (source == target || source.IsInManagementChain(target))
                    {
                        return false;
                    }

                    // The head of the organization can never have peers
                    if (target.Manager == null)
                    {
                        return false;
                    }

                    // Insertions that would lead to the same order don't make sense
                    int direction = dropArea == DirectionalDropArea.Above ? -1 : 1;
                    if (source.Manager == target.Manager && source.Manager.Reports.IndexOf(source) == target.Manager.Reports.IndexOf(target) + direction)
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

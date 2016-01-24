//------------------------------------------------------------------------------
// <copyright file="OrgChartToolWindowPaneControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace OrgChart
{
    using Microsoft.Internal.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Sample;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OrgChartToolWindowPaneControl.
    /// </summary>
    public partial class OrgChartToolWindowPaneControl : UserControl
    {
        private PersonViewModel philSpencer;
        private PersonViewModel virtualRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrgChartToolWindowPaneControl"/> class.
        /// </summary>
        public OrgChartToolWindowPaneControl()
        {
            this.InitializeComponent();

            philSpencer = new PersonViewModel("Phil Spencer");
            virtualRoot = new PersonViewModel("<virtual>")
            {
                Reports =
                {
                    new PersonViewModel("Satya Nadella")
                    {
                        Reports =
                        {
                            new PersonViewModel("Terry Myerson")
                            {
                                Reports =
                                {
                                    new PersonViewModel("Kudo Tsunoda"),
                                    philSpencer
                                }
                            },

                            new PersonViewModel("Scott Guthrie")
                        }
                    }
                }
            };

            this.RootTreeView.ItemsPath = "Reports";
            this.RootTreeView.IsExpandablePath = "HasReports";
            this.RootTreeView.FilterParentEvaluator = GetParent;
            this.RootTreeView.RootItemsSource = virtualRoot.Reports;

            this.RootTreeView.DragOver += RootTreeView_DragOver;
            this.RootTreeView.Drop += RootTreeView_Drop;
        }

        private void RootTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PersonViewModel)))
            {
                e.Effects = DragDropEffects.All;
                e.Handled = true;
            }
        }

        private void RootTreeView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PersonViewModel)))
            {
                e.Handled = true;
                PersonViewModel person = e.Data.GetData(typeof(PersonViewModel)) as PersonViewModel;
                if (person != null && person.Manager != null)
                {
                    person.Manager.Reports.Remove(person);
                    virtualRoot.Reports.Add(person);
                }
            }
        }

        private IEnumerable<object> GetParent(object item)
        {
            PersonViewModel viewModel = item as PersonViewModel;
            if (viewModel != null && viewModel.Manager != null)
            {
                yield return viewModel.Manager;
            }
        }

        private void EnsureAncestorsExpanded(PersonViewModel item)
        {
            Stack<PersonViewModel> models = new Stack<PersonViewModel>();
            PersonViewModel iter = item.Manager;
            while (iter != null)
            {
                models.Push(iter);
                iter = iter.Manager;
            }

            while (models.Count > 0)
            {
                PersonViewModel manager = models.Pop();
                IVirtualizingTreeNode managerNode = this.RootTreeView.GetFirstTreeNode(manager);
                if (managerNode != null)
                {
                    managerNode.IsExpanded = true;
                }
            }
        }

        private void OnSelectPhilSpencerClicked(object sender, RoutedEventArgs e)
        {
            EnsureAncestorsExpanded(this.philSpencer);
            IVirtualizingTreeNode philSpencerNode = this.RootTreeView.GetFirstTreeNode(this.philSpencer);
            this.RootTreeView.ChangeSelection(philSpencerNode, VirtualizingTreeSelectionAction.SingleSelection);
            this.RootTreeView.FocusSelectedItem();
        }

        private void OnShowSelectionInfoClicked(object sender, RoutedEventArgs e)
        {
            IEnumerable<string> selectedPeople = this.RootTreeView.GetDistinctSelection<PersonViewModel>().Select(m => m.Name);
            VsShellUtilities.ShowMessageBox(
                ServiceProvider.GlobalProvider,
                $"You've selected:\r\n{string.Join("\r\n", selectedPeople)}",
                null,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
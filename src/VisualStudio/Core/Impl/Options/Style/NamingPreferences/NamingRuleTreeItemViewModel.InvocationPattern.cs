// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    partial class NamingRuleTreeItemViewModel : IInvocationPattern
    {
        public bool CanPreview
        {
            get { return false; }
        }

        public IInvocationController InvocationController
        {
            get { return PersonInvocationController.Instance; }
        }

        private class PersonInvocationController : IInvocationController
        {
            private static PersonInvocationController instance;

            public static PersonInvocationController Instance
            {
                get { return instance ?? (instance = new PersonInvocationController()); }
            }

            public bool Invoke(IEnumerable<object> items, InputSource inputSource, bool preview)
            {
                NamingRuleTreeItemViewModel[] selectedRules = items.OfType<NamingRuleTreeItemViewModel>().ToArray();

                // When a single Naming Rule is invoked using the mouse, expanding/collapse it in the tree
                if (selectedRules.Length == 1)
                {
                    var selectedRule = selectedRules[0];
                    if (selectedRule.VM == null)
                    {
                        return false;
                    }

                    var viewModel = new NamingRuleDialogViewModel(
                        selectedRule.name,
                        selectedRule.symbolSpec,
                        selectedRule.VM.SymbolSpecificationList,
                        selectedRule.namingStyle,
                        selectedRule.VM.NamingStyleList,
                        selectedRule.parent,
                        selectedRule.VM.CreateAllowableParentList(selectedRule),
                        selectedRule.EnforcementLevel,
                        selectedRule.VM._notificationService);
                    var dialog = new NamingRuleDialog(viewModel, selectedRule.VM, selectedRule.VM._notificationService);
                    var result = dialog.ShowModal();
                    if (result == true)
                    {
                        selectedRule.namingStyle = viewModel.NamingStyleList.GetItemAt(viewModel.NamingStyleIndex) as NamingStyleViewModel;
                        selectedRule.symbolSpec = viewModel.SymbolSpecificationList.GetItemAt(viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel;
                        selectedRule.Title = viewModel.Title;
                        selectedRule.EnforcementLevel = viewModel.EnforcementLevelsList[viewModel.EnforcementLevelIndex];
                        selectedRule.NotifyPropertyChanged(nameof(selectedRule.Text));

                        if (viewModel.ParentRuleIndex == 0)
                        {
                            if (selectedRule.Parent != selectedRule.VM._root)
                            {
                                selectedRule.Parent.Children.Remove(selectedRule);
                                selectedRule.VM._root.Children.Add(selectedRule);
                            }
                        }
                        else
                        {
                            var newParent = viewModel.ParentRuleList.GetItemAt(viewModel.ParentRuleIndex) as NamingRuleTreeItemViewModel;
                            if (newParent != selectedRule.Parent)
                            {
                                selectedRule.Parent.Children.Remove(selectedRule);
                                newParent.Children.Add(selectedRule);
                            }
                        }
                    }

                    return true;
                }

                return false;
            }
        }
    }
}

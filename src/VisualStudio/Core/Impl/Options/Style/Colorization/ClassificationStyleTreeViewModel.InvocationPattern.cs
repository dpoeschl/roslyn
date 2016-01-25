using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    partial class ClassificationStyleTreeViewModel : IInvocationPattern
    {
        public bool CanPreview
        {
            get
            {
                return false;
            }
        }

        public IInvocationController InvocationController
        {
            get
            {
                return PersonInvocationController.Instance;
            }
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
                ClassificationStyleTreeViewModel[] selectedRules = items.OfType<ClassificationStyleTreeViewModel>().ToArray();

                // When a single person is invoked using the mouse, do the default action of expanding/collapsing the person in the tree
                if (selectedRules.Length == 1) // && people[0].HasChildren && inputSource == InputSource.Mouse)
                {
                    var selectedRule = selectedRules[0];

                    if (selectedRule.VM == null)
                    {
                        return false;
                    }

                    var viewModel = new ClassificationRuleDialogViewModel(
                        selectedRule.name,
                        selectedRule.symbolSpec,
                        selectedRule.VM.SymbolSpecificationList,
                        selectedRule.namingStyle,
                        selectedRule.VM.NamingStyleList,
                        selectedRule.parent,
                        selectedRule.VM.CreateAllowableParentList(selectedRule));
                    var dialog = new ClassificationRuleDialog(viewModel);
                    var result = dialog.ShowModal();
                    if (result == true)
                    {
                        selectedRule.namingStyle = viewModel.NamingStyleList.GetItemAt(viewModel.NamingStyleIndex) as ClassificationStyleViewModel;
                        selectedRule.symbolSpec = viewModel.SymbolSpecificationList.GetItemAt(viewModel.SelectedSymbolSpecificationIndex) as SymbolSpecificationViewModel;
                        selectedRule.Title = viewModel.Title;

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
                            var newParent = viewModel.ParentRuleList.GetItemAt(viewModel.ParentRuleIndex) as ClassificationStyleTreeViewModel;
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

using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgChart.Sample
{
    partial class PersonViewModel : IInvocationPattern
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
                PersonViewModel[] people = items.OfType<PersonViewModel>().ToArray();

                // When a single person is invoked using the mouse, do the default action of expanding/collapsing the person in the tree
                if (people.Length == 1 && people[0].HasReports && inputSource == InputSource.Mouse)
                {
                    return false;
                }

                VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, "You selected: " + string.Join(", ", items.OfType<PersonViewModel>().Select(p => p.Name)), "Org Chart", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return true;
            }
        }
    }
}

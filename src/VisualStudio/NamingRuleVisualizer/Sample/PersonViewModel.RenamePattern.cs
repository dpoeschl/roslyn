using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgChart.Sample
{
    partial class PersonViewModel : IRenamePattern
    {
        public bool CanRename
        {
            get
            {
                return true;
            }
        }

        public IRenameItemTransaction BeginRename(object container, Func<IRenameItemTransaction, IRenameItemValidationResult> validator)
        {
            return new PersonRenameTransaction(this, container, validator);
        }

        private class PersonRenameTransaction : RenameItemTransaction
        {
            public PersonRenameTransaction(PersonViewModel viewModel, object container, Func<IRenameItemTransaction, IRenameItemValidationResult> validator)
                : base(viewModel, container, validator)
            {
                this.RenameLabel = viewModel.Text;
                this.Completed += (s, e) =>
                {
                    viewModel.Name = this.RenameLabel;
                };
            }
        }
    }
}

using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgChart.Sample
{
    internal partial class PersonViewModel : ObservableObject
    {
        private readonly ReportCollection reports;
        private PersonViewModel manager;
        private string name;
        private bool hasReports;

        public PersonViewModel(string name)
        {
            this.name = name;
            this.reports = new ReportCollection(this);
            this.reports.CollectionChanged += OnReportsCollectionChanged;
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value, () => NotifyPropertyChanged(nameof(ITreeDisplayItem.Text)));
            }
        }

        public PersonViewModel Manager
        {
            get
            {
                return this.manager;
            }
            private set
            {
                this.SetProperty(ref this.manager, value);
            }
        }

        public IList<PersonViewModel> Reports
        {
            get { return this.reports; }
        }

        public bool HasReports
        {
            get { return this.reports.Count > 0; }
            private set
            {
                this.SetProperty(ref this.hasReports, value);
            }
        }

        private void OnReportsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasReports = this.reports.Count > 0;
        }

        public bool IsInManagementChain(PersonViewModel person)
        {
            PersonViewModel iter = person.Manager;
            while (iter != null)
            {
                if (iter == this)
                {
                    return true;
                }
                iter = iter.Manager;
            }

            return false;
        }

        private class ReportCollection : ObservableCollection<PersonViewModel>
        {
            private readonly PersonViewModel owner;

            public ReportCollection(PersonViewModel owner)
            {
                this.owner = owner;
            }

            protected override void InsertItem(int index, PersonViewModel item)
            {
                base.InsertItem(index, item);
                this.TakeOwnership(item);
            }

            protected override void RemoveItem(int index)
            {
                PersonViewModel item = this[index];
                base.RemoveItem(index);
                this.LoseOwnership(item);
            }

            protected override void SetItem(int index, PersonViewModel item)
            {
                PersonViewModel oldItem = this[index];
                base.SetItem(index, item);
                this.LoseOwnership(oldItem);
                this.TakeOwnership(item);
            }

            protected override void ClearItems()
            {
                List<PersonViewModel> list = new List<PersonViewModel>(this);
                base.ClearItems();
                foreach (PersonViewModel item in list)
                {
                    this.LoseOwnership(item);
                }
            }

            private void TakeOwnership(PersonViewModel item)
            {
                item.Manager = this.owner;
            }

            private void LoseOwnership(PersonViewModel item)
            {
                item.Manager = null;
            }
        }
    }
}

using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    public partial class NamingRuleTreeViewModel : ObservableObject
    {
        internal NamingRuleTreeViewModel(string name)
        {
            // TODO Remove constructor
            this.name = name;
            this.children = new RuleSpecifierCollection(this);
            this.children.CollectionChanged += OnChildrenCollectionChanged;
        }

        internal NamingRuleTreeViewModel(
            string name,
            SymbolSpecificationViewModel symbolSpec, 
            NamingStyleViewModel namingStyle,
            NamingPreferencesDialogViewModel vm)
        {
            this.name = name;
            this.symbolSpec = symbolSpec;
            this.namingStyle = namingStyle;
            this._vm = vm;

            this.children = new RuleSpecifierCollection(this);
            this.children.CollectionChanged += OnChildrenCollectionChanged;
        }

        private string name;
        public string NamingRuleTitle
        {
            get { return name; }
            set { name = value; }
        }

        private NamingRuleTreeViewModel parent;
        private readonly RuleSpecifierCollection children;
        private bool hasChildren;

        internal readonly SymbolSpecificationViewModel symbolSpec;
        internal NamingStyleViewModel namingStyle;

        public string EnforcementLevel { get; set; }

        public string Title
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value, () => NotifyPropertyChanged(nameof(ITreeDisplayItem.Text)));
            }
        }

        public NamingRuleTreeViewModel Parent
        {
            get
            {
                return this.parent;
            }
            private set
            {
                this.SetProperty(ref this.parent, value);
            }
        }

        public IList<NamingRuleTreeViewModel> Children
        {
            get { return this.children; }
        }

        public bool HasChildren
        {
            get { return this.children.Count > 0; }
            private set
            {
                this.SetProperty(ref this.hasChildren, value);
            }
        }

        internal bool TrySubmit()
        {
            return true;
        }

        private NamingPreferencesDialogViewModel _vm;
        internal NamingPreferencesDialogViewModel VM
        {
            get
            {
                return _vm;
            }
            set
            {

            }
        }
        
        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasChildren = this.children.Count > 0;
        }

        public bool IsAncestorOfMe(NamingRuleTreeViewModel rule)
        {
            NamingRuleTreeViewModel potentialAncestor = rule.Parent;
            while (potentialAncestor != null)
            {
                if (potentialAncestor == this)
                {
                    return true;
                }

                potentialAncestor = potentialAncestor.Parent;
            }

            return false;
        }

        private class RuleSpecifierCollection : ObservableCollection<NamingRuleTreeViewModel>
        {
            private readonly NamingRuleTreeViewModel owner;

            public RuleSpecifierCollection() : this(null)
            {

            }

            public RuleSpecifierCollection(NamingRuleTreeViewModel owner)
            {
                this.owner = owner;
            }

            protected override void InsertItem(int index, NamingRuleTreeViewModel item)
            {
                base.InsertItem(index, item);
                this.TakeOwnership(item);
            }

            protected override void RemoveItem(int index)
            {
                NamingRuleTreeViewModel item = this[index];
                base.RemoveItem(index);
                this.LoseOwnership(item);
            }

            protected override void SetItem(int index, NamingRuleTreeViewModel item)
            {
                NamingRuleTreeViewModel oldItem = this[index];
                base.SetItem(index, item);
                this.LoseOwnership(oldItem);
                this.TakeOwnership(item);
            }

            protected override void ClearItems()
            {
                List<NamingRuleTreeViewModel> list = new List<NamingRuleTreeViewModel>(this);
                base.ClearItems();
                foreach (NamingRuleTreeViewModel item in list)
                {
                    this.LoseOwnership(item);
                }
            }

            private void TakeOwnership(NamingRuleTreeViewModel item)
            {
                item.Parent = this.owner;
            }

            private void LoseOwnership(NamingRuleTreeViewModel item)
            {
                item.Parent = null;
            }

        }
    }
}

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

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    public partial class ClassificationStyleTreeViewModel : ObservableObject
    {
        public ClassificationStyleTreeViewModel(string name)
        {
            // TODO Remove constructor
            this.name = name;
            this.children = new RuleSpecifierCollection(this);
            this.children.CollectionChanged += OnChildrenCollectionChanged;
        }

        internal ClassificationStyleTreeViewModel(
            string name,
            SymbolSpecificationViewModel symbolSpec, 
            ClassificationStyleViewModel namingStyle,
            ClassificaitonDialogViewModel vm)
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

        private ClassificationStyleTreeViewModel parent;
        private readonly RuleSpecifierCollection children;
        private bool hasChildren;

        internal SymbolSpecificationViewModel symbolSpec;
        internal ClassificationStyleViewModel namingStyle;

        public string EnforcementLevel { get; set; }

        public string Title
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value, () => NotifyPropertyChanged(nameof(ITreeDisplayItem.Text)));
            }
        }

        public ClassificationStyleTreeViewModel Parent
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

        public IList<ClassificationStyleTreeViewModel> Children
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

        private ClassificaitonDialogViewModel _vm;
        internal ClassificaitonDialogViewModel VM
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

        public bool IsAncestorOfMe(ClassificationStyleTreeViewModel rule)
        {
            var potentialAncestor = rule.Parent;
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

        private class RuleSpecifierCollection : ObservableCollection<ClassificationStyleTreeViewModel>
        {
            private readonly ClassificationStyleTreeViewModel owner;

            public RuleSpecifierCollection() : this(null)
            {

            }

            public RuleSpecifierCollection(ClassificationStyleTreeViewModel owner)
            {
                this.owner = owner;
            }

            protected override void InsertItem(int index, ClassificationStyleTreeViewModel item)
            {
                base.InsertItem(index, item);
                this.TakeOwnership(item);
            }

            protected override void RemoveItem(int index)
            {
                var item = this[index];
                base.RemoveItem(index);
                this.LoseOwnership(item);
            }

            protected override void SetItem(int index, ClassificationStyleTreeViewModel item)
            {
                var oldItem = this[index];
                base.SetItem(index, item);
                this.LoseOwnership(oldItem);
                this.TakeOwnership(item);
            }

            protected override void ClearItems()
            {
                var list = new List<ClassificationStyleTreeViewModel>(this);
                base.ClearItems();
                foreach (var item in list)
                {
                    this.LoseOwnership(item);
                }
            }

            private void TakeOwnership(ClassificationStyleTreeViewModel item)
            {
                item.Parent = this.owner;
            }

            private void LoseOwnership(ClassificationStyleTreeViewModel item)
            {
                item.Parent = null;
            }

        }
    }
}

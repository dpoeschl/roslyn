// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    public partial class NamingRuleTreeItemViewModel : ObservableObject
    {
        internal NamingRuleTreeItemViewModel(string name)
        {
            // TODO: remove this
            this.name = name;
            this.children = new RuleSpecifierCollection(this);
            this.children.CollectionChanged += OnChildrenCollectionChanged;
        }

        internal NamingRuleTreeItemViewModel(
            string name,
            SymbolSpecificationViewModel symbolSpec, 
            NamingStyleViewModel namingStyle,
            EnforcementLevel enforcementLevel,
            NamingStylesOptionPageControlViewModel vm)
        {
            this.EnforcementLevel = enforcementLevel;
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

        private NamingRuleTreeItemViewModel parent;
        private readonly RuleSpecifierCollection children;
        private bool hasChildren;

        internal SymbolSpecificationViewModel symbolSpec;
        internal NamingStyleViewModel namingStyle;

        public string Title
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value, () => NotifyPropertyChanged(nameof(ITreeDisplayItem.Text)));
            }
        }

        //public string EnforcementLevelSuffix
        //{
        //    get
        //    {
        //        if (Parent == null)
        //        {
        //            return string.Empty;
        //        }

        //        return EnforcementLevel.Value == CodeAnalysis.DiagnosticSeverity.Hidden
        //            ? string.Empty
        //            : $" [{EnforcementLevel.Name}]";
        //    }
        //}


        public NamingRuleTreeItemViewModel Parent
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

        public IList<NamingRuleTreeItemViewModel> Children
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

        private NamingStylesOptionPageControlViewModel _vm;
        private EnforcementLevel enforcementLevel;

        internal NamingStylesOptionPageControlViewModel VM
        {
            get
            {
                return _vm;
            }
            set
            {

            }
        }

        internal EnforcementLevel EnforcementLevel
        {
            get
            {
                return enforcementLevel;
            }

            set
            {
                if (SetProperty(ref enforcementLevel, value))
                {
                    NotifyPropertyChanged(nameof(IconMoniker));
                    NotifyPropertyChanged(nameof(ExpandedIconMoniker));
                }
            }
        }

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasChildren = this.children.Count > 0;
        }

        public bool IsAncestorOfMe(NamingRuleTreeItemViewModel rule)
        {
            NamingRuleTreeItemViewModel potentialAncestor = rule.Parent;
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

        private class RuleSpecifierCollection : ObservableCollection<NamingRuleTreeItemViewModel>
        {
            private readonly NamingRuleTreeItemViewModel owner;

            public RuleSpecifierCollection() : this(null)
            {

            }

            public RuleSpecifierCollection(NamingRuleTreeItemViewModel owner)
            {
                this.owner = owner;
            }

            protected override void InsertItem(int index, NamingRuleTreeItemViewModel item)
            {
                base.InsertItem(index, item);
                this.TakeOwnership(item);
            }

            protected override void RemoveItem(int index)
            {
                NamingRuleTreeItemViewModel item = this[index];
                base.RemoveItem(index);
                this.LoseOwnership(item);
            }

            protected override void SetItem(int index, NamingRuleTreeItemViewModel item)
            {
                NamingRuleTreeItemViewModel oldItem = this[index];
                base.SetItem(index, item);
                this.LoseOwnership(oldItem);
                this.TakeOwnership(item);
            }

            protected override void ClearItems()
            {
                List<NamingRuleTreeItemViewModel> list = new List<NamingRuleTreeItemViewModel>(this);
                base.ClearItems();
                foreach (NamingRuleTreeItemViewModel item in list)
                {
                    this.LoseOwnership(item);
                }
            }

            private void TakeOwnership(NamingRuleTreeItemViewModel item)
            {
                item.Parent = this.owner;
            }

            private void LoseOwnership(NamingRuleTreeItemViewModel item)
            {
                item.Parent = null;
            }

        }
    }
}

using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using System.Collections.Generic;
using System.Windows.Data;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal partial class NamingRuleDialogViewModel : AbstractNotifyPropertyChanged
    {
        public NamingRuleDialogViewModel(
            string name,
            SymbolSpecificationViewModel symbolSpecification,
            List<SymbolSpecificationViewModel> symbolSpecificationList,
            NamingStyleViewModel namingStyle,
            List<NamingStyleViewModel> namingStyleList,
            NamingRuleTreeViewModel parent,
            List<NamingRuleTreeViewModel> allowableParentList)
        {
            this.name = name;

            this.symbolSpec = symbolSpecification;
            this._symbolSpecificationList = new CollectionView(symbolSpecificationList);
            this._selectedSymbolSpecificationIndex = symbolSpecificationList.IndexOf(symbolSpec);

            this.namingStyle = namingStyle;
            this._namingStyleList = new CollectionView(namingStyleList);
            this._namingStyleIndex = namingStyleList.IndexOf(namingStyle);

            allowableParentList.Insert(0, new NamingRuleTreeViewModel("-- None --"));
            this._parentRuleList = new CollectionView(allowableParentList);
            this._parentRuleIndex = parent != null ? allowableParentList.IndexOf(parent) : 0;
            if (_parentRuleIndex < 0)
            {
                _parentRuleIndex = 0;
            }
        }

        private string name;
        public string NamingRuleTitle
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        public string Title
        {
            get { return this.name; }
            set
            {
                this.SetProperty(ref this.name, value);
            }
        }

        private readonly SymbolSpecificationViewModel symbolSpec;

        private CollectionView _symbolSpecificationList;
        public CollectionView SymbolSpecificationList
        {
            get
            {
                return _symbolSpecificationList;
            }
            set
            {

            }
        }

        private int _selectedSymbolSpecificationIndex;
        public int SelectedSymbolSpecificationIndex
        {
            get
            {
                return _selectedSymbolSpecificationIndex;
            }
            set
            {
                _selectedSymbolSpecificationIndex = value;
            }
        }

        private NamingStyleViewModel namingStyle;

        public string SymbolSpecificationName
        {
            get
            {
                return symbolSpec.SymbolSpecName;
            }
            set
            {
            }
        }

        private CollectionView _namingStyleList;
        public CollectionView NamingStyleList
        {
            get
            {
                return _namingStyleList;
            }
            set
            {

            }
        }

        private int _namingStyleIndex;
        public int NamingStyleIndex
        {
            get
            {
                return _namingStyleIndex;
            }
            set
            {
                _namingStyleIndex = value;
            }
        }

        private NamingRuleTreeViewModel parent;

        private CollectionView _parentRuleList;
        public CollectionView ParentRuleList
        {
            get
            {
                return _parentRuleList;
            }
            set
            {

            }
        }

        private int _parentRuleIndex;

        public int ParentRuleIndex
        {
            get
            {
                return _parentRuleIndex;
            }
            set
            {
                _parentRuleIndex = value;
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

        public string EnforcementLevel { get; set; }

        internal bool TrySubmit()
        {
            return true;
        }
    }
}

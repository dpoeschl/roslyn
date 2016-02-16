// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal partial class NamingRuleDialogViewModel : AbstractNotifyPropertyChanged
    {
        private INotificationService _notificationService;

        public NamingRuleDialogViewModel(
            string name,
            SymbolSpecificationViewModel symbolSpecification,
            IList<SymbolSpecificationViewModel> symbolSpecificationList,
            NamingStyleViewModel namingStyle,
            IList<NamingStyleViewModel> namingStyleList,
            NamingRuleTreeItemViewModel parent,
            IList<NamingRuleTreeItemViewModel> allowableParentList,
            EnforcementLevel enforcementLevel,
            INotificationService notificationService)
        {
            this.name = name;

            this._notificationService = notificationService;

            this.symbolSpec = symbolSpecification;
            this._symbolSpecificationList = new CollectionView(symbolSpecificationList);
            this._selectedSymbolSpecificationIndex = symbolSpecificationList.IndexOf(symbolSpec);

            this._namingStyle = namingStyle;
            this._namingStyleList = new CollectionView(namingStyleList);
            this._namingStyleIndex = namingStyleList.IndexOf(namingStyle);

            allowableParentList.Insert(0, new NamingRuleTreeItemViewModel("-- None --"));
            this._parentRuleList = new CollectionView(allowableParentList);
            this._parentRuleIndex = parent != null ? allowableParentList.IndexOf(parent) : 0;
            if (_parentRuleIndex < 0)
            {
                _parentRuleIndex = 0;
            }

            _enforcementLevelsList = new List<EnforcementLevel>
                {
                    new EnforcementLevel("None", DiagnosticSeverity.Hidden, KnownMonikers.None),
                    new EnforcementLevel("Info", DiagnosticSeverity.Info, KnownMonikers.StatusInformation),
                    new EnforcementLevel("Warning", DiagnosticSeverity.Warning, KnownMonikers.StatusWarning),
                    new EnforcementLevel("Error", DiagnosticSeverity.Error, KnownMonikers.StatusError),
                };

            _enforcementLevelIndex = _enforcementLevelsList.IndexOf(_enforcementLevelsList.Single(e => e.Value == enforcementLevel.Value));
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
                SetProperty(ref _selectedSymbolSpecificationIndex, value);
            }
        }

        private NamingStyleViewModel _namingStyle;

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
                SetProperty(ref _namingStyleIndex, value);
            }
        }

        private NamingRuleTreeItemViewModel parent;

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

        private IList<EnforcementLevel> _enforcementLevelsList;
        public IList<EnforcementLevel> EnforcementLevelsList
        {
            get
            {
                return _enforcementLevelsList;
            }
        }

        private int _enforcementLevelIndex;
        public int EnforcementLevelIndex
        {
            get
            {
                return _enforcementLevelIndex;
            }
            set
            {
                _enforcementLevelIndex = value;
            }
        }

        internal bool TrySubmit()
        {
            if (_selectedSymbolSpecificationIndex < 0 || _namingStyleIndex < 0)
            {
                _notificationService.SendNotification("Choose a Symbol Specification and a Naming Style.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                _notificationService.SendNotification("Enter a title for this Naming Rule.");
                return false;
            }
            
            return true;
        }

        private void SendFailureNotification(string message)
        {
            _notificationService.SendNotification(message, severity: NotificationSeverity.Information);
        }
    }
}

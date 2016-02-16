// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options
{
    internal class SymbolSpecificationViewModel : AbstractNotifyPropertyChanged
    {
        public Guid ID { get; set; }
        public List<SymbolKindViewModel> SymbolKindList { get; set; }
        public List<AccessibilityViewModel> AccessibilityList { get; set; }
        public List<ModifierViewModel> ModifierList { get; set; }

        private string _symbolSpecName;
        public string SymbolSpecName
        {
            get { return _symbolSpecName; }
            set { SetProperty(ref _symbolSpecName, value); }
        }

        public SymbolSpecificationViewModel(INotificationService notificationService) : this(new SymbolSpecification(), notificationService) { }

        private readonly INotificationService _notificationService;

        public SymbolSpecificationViewModel(SymbolSpecification specification, INotificationService notificationService)
        {
            _notificationService = notificationService;
            SymbolSpecName = specification.Name;
            ID = specification.ID;

            // The list of supported SymbolKinds is limited due to https://github.com/dotnet/roslyn/issues/8753. 
            // We would prefer to allow for naming symbols specifications over all SymbolKinds.
            SymbolKindList = new List<SymbolKindViewModel>
                {
                    new SymbolKindViewModel(specification, TypeKind.Class),
                    new SymbolKindViewModel(specification, TypeKind.Struct),
                    new SymbolKindViewModel(specification, TypeKind.Interface),
                    new SymbolKindViewModel(specification, TypeKind.Enum),
                    // new SymbolKindViewModel(specification, TypeKind.Module),
                    new SymbolKindViewModel(specification, SymbolKind.Property),
                    new SymbolKindViewModel(specification, SymbolKind.Method),
                    new SymbolKindViewModel(specification, SymbolKind.Field),
                    new SymbolKindViewModel(specification, SymbolKind.Event),
                    new SymbolKindViewModel(specification, SymbolKind.Namespace),
                    new SymbolKindViewModel(specification, TypeKind.Delegate),
                    new SymbolKindViewModel(specification, TypeKind.Pointer),
                    new SymbolKindViewModel(specification, TypeKind.TypeParameter),                    
                };

            AccessibilityList = new List<AccessibilityViewModel>
                {
                    new AccessibilityViewModel(specification, Accessibility.Public),
                    new AccessibilityViewModel(specification, Accessibility.Internal),
                    new AccessibilityViewModel(specification, Accessibility.Private),
                    new AccessibilityViewModel(specification, Accessibility.Protected),
                    // new AccessibilityViewModel(specification, Accessibility.ProtectedAndInternal),
                    new AccessibilityViewModel(specification, Accessibility.ProtectedOrInternal),
                };

            ModifierList = new List<ModifierViewModel>
                {
                    new ModifierViewModel(specification, DeclarationModifiers.Abstract),
                    new ModifierViewModel(specification, DeclarationModifiers.Async),
                    new ModifierViewModel(specification, DeclarationModifiers.Const),
                    new ModifierViewModel(specification, DeclarationModifiers.ReadOnly),
                    new ModifierViewModel(specification, DeclarationModifiers.Static),
                };
        }

        internal interface ISymbolSpecificationViewModelPart
        {
            bool IsChecked { get; set; }
        }

        internal SymbolSpecification GetSymbolSpecification()
        {
            var result = new SymbolSpecification
            {
                ID = ID,
                Name = SymbolSpecName,
                ApplicableAccessibilityList = AccessibilityList.Where(a => a.IsChecked).Select(a => new SymbolSpecification.AccessibilityKind(a._accessibility)).ToList(),
                RequiredModifierList = ModifierList.Where(m => m.IsChecked).Select(m => new SymbolSpecification.ModifierKind(m._modifier)).ToList(),
                ApplicableSymbolKindList = SymbolKindList.Where(s => s.IsChecked).Select(s => s.CreateSymbolKindOrTypeKind()).ToList()
            };
            return result;
        }

        public class SymbolKindViewModel : AbstractNotifyPropertyChanged, ISymbolSpecificationViewModelPart
        {
            public string Name { get; set; }
            public bool IsChecked
            {
                get { return _isChecked; }
                set { SetProperty(ref _isChecked, value); }
            }

            private readonly SymbolKind? _symbolKind;
            private readonly TypeKind? _typeKind;

            private bool _isChecked;

            public SymbolKindViewModel(SymbolSpecification specification, SymbolKind symbolKind)
            {
                this._symbolKind = symbolKind;
                Name = symbolKind.ToString();
                IsChecked = specification.ApplicableSymbolKindList.Any(k => k.SymbolKind == symbolKind);
            }

            public SymbolKindViewModel(SymbolSpecification specification, TypeKind typeKind)
            {
                this._typeKind = typeKind;
                Name = typeKind.ToString();
                IsChecked = specification.ApplicableSymbolKindList.Any(k => k.TypeKind == typeKind);
            }

            internal SymbolSpecification.SymbolKindOrTypeKind CreateSymbolKindOrTypeKind()
            {
                if (_symbolKind.HasValue)
                {
                    return new SymbolSpecification.SymbolKindOrTypeKind(_symbolKind.Value);
                }
                else
                {
                    return new SymbolSpecification.SymbolKindOrTypeKind(_typeKind.Value);
                }
            }
        }

        public class AccessibilityViewModel: AbstractNotifyPropertyChanged, ISymbolSpecificationViewModelPart
        {
            internal readonly Accessibility _accessibility;

            public string Name { get; set; }

            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set { SetProperty(ref _isChecked, value); }
            }

            public AccessibilityViewModel(SymbolSpecification specification, Accessibility accessibility)
            {
                this._accessibility = accessibility;
                Name = accessibility.ToString();
                IsChecked = specification.ApplicableAccessibilityList.Any(a => a.Accessibility == accessibility);
            }
        }

        public class ModifierViewModel: AbstractNotifyPropertyChanged, ISymbolSpecificationViewModelPart
        {
            public string Name { get; set; }

            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set { SetProperty(ref _isChecked, value); }
            }

            internal readonly DeclarationModifiers _modifier;

            public ModifierViewModel(SymbolSpecification specification, DeclarationModifiers modifier)
            {
                this._modifier = modifier;
                Name = modifier.ToString();
                IsChecked = specification.RequiredModifierList.Any(m => m.Modifier == modifier);
            }
        }

        internal bool TrySubmit()
        {
            if (string.IsNullOrWhiteSpace(SymbolSpecName))
            {
                _notificationService.SendNotification("Enter a name for this Symbol Specification.");
                return false;
            }

            return true;
        }
    }
}

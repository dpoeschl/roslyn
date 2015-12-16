// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.Editing;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal class SymbolSpecificationViewModel : AbstractNotifyPropertyChanged
    {
        public Guid ID { get; set; }
        public string SymbolSpecName { get; set; }
        public List<SymbolKindViewModel> SymbolKindList { get; set; }
        public List<AccessibilityViewModel> AccessibilityList { get; set; }
        public List<ModifierViewModel> ModifierList { get; set; }

        public SymbolSpecificationViewModel() : this(new SymbolSpecification()) { }

        public SymbolSpecificationViewModel(SymbolSpecification specification)
        {
            SymbolSpecName = specification.Name;
            ID = specification.ID;

            SymbolKindList = new List<SymbolKindViewModel>
                {
                    new SymbolKindViewModel(specification, SymbolKind.Namespace),
                    new SymbolKindViewModel(specification, TypeKind.Class),
                    new SymbolKindViewModel(specification, TypeKind.Struct),
                    new SymbolKindViewModel(specification, TypeKind.Interface),
                    new SymbolKindViewModel(specification, TypeKind.Delegate),
                    new SymbolKindViewModel(specification, TypeKind.Dynamic),
                    new SymbolKindViewModel(specification, TypeKind.Enum),
                    new SymbolKindViewModel(specification, TypeKind.Module),
                    new SymbolKindViewModel(specification, TypeKind.Pointer),
                    new SymbolKindViewModel(specification, TypeKind.Submission),
                    new SymbolKindViewModel(specification, TypeKind.TypeParameter),
                    new SymbolKindViewModel(specification, SymbolKind.Property),
                    new SymbolKindViewModel(specification, SymbolKind.Method),
                    new SymbolKindViewModel(specification, SymbolKind.Field),
                    new SymbolKindViewModel(specification, SymbolKind.Event),
                    new SymbolKindViewModel(specification, SymbolKind.Parameter),
                    new SymbolKindViewModel(specification, SymbolKind.Local),
                    new SymbolKindViewModel(specification, SymbolKind.Label),
                    new SymbolKindViewModel(specification, SymbolKind.Alias),
                    new SymbolKindViewModel(specification, SymbolKind.Preprocessing),
                    new SymbolKindViewModel(specification, SymbolKind.RangeVariable),
                };

            AccessibilityList = new List<AccessibilityViewModel>
                {
                    new AccessibilityViewModel(specification, Accessibility.Public),
                    new AccessibilityViewModel(specification, Accessibility.Internal),
                    new AccessibilityViewModel(specification, Accessibility.Private),
                    new AccessibilityViewModel(specification, Accessibility.Protected),
                    new AccessibilityViewModel(specification, Accessibility.ProtectedAndInternal),
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

        internal SymbolSpecification GetSymbolSpecification()
        {
            var result = new SymbolSpecification
            {
                ID = ID,
                Name = SymbolSpecName,
                AccessibilityList = AccessibilityList.Where(a => a.IsChecked).Select(a => new SymbolSpecification.AccessibilityKind(a._accessibility)).ToList(),
                ModifierList = ModifierList.Where(m => m.IsChecked).Select(m => new SymbolSpecification.ModifierKind(m._modifier)).ToList(),
                SymbolKindList = SymbolKindList.Where(s => s.IsChecked).Select(s => s.CreateSymbolKindOrTypeKind()).ToList()
            };
            return result;
        }

        public class SymbolKindViewModel
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }

            private readonly SymbolKind? _symbolKind;
            private readonly TypeKind? _typeKind;

            public SymbolKindViewModel(SymbolSpecification specification, SymbolKind symbolKind)
            {
                this._symbolKind = symbolKind;
                Name = symbolKind.ToString();
                IsChecked = specification.SymbolKindList.Any(k => k.SymbolKind == symbolKind);
            }

            public SymbolKindViewModel(SymbolSpecification specification, TypeKind typeKind)
            {
                this._typeKind = typeKind;
                Name = typeKind.ToString();
                IsChecked = specification.SymbolKindList.Any(k => k.TypeKind == typeKind);
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

        public class AccessibilityViewModel
        {
            internal readonly Accessibility _accessibility;

            public string Name { get; set; }
            public bool IsChecked { get; set; }

            public AccessibilityViewModel(SymbolSpecification specification, Accessibility accessibility)
            {
                this._accessibility = accessibility;
                Name = accessibility.ToString();
                IsChecked = specification.AccessibilityList.Any(a => a.Accessibility == accessibility);
            }
        }

        public class ModifierViewModel
        {
            public string Name { get; set; }
            public bool IsChecked { get; set; }

            internal readonly DeclarationModifiers _modifier;

            public ModifierViewModel(SymbolSpecification specification, DeclarationModifiers modifier)
            {
                this._modifier = modifier;
                Name = modifier.ToString();
                IsChecked = specification.ModifierList.Any(m => m.Modifier == modifier);
            }
        }

        internal bool TrySubmit()
        {
            return true;
        }
    }
}

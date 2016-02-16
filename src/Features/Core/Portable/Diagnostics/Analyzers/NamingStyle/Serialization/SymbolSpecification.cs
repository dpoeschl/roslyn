// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    [DataContract]
    internal class SymbolSpecification
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<SymbolKindOrTypeKind> SymbolKindList { get; set; }
        [DataMember]
        public List<AccessibilityKind> AccessibilityList { get; set; }
        [DataMember]
            public List<ModifierKind> ModifierList { get; set; }

        internal SymbolSpecification()
        {
            ID = Guid.NewGuid();

            SymbolKindList = new List<SymbolKindOrTypeKind>
                {
                    new SymbolKindOrTypeKind(SymbolKind.Namespace),
                    new SymbolKindOrTypeKind(TypeKind.Class),
                    new SymbolKindOrTypeKind(SymbolKind.Property),
                    new SymbolKindOrTypeKind(SymbolKind.Method),
                    new SymbolKindOrTypeKind(SymbolKind.Field),
                    new SymbolKindOrTypeKind(SymbolKind.Event),
                    new SymbolKindOrTypeKind(TypeKind.Struct),
                    new SymbolKindOrTypeKind(TypeKind.Interface),
                    new SymbolKindOrTypeKind(TypeKind.Delegate),
                    new SymbolKindOrTypeKind(TypeKind.Enum),
                    new SymbolKindOrTypeKind(TypeKind.Module),
                    new SymbolKindOrTypeKind(TypeKind.Pointer),
                    new SymbolKindOrTypeKind(TypeKind.TypeParameter),
                    new SymbolKindOrTypeKind(SymbolKind.Parameter),
                    new SymbolKindOrTypeKind(SymbolKind.Local),
                    new SymbolKindOrTypeKind(SymbolKind.Alias),
                    new SymbolKindOrTypeKind(SymbolKind.RangeVariable),
                };

            AccessibilityList = new List<AccessibilityKind>
                {
                    new AccessibilityKind(Accessibility.Public),
                    new AccessibilityKind(Accessibility.Internal),
                    new AccessibilityKind(Accessibility.Private),
                    new AccessibilityKind(Accessibility.Protected),
                    new AccessibilityKind(Accessibility.ProtectedAndInternal),
                    new AccessibilityKind(Accessibility.ProtectedOrInternal),
                };

            ModifierList = new List<ModifierKind>
                {
                };
        }

        internal bool AppliesTo(ISymbol symbol)
        {
            if (SymbolKindList.Any() && !SymbolKindList.Any(k => k.AppliesTo(symbol)))
            {
                return false;
            }

            if (!ModifierList.All(m => m.MatchesSymbol(symbol)))
            {
                return false;
            }

            if (AccessibilityList.Any() && !AccessibilityList.Any(k => k.MatchesSymbol(symbol)))
            {
                return false;
            }

            return true;
        }

        [DataContract]

        public class SymbolKindOrTypeKind
        {
            [DataMember]
            public SymbolKind? SymbolKind { get; set; }
            [DataMember]
            public TypeKind? TypeKind { get; set; }

            public SymbolKindOrTypeKind(SymbolKind symbolKind)
            {
                SymbolKind = symbolKind;
            }

            public SymbolKindOrTypeKind(TypeKind typeKind)
            {
                TypeKind = typeKind;
            }

            public bool AppliesTo(ISymbol symbol)
            {
                if (SymbolKind.HasValue)
                {
                    return symbol.IsKind(SymbolKind.Value);
                }
                else
                {
                    var typeSymbol = symbol as ITypeSymbol;
                    return typeSymbol != null && typeSymbol.TypeKind == TypeKind.Value;
                }
            }
        }

        [DataContract]
        public class AccessibilityKind
        {
            [DataMember]
            public Accessibility Accessibility { get; set; }

            public AccessibilityKind(Accessibility accessibility)
            {
                Accessibility = accessibility;
            }

            public bool MatchesSymbol(ISymbol symbol)
            {
                return symbol.DeclaredAccessibility == Accessibility;
            }
        }

        [DataContract]
        public class ModifierKind
        {
            [DataMember]
            public ModifierKindEnum ModifierKindWrapper;

            private DeclarationModifiers _modifier;
            internal DeclarationModifiers Modifier
            {
                get
                {
                    if (_modifier == DeclarationModifiers.None)
                    {
                        _modifier = new DeclarationModifiers(
                            isAbstract: ModifierKindWrapper == ModifierKindEnum.IsAbstract,
                            isStatic: ModifierKindWrapper == ModifierKindEnum.IsStatic,
                            isAsync: ModifierKindWrapper == ModifierKindEnum.IsAsync,
                            isReadOnly: ModifierKindWrapper == ModifierKindEnum.IsReadOnly,
                            isConst: ModifierKindWrapper == ModifierKindEnum.IsConst);
                    }

                    return _modifier;
                }
                set
                {
                    _modifier = value;

                    if (value.IsAbstract)
                    {
                        ModifierKindWrapper = ModifierKindEnum.IsAbstract;
                    }
                    else if (value.IsStatic)
                    {
                        ModifierKindWrapper = ModifierKindEnum.IsStatic;
                    }
                    else if (value.IsAsync)
                    {
                        ModifierKindWrapper = ModifierKindEnum.IsAsync;
                    }
                    else if (value.IsReadOnly)
                    {
                        ModifierKindWrapper = ModifierKindEnum.IsReadOnly;
                    }
                    else if (value.IsConst)
                    {
                        ModifierKindWrapper = ModifierKindEnum.IsConst;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            public ModifierKind(DeclarationModifiers modifier)
            {
                this.Modifier = modifier;
            }

            public bool MatchesSymbol(ISymbol symbol)
            {
                if ((Modifier.IsAbstract && symbol.IsAbstract) ||
                    (Modifier.IsStatic && symbol.IsStatic))
                {
                    return true;
                }

                var method = symbol as IMethodSymbol;
                var field = symbol as IFieldSymbol;
                var local = symbol as ILocalSymbol;

                if (Modifier.IsAsync && method != null && method.IsAsync)
                {
                    return true;
                }

                if (Modifier.IsReadOnly && field != null && field.IsReadOnly)
                {
                    return true;
                }

                if (Modifier.IsConst && (field != null && field.IsConst) || (local != null && local.IsConst))
                {
                    return true;
                }

                return false;
            }
        }
        
        public enum ModifierKindEnum
        {
            IsAbstract,
            IsStatic,
            IsAsync,
            IsReadOnly,
            IsConst
        }
    }
}
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles
{
    internal class NamingRule
    {
        public string Title;
        public List<NamingRule> Children;
        public SymbolSpecification SymbolSpecification;
        public NamingStyle NamingStyle;
        public DiagnosticSeverity EnforcementLevel;

        public NamingRule(string title, List<NamingRule> children, SymbolSpecification symbolSpecification, NamingStyle namingStyle, DiagnosticSeverity enforcementLevel)
        {
            Title = title;
            Children = children;
            SymbolSpecification = symbolSpecification;
            NamingStyle = namingStyle;
            EnforcementLevel = enforcementLevel; // TODO: unify with Balaji's EnforcementLevel mechanism if possible
        }

        public bool AppliesTo(ISymbol symbol)
        {
            return SymbolSpecification.AppliesTo(symbol);
        }

        internal NamingRule GetBestMatchingRule(ISymbol symbol)
        {
            Debug.Assert(SymbolSpecification.AppliesTo(symbol));
            var matchingChild = Children?.FirstOrDefault(r => r.AppliesTo(symbol));
            return matchingChild?.GetBestMatchingRule(symbol) ?? this;
        }

        public bool IsNameCompliant(string name, out string failureReason)
        {
            return NamingStyle.IsNameCompliant(name, out failureReason);
        }
    }
}
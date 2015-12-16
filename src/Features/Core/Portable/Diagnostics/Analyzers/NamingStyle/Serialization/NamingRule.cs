using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    internal class NamingRule
    {
        public string Title;
        public List<NamingRule> Children;
        public SymbolSpecification SymbolSpecification;
        public NamingStyle NamingStyle;

        public NamingRule(string title, List<NamingRule> children, SymbolSpecification symbolSpecification, NamingStyle namingStyle)
        {
            Title = title;
            Children = children;
            SymbolSpecification = symbolSpecification;
            NamingStyle = namingStyle;
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

        public bool IsNameAcceptable(string name, out string failureReason)
        {
            return NamingStyle.IsNameAcceptable(name, out failureReason);
        }
    }
}
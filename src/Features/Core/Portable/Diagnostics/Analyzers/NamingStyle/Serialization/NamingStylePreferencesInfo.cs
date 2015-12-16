using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    internal class NamingStylePreferencesInfo
    {
        public IList<NamingRule> NamingRules { get; set; }

        internal bool TryGetApplicableRule(ISymbol symbol, out NamingRule applicableRule)
        {
            if (NamingRules == null)
            {
                applicableRule = null;
                return false;
            }

            var matchingRule = NamingRules.FirstOrDefault(r => r.AppliesTo(symbol));
            if (matchingRule == null)
            {
                applicableRule = null;
                return false;
            }

            applicableRule = matchingRule.GetBestMatchingRule(symbol);
            return true;
        }
    }
}
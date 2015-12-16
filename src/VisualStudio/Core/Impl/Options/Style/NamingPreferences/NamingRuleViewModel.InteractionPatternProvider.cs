using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    partial class NamingRuleTreeViewModel : IInteractionPatternProvider
    {
        TPattern IInteractionPatternProvider.GetPattern<TPattern>()
        {
            return this as TPattern;
        }
    }
}

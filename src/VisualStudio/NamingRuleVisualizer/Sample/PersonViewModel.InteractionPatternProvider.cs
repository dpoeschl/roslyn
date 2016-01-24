using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgChart.Sample
{
    partial class PersonViewModel : IInteractionPatternProvider
    {
        public PersonViewModel()
        {
        }

        TPattern IInteractionPatternProvider.GetPattern<TPattern>()
        {
            return this as TPattern;
        }
    }
}

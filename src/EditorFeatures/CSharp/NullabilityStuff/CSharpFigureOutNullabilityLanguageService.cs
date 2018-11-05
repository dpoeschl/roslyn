using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor.CSharp
{
    [ExportLanguageService(typeof(IFigureOutNullabilityLanguageService), LanguageNames.CSharp), Shared]
    class CSharpFigureOutNullabilityLanguageService : IFigureOutNullabilityLanguageService
    {
        public bool ProjectSupportsNullability(Project project)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return false;
            }

            if (!(compilation is CSharpCompilation csCompilation))
            {
                return false;
            }

            var trees = compilation.SyntaxTrees;
            if (trees == null || !trees.Any())
            {
                return false;
            }

            var options = (CSharpParseOptions)trees.First().Options;
            return options.IsFeatureEnabled(12709 + 1200);
        }
    }
}

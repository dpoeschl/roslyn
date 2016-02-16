// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeFixes.FullyQualify
{
    internal abstract partial class AbstractNamingStyleCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(IDEDiagnosticIds.NamingRuleId); }
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var serializedNamingStyle = diagnostic.Properties[nameof(NamingStyle)];

            NamingStyle style;
            using (var reader = XmlReader.Create(new StringReader(serializedNamingStyle)))
            {
                var serializer = new DataContractSerializer(typeof(NamingStyle));
                style = serializer.ReadObject(reader) as NamingStyle;
            }

            var document = context.Document;
            var span = context.Span;

            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindToken(span.Start).GetAncestors<SyntaxNode>().First(n => n.Span.Contains(span));
            var model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var symbol = model.GetDeclaredSymbol(node, context.CancellationToken);

            var fixedName = style.FixNameEasy(symbol.Name);
            var solution = context.Document.Project.Solution;
            var updatedSolution = await Renamer.RenameSymbolAsync(solution, symbol, fixedName, solution.Workspace.Options).ConfigureAwait(false);
            context.RegisterCodeFix(new MyCodeAction(string.Format("Fix Name Violation: {0}", fixedName), c => Task.FromResult(updatedSolution), nameof(AbstractNamingStyleCodeFixProvider)), diagnostic);

            string otherFixedName = style.FixName(symbol.Name);
            if (otherFixedName != fixedName)
            {
                var updatedSolution2 = await Renamer.RenameSymbolAsync(solution, symbol, otherFixedName, solution.Workspace.Options).ConfigureAwait(false);
                context.RegisterCodeFix(new MyCodeAction(string.Format("Fix Name Violation: {0}", otherFixedName), c => Task.FromResult(updatedSolution2), nameof(AbstractNamingStyleCodeFixProvider) + "2"), diagnostic);
            }
        }
        private class MyCodeAction : CodeAction.SolutionChangeAction
        {
            public MyCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution, equivalenceKey)
            {
            }
        }
    }
}

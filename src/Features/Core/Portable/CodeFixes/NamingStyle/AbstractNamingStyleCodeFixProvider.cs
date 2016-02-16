﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeFixes.NamingStyles
{
    internal abstract class AbstractNamingStyleCodeFixProvider : CodeFixProvider
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

            var fixedNames = style.MakeCompliant(symbol.Name);
            foreach (var fixedName in fixedNames)
            {
                var solution = context.Document.Project.Solution;
                context.RegisterCodeFix(
                    new FixNameCodeAction(
                        string.Format("Fix Name Violation: {0}", fixedName),
                        async c => await Renamer.RenameSymbolAsync(
                            solution,
                            symbol,
                            fixedName,
                            solution.Workspace.Options,
                            c).ConfigureAwait(false), 
                        nameof(AbstractNamingStyleCodeFixProvider)), 
                    diagnostic);
            }
        }

        private class FixNameCodeAction : CodeAction.SolutionChangeAction
        {
            public FixNameCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution, string equivalenceKey)
                : base(title, createChangedSolution, equivalenceKey)
            {
            }
        }
    }
}

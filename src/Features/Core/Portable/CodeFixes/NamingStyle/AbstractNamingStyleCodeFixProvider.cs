﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace Microsoft.CodeAnalysis.CodeFixes.FullyQualify
{
    internal abstract partial class AbstractNamingStyleCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(IDEDiagnosticIds.NamingRuleId);
            }
        }

        protected AbstractNamingStyleCodeFixProvider()
        {
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var serializedNamingStyle = diagnostic.Properties[nameof(NamingStyle)];

            DataContractSerializer ser = new DataContractSerializer(typeof(NamingStyle));
            var reader = XmlReader.Create(new StringReader(serializedNamingStyle));
            var style = ser.ReadObject(reader) as NamingStyle;

            var document = context.Document;
            var span = context.Span;
            var diagnostics = context.Diagnostics;
            var cancellationToken = context.CancellationToken;

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var node = root.FindToken(span.Start).GetAncestors<SyntaxNode>().First(n => n.Span.Contains(span));
            var model = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var symbol = model.GetDeclaredSymbol(node, context.CancellationToken);

            var fixedName = style.FixName(symbol.Name);
            var solution = context.Document.Project.Solution;
            var updatedSolution = await Rename.Renamer.RenameSymbolAsync(solution, symbol, fixedName, solution.Workspace.Options).ConfigureAwait(false);
            context.RegisterCodeFix(CodeAction.Create(string.Format("Fix Name Violation: {0}", fixedName), c => Task.FromResult(updatedSolution)), diagnostic);
        }
    }
}

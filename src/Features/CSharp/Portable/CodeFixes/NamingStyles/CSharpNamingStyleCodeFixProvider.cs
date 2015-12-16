// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.ImplementInterface;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.CodeFixes.FullyQualify;

namespace Microsoft.CodeAnalysis.CSharp.CodeFixes.FullyQualify
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "Naming Style"), Shared]
    internal class CSharpNamingStyleCodeFixProvider : AbstractNamingStyleCodeFixProvider
    {
        //public sealed override FixAllProvider GetFixAllProvider()
        //{
        //    return WellKnownFixAllProviders.BatchFixer;
        //}
    }
}

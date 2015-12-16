// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.RemoveUnnecessaryCast;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;

namespace Microsoft.CodeAnalysis.CSharp.Diagnostics.NamingStyle
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class CSharpNamingStyleDiagnosticAnalyzer : NamingStyleDiagnosticAnalyzerBase
    {
    }
}

Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Diagnostics.Analyzers

Namespace Microsoft.CodeAnalysis.CSharp.Diagnostics.NamingStyle
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Friend Class CSharpNamingStyleDiagnosticAnalyzer
        Inherits NamingStyleDiagnosticAnalyzerBase
    End Class
End Namespace
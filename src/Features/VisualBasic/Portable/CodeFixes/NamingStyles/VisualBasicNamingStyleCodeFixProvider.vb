Imports System.Composition
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.CodeFixes.FullyQualify

Namespace Microsoft.CodeAnalysis.CSharp.CodeFixes.FullyQualify
    <ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=PredefinedCodeFixProviderNames.ApplyNamingStyle), [Shared]>
    Friend Class CSharpNamingStyleCodeFixProvider
        Inherits AbstractNamingStyleCodeFixProvider
    End Class
End Namespace
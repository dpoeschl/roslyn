﻿using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers
{
    internal abstract class NamingStyleDiagnosticAnalyzerBase : DiagnosticAnalyzer, IBuiltInAnalyzer
    {
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(FeaturesResources.DaveMessage), FeaturesResources.ResourceManager, typeof(FeaturesResources));
        private static readonly LocalizableString s_localizableTitleQualifyMembers = new LocalizableResourceString(nameof(FeaturesResources.DaveMessage), FeaturesResources.ResourceManager, typeof(FeaturesResources));
        private static readonly DiagnosticDescriptor s_descriptorQualifyMemberAccess = new DiagnosticDescriptor(IDEDiagnosticIds.NamingRuleId,
             s_localizableTitleQualifyMembers,
             s_localizableMessage,
             DiagnosticCategory.Style,
             DiagnosticSeverity.Hidden,
             isEnabledByDefault: true,
             customTags: DiagnosticCustomTags.Unnecessary);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_descriptorQualifyMemberAccess);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(Foox);
        }

        private void Foox(CompilationStartAnalysisContext context)
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(SerializableNamingStylePreferencesInfo));

            var optionSet = (context.Options as WorkspaceAnalyzerOptions)?.Workspace.Options;
            var currentValue = optionSet.GetOption(SimplificationOptions.NamingPreferences, context.Compilation.Language);

            if (!string.IsNullOrEmpty(currentValue))
            {
                SerializableNamingStylePreferencesInfo viewModel = 
                    Deserialize(currentValue, typeof(SerializableNamingStylePreferencesInfo)) as SerializableNamingStylePreferencesInfo;
                var preferencesInfo = viewModel.GetPreferencesInfo();
                context.RegisterSymbolAction(
                    a => Foo(a, preferencesInfo),
                    Enum.GetValues(typeof(SymbolKind)).Cast<SymbolKind>().ToArray()); // Yuck
            }
        }

        private void Foo(SymbolAnalysisContext context, NamingStylePreferencesInfo preferences)
        {
            NamingRule applicableRule;
            if (preferences.TryGetApplicableRule(context.Symbol, out applicableRule))
            {
                string failureReason;
                if (!applicableRule.IsNameAcceptable(context.Symbol.Name, out failureReason))
                {
                    var reference = context.Symbol.DeclaringSyntaxReferences.First();
                    var location = Location.Create(reference.SyntaxTree, reference.Span);

                    var descriptor = new DiagnosticDescriptor(IDEDiagnosticIds.NamingRuleId,
                         "Title",
                         string.Format("'{0}' naming violation - ", applicableRule.Title) + failureReason,
                         DiagnosticCategory.Style,
                         DiagnosticSeverity.Warning,
                         isEnabledByDefault: true,
                         customTags: DiagnosticCustomTags.Unnecessary);

                    var resultingXml = Serialize(applicableRule.NamingStyle);
                    var builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder[nameof(NamingStyle)] = resultingXml;
                    builder["OptionsPageGuid"] = "13c3bbb4-f18f-4111-9f54-a0fb010d8888";
                    var diagnostic = Diagnostic.Create(descriptor, location, builder.ToImmutable());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(toType);
                return deserializer.ReadObject(stream);
            }
        }

        public DiagnosticAnalyzerCategory GetAnalyzerCategory()
        {
            return DiagnosticAnalyzerCategory.SemanticDocumentAnalysis;
        }
    }
}

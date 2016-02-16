// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Simplification;

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

        // https://github.com/dotnet/roslyn/issues/8753
        private static readonly SymbolKind[] _symbolKinds = new[] { SymbolKind.Event, SymbolKind.Field, SymbolKind.Method, SymbolKind.NamedType, SymbolKind.Namespace, SymbolKind.Property };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_descriptorQualifyMemberAccess);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStartAction);
        }

        private void CompilationStartAction(CompilationStartAnalysisContext context)
        {
            var optionSet = (context.Options as WorkspaceAnalyzerOptions)?.Workspace.Options;
            var currentValue = optionSet.GetOption(SimplificationOptions.NamingPreferences, context.Compilation.Language);

            if (!string.IsNullOrEmpty(currentValue))
            {
                SerializableNamingStylePreferencesInfo viewModel = 
                    Deserialize(currentValue, typeof(SerializableNamingStylePreferencesInfo)) as SerializableNamingStylePreferencesInfo;
                var preferencesInfo = viewModel.GetPreferencesInfo();
                context.RegisterSymbolAction(
                    symbolContext => SymbolAction(symbolContext, preferencesInfo),
                    _symbolKinds);
            }
        }

        private void SymbolAction(SymbolAnalysisContext context, NamingStylePreferencesInfo preferences)
        {
            NamingRule applicableRule;
            if (preferences.TryGetApplicableRule(context.Symbol, out applicableRule))
            {
                string failureReason;
                if (applicableRule.EnforcementLevel != DiagnosticSeverity.Hidden && 
                    !applicableRule.IsNameAcceptable(context.Symbol.Name, out failureReason))
                {
                    var descriptor = new DiagnosticDescriptor(IDEDiagnosticIds.NamingRuleId,
                         "Title",
                         string.Format("'{0}' naming violation - ", applicableRule.Title) + failureReason,
                         DiagnosticCategory.Style,
                         applicableRule.EnforcementLevel,
                         isEnabledByDefault: true,
                         customTags: DiagnosticCustomTags.Unnecessary);

                    var builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder[nameof(NamingStyle)] = Serialize(applicableRule.NamingStyle);
                    builder["OptionsPageGuid"] = "8FD0B177-B244-AAAA-8E37-6FB7FFFFFFFF"; // TODO: Make host-agnostic
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, context.Symbol.Locations.First(), builder.ToImmutable()));
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

        // TODO?
        public DiagnosticAnalyzerCategory GetAnalyzerCategory()
        {
            return DiagnosticAnalyzerCategory.SemanticDocumentAnalysis;
        }
    }
}

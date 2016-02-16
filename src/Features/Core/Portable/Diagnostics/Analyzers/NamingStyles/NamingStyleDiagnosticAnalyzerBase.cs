// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Simplification;

namespace Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles
{
    internal abstract class NamingStyleDiagnosticAnalyzerBase : DiagnosticAnalyzer, IBuiltInAnalyzer
    {
        // TODO: Loc
        private static readonly LocalizableString s_localizableMessage = new LocalizableResourceString(nameof(FeaturesResources.Unknown), FeaturesResources.ResourceManager, typeof(FeaturesResources));
        private static readonly LocalizableString s_localizableTitleNamingStyle = new LocalizableResourceString(nameof(FeaturesResources.Unknown), FeaturesResources.ResourceManager, typeof(FeaturesResources));
        private static readonly DiagnosticDescriptor s_descriptorNamingStyle = new DiagnosticDescriptor(IDEDiagnosticIds.NamingRuleId,
             s_localizableTitleNamingStyle,
             s_localizableMessage,
             DiagnosticCategory.Style,
             DiagnosticSeverity.Hidden,
             isEnabledByDefault: true,
             customTags: DiagnosticCustomTags.Unnecessary);

        // Applicable SymbolKind list is limited due to https://github.com/dotnet/roslyn/issues/8753. 
        // We would prefer to respond to the names of all symbols.
        private static readonly ImmutableArray<SymbolKind> _symbolKinds = new[] 
            {
                SymbolKind.Event,
                SymbolKind.Field,
                SymbolKind.Method,
                SymbolKind.NamedType,
                SymbolKind.Namespace,
                SymbolKind.Property
            }.ToImmutableArray();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_descriptorNamingStyle);

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
                // Deserializing the naming preference info on every CompilationStart is expensive.
                // Instead, the diagnostic engine should listen for option changes and have the
                // ability to create the new SerializableNamingStylePreferencesInfo when it detects
                // any change. The overall system would then only deserialize & allocate when 
                // actually necessary.
                var viewModel = Deserialize(currentValue, typeof(SerializableNamingStylePreferencesInfo)) as SerializableNamingStylePreferencesInfo;
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
                    !applicableRule.IsNameCompliant(context.Symbol.Name, out failureReason))
                {
                    var descriptor = new DiagnosticDescriptor(IDEDiagnosticIds.NamingRuleId,
                         "Title", // Todo: strings
                         string.Format("'{0}' naming violation - ", applicableRule.Title) + failureReason,
                         DiagnosticCategory.Style,
                         applicableRule.EnforcementLevel,
                         isEnabledByDefault: true);

                    var builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder[nameof(NamingStyle)] = Serialize(applicableRule.NamingStyle);
                    builder["OptionName"] = nameof(SimplificationOptions.NamingPreferences); // TODO: doc this
                    builder["OptionLanguage"] = LanguageNames.CSharp;
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
            try
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
            catch (SerializationException)
            {
                return new SerializableNamingStylePreferencesInfo();
            }
        }

        public DiagnosticAnalyzerCategory GetAnalyzerCategory()
        {
            return DiagnosticAnalyzerCategory.SemanticSpanAnalysis;
        }
    }
}

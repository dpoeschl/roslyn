// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.LanguageServices.Implementation.Options;
using Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis;
using System.Xml;
using System.IO;

namespace Microsoft.VisualStudio.LanguageServices.CSharp.Options
{
    [Guid(Guids.CSharpOptionPageNamingStyleIdString)]
    internal class NamingStylesOptionPage : AbstractOptionPage
    {
        protected override AbstractOptionPageControl CreateOptionPage(IServiceProvider serviceProvider)
        {
            return new NamingPreferencesDialog(serviceProvider);

            //var result = dialog.ShowModal();
            //if (result == true)
            //{
            //    using (var output = new StringWriter())
            //    using (var writer = new XmlTextWriter(output) { Formatting = System.Xml.Formatting.Indented })
            //    {
            //        ser.WriteObject(writer, viewModel.GetInfo());
            //        var resultingXml = output.GetStringBuilder().ToString();
            //        Options = Options.WithChangedOption(SimplificationOptions.NamingPreferences, LanguageNames.CSharp, resultingXml);
            //    }
            //}
        }
    }

    [Guid(Guids.CSharpOptionPageColorizationStyleIdString)]
    internal class ColorizationStyleOptionPage : AbstractOptionPage
    {
        protected override AbstractOptionPageControl CreateOptionPage(IServiceProvider serviceProvider)
        {
            return new NamingPreferencesDialog(serviceProvider);

            //var result = dialog.ShowModal();
            //if (result == true)
            //{
            //    using (var output = new StringWriter())
            //    using (var writer = new XmlTextWriter(output) { Formatting = System.Xml.Formatting.Indented })
            //    {
            //        ser.WriteObject(writer, viewModel.GetInfo());
            //        var resultingXml = output.GetStringBuilder().ToString();
            //        Options = Options.WithChangedOption(SimplificationOptions.NamingPreferences, LanguageNames.CSharp, resultingXml);
            //    }
            //}
        }
    }
}

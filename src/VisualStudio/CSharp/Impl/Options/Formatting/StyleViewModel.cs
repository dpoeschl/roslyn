// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.VisualStudio.LanguageServices.Implementation.Options;
using System.Windows.Controls;
using Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences;
using Roslyn.Utilities;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using System.Windows.Media;

namespace Microsoft.VisualStudio.LanguageServices.CSharp.Options.Formatting
{
    internal class StyleViewModel : AbstractOptionPreviewViewModel
    {
        internal override bool ShouldPersistOption(OptionKey key)
        {
            return key.Option.Feature == CSharpCodeStyleOptions.FeatureName || key.Option.Feature == SimplificationOptions.PerLanguageFeatureName;
        }

        private static readonly string s_declarationPreviewTrue = @"
class C{
    int x;
    void foo()
    {
//[
        this.x = 0;
//]
    }
}";

        private static readonly string s_declarationPreviewFalse = @"
class C{
    int x;
    void foo()
    {
//[
        x = 0;
//]
    }
}";

        private static readonly string s_varPreviewTrue = @"
class C{
    void foo()
    {
//[
        var x = 0;
//]
    }
}";

        private static readonly string s_varPreviewFalse = @"
class C{
    void foo()
    {
//[
        int x = 0;
//]
    }
}";

        private static readonly string s_intrinsicPreviewDeclarationTrue = @"
class Program
{
//[
    private int _member;
    static void M(int argument)
    {
        int local;
    }
//]
}";

        private static readonly string s_intrinsicPreviewDeclarationFalse = @"
using System;
class Program
{
//[
    private Int32 _member;
    static void M(Int32 argument)
    {
        Int32 local;
    }
//]
}";

        private static readonly string s_intrinsicPreviewMemberAccessTrue = @"
class Program
{
//[
    static void M()
    {
        var local = int.MaxValue;
    }
//]
}";

        private static readonly string s_intrinsicPreviewMemberAccessFalse = @"
using System;
class Program
{
//[
    static void M()
    {
        var local = Int32.MaxValue;
    }
//]
}";
        private readonly OptionSet _optionSet;

        internal StyleViewModel(OptionSet optionSet, IServiceProvider serviceProvider, string parameter = null) : base(optionSet, serviceProvider, LanguageNames.CSharp)
        {
            Items.Add(new CheckBoxOptionViewModel(SimplificationOptions.QualifyMemberAccessWithThisOrMe, CSharpVSResources.QualifyMemberAccessWithThis, s_declarationPreviewTrue, s_declarationPreviewFalse, this, optionSet));
            Items.Add(new CheckBoxOptionViewModel(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInDeclaration, CSharpVSResources.PreferIntrinsicPredefinedTypeKeywordInDeclaration, s_intrinsicPreviewDeclarationTrue, s_intrinsicPreviewDeclarationFalse, this, optionSet));
            Items.Add(new CheckBoxOptionViewModel(SimplificationOptions.PreferIntrinsicPredefinedTypeKeywordInMemberAccess, CSharpVSResources.PreferIntrinsicPredefinedTypeKeywordInMemberAccess, s_intrinsicPreviewMemberAccessTrue, s_intrinsicPreviewMemberAccessFalse, this, optionSet));
            Items.Add(new CheckBoxOptionViewModel(CSharpCodeStyleOptions.UseVarWhenDeclaringLocals, CSharpVSResources.UseVarWhenGeneratingLocals, s_varPreviewTrue, s_varPreviewFalse, this, optionSet));
            //var button = new Button();
            //button.Click += Button_Click;
            //button.Content = "Naming Styles";
            //Items.Add(button);
            //this._optionSet = optionSet;

            //int highlightIndex;
            //if (parameter != null && int.TryParse(parameter, out highlightIndex))
            //{
            //    if (highlightIndex <= 4)
            //    {
            //        var vm = Items[highlightIndex] as CheckBoxOptionViewModel;
            //        if (vm != null)
            //        {
            //            vm.IsHighlighted = true;
            //        }

            //        var highlightedButton = Items[highlightIndex] as Button;
            //        if (highlightedButton != null)
            //        {
            //            highlightedButton.Background = new SolidColorBrush(Colors.LightYellow);
            //        }
            //    }
            //}
        }

        //private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    DataContractSerializer ser = new DataContractSerializer(typeof(SerializableNamingStylePreferencesInfo));
        //    var currentValue = Options.GetOption(SimplificationOptions.NamingPreferences, LanguageNames.CSharp);
        //    SerializableNamingStylePreferencesInfo info;

        //    if (string.IsNullOrEmpty(currentValue))
        //    {
        //        info = new SerializableNamingStylePreferencesInfo();
        //    }
        //    else
        //    {
        //        var reader = XmlReader.Create(new StringReader(currentValue));
        //        info = ser.ReadObject(reader) as SerializableNamingStylePreferencesInfo;
        //    }

        //    var viewModel = new NamingPreferencesDialogViewModel(info);

        //    var dialog = new NamingPreferencesDialog(viewModel);
        //    var result = dialog.ShowModal();
        //    if (result == true)
        //    {
        //        using (var output = new StringWriter())
        //        using (var writer = new XmlTextWriter(output) { Formatting = System.Xml.Formatting.Indented })
        //        {
        //            ser.WriteObject(writer, viewModel.GetInfo());
        //            var resultingXml = output.GetStringBuilder().ToString();
        //            Options = Options.WithChangedOption(SimplificationOptions.NamingPreferences, LanguageNames.CSharp, resultingXml);
        //        }
        //    }
        //}
    }
}

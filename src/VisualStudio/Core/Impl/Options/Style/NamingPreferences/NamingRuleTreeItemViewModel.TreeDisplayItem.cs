// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    partial class NamingRuleTreeItemViewModel : ITreeDisplayItemWithImages
    {
        public ImageMoniker ExpandedIconMoniker
        {
            get
            {
                if (EnforcementLevel == null)
                {
                    return KnownMonikers.FolderOpened;
                }

                switch (EnforcementLevel.Value)
                {
                    case CodeAnalysis.DiagnosticSeverity.Hidden:
                        return KnownMonikers.None;
                    case CodeAnalysis.DiagnosticSeverity.Info:
                        return KnownMonikers.StatusInformation;
                    case CodeAnalysis.DiagnosticSeverity.Warning:
                        return KnownMonikers.StatusWarning;
                    case CodeAnalysis.DiagnosticSeverity.Error:
                        return KnownMonikers.StatusError;
                    default:
                        break;
                }
                return KnownMonikers.Rule;
            }
        }

        public FontStyle FontStyle
        {
            get { return FontStyles.Normal; }
        }

        public FontWeight FontWeight
        {
            get { return FontWeights.Normal; }
        }

        public ImageMoniker IconMoniker
        {
            get
            {
                if (EnforcementLevel == null)
                {
                    return KnownMonikers.FolderOpened;
                }

                // SetProperty()
                switch (EnforcementLevel.Value)
                {
                    case CodeAnalysis.DiagnosticSeverity.Hidden:
                        return KnownMonikers.None;
                    case CodeAnalysis.DiagnosticSeverity.Info:
                        return KnownMonikers.StatusInformation;
                    case CodeAnalysis.DiagnosticSeverity.Warning:
                        return KnownMonikers.StatusWarning;
                    case CodeAnalysis.DiagnosticSeverity.Error:
                        return KnownMonikers.StatusError;
                    default:
                        break;
                }
                return KnownMonikers.Rule;
            }
        }

        public bool IsCut
        {
            get { return false; }
        }

        public ImageMoniker OverlayIconMoniker
        {
            get { return default(ImageMoniker); }
        }

        public ImageMoniker StateIconMoniker
        {
            get { return default(ImageMoniker); }
        }

        public string StateToolTipText
        {
            get { return null; }
        }

        public string Text
        {
            get { return this.Title; }
        }

        public object ToolTipContent
        {
            get { return null; }
        }

        public string ToolTipText
        {
            get { return null; }
        }
    }
}

using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    partial class ClassificationStyleTreeViewModel : ITreeDisplayItemWithImages
    {
        public ImageMoniker ExpandedIconMoniker
        {
            get
            {
                return KnownMonikers.Rule;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return FontStyles.Normal;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return FontWeights.Normal;
            }
        }

        public ImageMoniker IconMoniker
        {
            get
            {
                return KnownMonikers.Rule;
            }
        }

        public bool IsCut
        {
            get
            {
                return false;
            }
        }

        public ImageMoniker OverlayIconMoniker
        {
            get
            {
                return default(ImageMoniker);
            }
        }

        public ImageMoniker StateIconMoniker
        {
            get
            {
                return default(ImageMoniker);
            }
        }

        public string StateToolTipText
        {
            get
            {
                return null;
            }
        }

        public string Text
        {
            get
            {
                return this.Title;
            }
        }

        public object ToolTipContent
        {
            get
            {
                return null;
            }
        }

        public string ToolTipText
        {
            get
            {
                return null;
            }
        }
    }
}

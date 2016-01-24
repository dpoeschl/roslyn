using Microsoft.Internal.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace OrgChart.Sample
{
    internal partial class PersonViewModel : ITreeDisplayItemWithImages
    {
        public ImageMoniker ExpandedIconMoniker
        {
            get
            {
                return KnownMonikers.FolderOpened;
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
                return KnownMonikers.FolderClosed;
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
                return this.Name;
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

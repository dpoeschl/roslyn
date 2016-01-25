// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using System.Windows.Data;
using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.Classification
{
    internal class ClassificationStyleViewModel : AbstractNotifyPropertyChanged
    {
        private NamingStyle style;

        public ClassificationStyleViewModel(NamingStyle style)
        {
            this.style = style;
            this.ID = style.ID;
            this.RequiredPrefix = style.Prefix;
            this.RequiredSuffix = style.Suffix;
            this.WordSeparator = style.WordSeparator;
            this.FirstWordGroupCapitalization = (int)style.CapitalizationScheme;
            this.NamingConventionName = style.Name;
        }

        public Guid ID { get; internal set; }

        private string _namingConventionName;
        public string NamingConventionName
        {
            get { return _namingConventionName; }
            set { SetProperty(ref _namingConventionName, value); }
        }

        public string CurrentConfiguration
        {
            get
            {
                return style.CreateName(new[] { "test", "variable", "name" });
            }
            set
            {
            }
        }

        private string _requiredPrefix;

        public string RequiredPrefix
        {
            get
            {
                return _requiredPrefix;
            }
            set
            {
                style.Prefix = value;
                if (SetProperty(ref _requiredPrefix, value))
                {
                    NotifyPropertyChanged(nameof(CurrentConfiguration));
                }
            }
        }


        private string _requiredSuffix;
        public string RequiredSuffix
        {
            get
            {
                return _requiredSuffix;
            }
            set
            {
                style.Suffix = value;
                if (SetProperty(ref _requiredSuffix, value))
                {
                    NotifyPropertyChanged(nameof(CurrentConfiguration));
                }
            }
        }

        private string _wordSeparator;
        public string WordSeparator
        {
            get
            {
                return _wordSeparator;
            }
            set
            {
                style.WordSeparator = value;
                if (SetProperty(ref _wordSeparator, value))
                {
                    NotifyPropertyChanged(nameof(CurrentConfiguration));
                }
            }
        }

        private int _firstWordGroupCapitalization;

        public int FirstWordGroupCapitalization
        {
            get
            {
                return _firstWordGroupCapitalization;
            }
            set
            {
                style.CapitalizationScheme = (Capitalization)value;
                if (SetProperty(ref _firstWordGroupCapitalization, value))
                {
                    NotifyPropertyChanged(nameof(CurrentConfiguration));
                }
            }
        }

        internal bool TrySubmit()
        {
            return true;
        }

        internal NamingStyle GetNamingStyle()
        {
            style.Name = NamingConventionName;
            style.ID = ID;
            return style;
        }
    }

    public class RadioBoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}

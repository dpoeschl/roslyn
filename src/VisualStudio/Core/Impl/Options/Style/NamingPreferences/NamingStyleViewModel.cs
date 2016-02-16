// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;
using System.Windows.Data;
using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Notification;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options.Style.NamingPreferences
{
    internal class NamingStyleViewModel : AbstractNotifyPropertyChanged
    {
        private NamingStyle style;

        public NamingStyleViewModel(NamingStyle style, INotificationService notificationService)
        {
            _notificationService = notificationService;
            this.style = style;
            this.ID = style.ID;
            this.RequiredPrefix = style.Prefix;
            this.RequiredSuffix = style.Suffix;
            this.WordSeparator = style.WordSeparator;
            this.FirstWordGroupCapitalization = (int)style.CapitalizationScheme;
            this.NamingConventionName = style.Name;

            CapitalizationSchemes = new List<CapitalizationDisplay>
                {
                    new CapitalizationDisplay(Capitalization.PascalCase, "Pascal Case Name"),
                    new CapitalizationDisplay(Capitalization.CamelCase, "camel Case Name"),
                    new CapitalizationDisplay(Capitalization.FirstUpper, "First word upper"),
                    new CapitalizationDisplay(Capitalization.AllUpper, "ALL UPPER"),
                    new CapitalizationDisplay(Capitalization.AllLower, "all lower")
                };

            CapitalizationSchemeIndex = CapitalizationSchemes.IndexOf(CapitalizationSchemes.Single(s => s.Capitalization == style.CapitalizationScheme));
        }

        public IList<CapitalizationDisplay> CapitalizationSchemes { get; set; }

        private int _capitalizationSchemeIndex;
        public int CapitalizationSchemeIndex
        {
            get
            {
                return _capitalizationSchemeIndex;
            }
            set
            {
                style.CapitalizationScheme = CapitalizationSchemes[value].Capitalization;
                if (SetProperty(ref _capitalizationSchemeIndex, value))
                {
                    NotifyPropertyChanged(nameof(CurrentConfiguration));
                }
            }
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
        internal readonly INotificationService _notificationService;

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
            if (string.IsNullOrWhiteSpace(NamingConventionName))
            {
                _notificationService.SendNotification("Enter a title for this Naming Style.");
                return false;
            }

            return true;
        }

        internal NamingStyle GetNamingStyle()
        {
            style.Name = NamingConventionName;
            style.ID = ID;
            return style;
        }

        public class CapitalizationDisplay
        {
            public Capitalization Capitalization { get; set; }
            public string Name { get; set; }

            public CapitalizationDisplay(Capitalization capitalization, string name)
            {
                this.Capitalization = capitalization;
                this.Name = name;
            }
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

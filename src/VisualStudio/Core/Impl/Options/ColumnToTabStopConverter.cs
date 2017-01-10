// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Options
{
    /// <summary>
    /// DataGridTemplateColumns have custom controls that should be focused instead of the cell.
    /// </summary>
    internal class ColumnToTabStopConverter : ValueConverter<DataGridColumn, bool>
    {
        protected override bool Convert(DataGridColumn value, object parameter, CultureInfo culture)
        {
            return !(value is DataGridTemplateColumn);
        }
    }
}

using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Gopher.NET.Helpers
{
    public class ThemeVariantToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var themeVariant = value as ThemeVariant;
            return themeVariant != null && themeVariant.Key.ToString() == (string?)parameter;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || value?.Equals(false) == true) return BindingOperations.DoNothing;

            switch (parameter as string)
            {
                case "Light":
                    return ThemeVariant.Light;
                case "Dark":
                    return ThemeVariant.Dark;
                case "Default":
                default:
                    return ThemeVariant.Default;
            }
        }
    }
}

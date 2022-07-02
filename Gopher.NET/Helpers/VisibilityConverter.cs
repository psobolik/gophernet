using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace Gopher.NET.Helpers
{
    public class VisibilityConverter : IValueConverter
    {
        /*
         * If parameter is null, returns false if value is an empty string, false or any null object, true otherwise.
         * If parameter is not null, the logic is reversed, so returns true if value is an empty string, true or any
         * null object, false otherwise.
         * */
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo cultureInfo)
        {
            bool hide;
            if (value is string) hide = string.IsNullOrWhiteSpace(value as string);
            else if (value is bool boolean) hide = !boolean;
            else hide = value == null;

            if (parameter == null) return hide;
            else return !hide;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo cultureInfo)
        {
            return new Avalonia.Data.BindingNotification(null);
        }
    }
}

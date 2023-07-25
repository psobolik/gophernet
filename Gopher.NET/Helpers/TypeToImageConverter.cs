using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// https://docs.avaloniaui.net/docs/controls/image
namespace Gopher.NET.Helpers
{
    public class TypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is char typeChar && targetType.IsAssignableFrom(typeof(Avalonia.Media.Imaging.Bitmap)))
            {
                var light = Application.Current!.ActualThemeVariant == ThemeVariant.Light;
                Uri uri = typeChar switch
                {
                    GopherLib.Models.GopherEntity.DocumentTypeChar =>
                        new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-document-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-document-32.png"),
                    GopherLib.Models.GopherEntity.DirectoryTypeChar =>
                        new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-folder-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-folder-32.png"),
                    GopherLib.Models.GopherEntity.IndexSearchTypeChar =>
                        new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-search-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-search-32.png"),
                    GopherLib.Models.GopherEntity.InfoTypeChar =>
                        new Uri("avares://Gopher.NET/Assets/Images/blank-32.png"),
                    GopherLib.Models.GopherEntity.BinaryTypeChar
                        or GopherLib.Models.GopherEntity.DosBinaryTypeChar
                        or GopherLib.Models.GopherEntity.GifTypeChar
                        or GopherLib.Models.GopherEntity.ImageTypeChar
                        or GopherLib.Models.GopherEntity.BitmapTypeChar
                        or GopherLib.Models.GopherEntity.MovieTypeChar
                        or GopherLib.Models.GopherEntity.SoundTypeChar
                        or GopherLib.Models.GopherEntity.DocTypeChar
                        or GopherLib.Models.GopherEntity.WavTypeChar =>
                        new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-download-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-download-32.png"),
                    GopherLib.Models.GopherEntity.ErrorTypeChar =>
                        new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-error-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-error-32.png"),
                    _ => new Uri(light ? "avares://Gopher.NET/Assets/Images/icons8-unavailable-32-lt.png" : "avares://Gopher.NET/Assets/Images/icons8-unavailable-32.png"),
                };
                return new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(uri));
            }
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}

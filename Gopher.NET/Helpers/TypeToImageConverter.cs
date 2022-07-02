using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Platform;
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
        // public static BitmapAssetValueConverter Instance = new BitmapAssetValueConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is char typeChar && targetType.IsAssignableFrom(typeof(Avalonia.Media.Imaging.Bitmap)))
            {
                Uri uri;
                switch (typeChar)
                {
                    case GopherLib.Models.GopherEntity.DocumentTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-document-32.png");
                        break;
                    case GopherLib.Models.GopherEntity.DirectoryTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-folder-32.png");
                        break;
                    case GopherLib.Models.GopherEntity.IndexSearchTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-search-32.png");
                        break;
                    case GopherLib.Models.GopherEntity.InfoTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/blank-32.png");
                        break;
                    case GopherLib.Models.GopherEntity.BinaryTypeChar:
                    case GopherLib.Models.GopherEntity.DosBinaryTypeChar:
                    case GopherLib.Models.GopherEntity.GifTypeChar:
                    case GopherLib.Models.GopherEntity.ImageTypeChar:
                    case GopherLib.Models.GopherEntity.BitmapTypeChar:
                    case GopherLib.Models.GopherEntity.MovieTypeChar:
                    case GopherLib.Models.GopherEntity.SoundTypeChar:
                    case GopherLib.Models.GopherEntity.DocTypeChar:
                    case GopherLib.Models.GopherEntity.WavTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-download-32.png");
                        break;
                    case GopherLib.Models.GopherEntity.ErrorTypeChar:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-error-32.png");
                        break;
                    default:
                        uri = new Uri("avares://Gopher.NET/Assets/Images/icons8-unavailable-32.png");
                        break;
                }

                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets?.Open(uri);

                return new Avalonia.Media.Imaging.Bitmap(asset);
            }

            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
            // throw new NotSupportedException();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
            // throw new NotSupportedException();
        }
    }
}

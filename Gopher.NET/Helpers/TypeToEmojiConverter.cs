using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Gopher.NET.Helpers
{
    internal class TypeToEmojiConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is char typeChar && targetType.IsAssignableFrom(typeof(System.String)))
            {
                return typeChar switch
                {
                    GopherLib.Models.GopherEntity.DocumentTypeChar => "📄",
                    GopherLib.Models.GopherEntity.DirectoryTypeChar => "📁",
                    GopherLib.Models.GopherEntity.IndexSearchTypeChar => "🔎",
                    GopherLib.Models.GopherEntity.HtmlTypeChar => "🌐",
                    GopherLib.Models.GopherEntity.InfoTypeChar => " ",
                    GopherLib.Models.GopherEntity.BinaryTypeChar
                        or GopherLib.Models.GopherEntity.DosBinaryTypeChar
                        or GopherLib.Models.GopherEntity.GifTypeChar
                        or GopherLib.Models.GopherEntity.ImageTypeChar
                        or GopherLib.Models.GopherEntity.BitmapTypeChar
                        or GopherLib.Models.GopherEntity.MovieTypeChar
                        or GopherLib.Models.GopherEntity.SoundTypeChar
                        or GopherLib.Models.GopherEntity.DocTypeChar
                        or GopherLib.Models.GopherEntity.WavTypeChar
                        or GopherLib.Models.GopherEntity.PngTypeChar
                        or GopherLib.Models.GopherEntity.RichTextTypeChar
                        or GopherLib.Models.GopherEntity.PdfTypeChar
                        or GopherLib.Models.GopherEntity.XmlTypeChar => "🔽",
                    GopherLib.Models.GopherEntity.ErrorTypeChar => "🔥",
                    _ => "🚫",
                };
            }
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(new NotSupportedException(), Avalonia.Data.BindingErrorType.Error);
        }
    }
}

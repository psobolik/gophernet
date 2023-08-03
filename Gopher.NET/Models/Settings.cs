using Avalonia.Media;
using Avalonia.Styling;
using Gopher.NET.Helpers;
using System;
using System.Linq;

namespace Gopher.NET.Models
{
    public class Settings
    {
        private const int MinimumFontSize = 1;
        public const string DefaultFontFamilyName = "avares://Gopher.NET/Assets/Fonts#Source Code Pro";
        private static readonly ThemeVariant DefaultThemeVariant = ThemeVariant.Default;

        public string? HomePage { get; set; }
        private string? _fontFamilyName;
        public string FontFamilyName
        {
            get => _fontFamilyName ?? DefaultFontFamilyName;
            set => _fontFamilyName = FontManager.Current.SystemFonts.Contains(value) ? value : null;
        }

        private int _fontSize = 16;
        public int FontSize
        {
            get => _fontSize;
            set => _fontSize = Math.Max(value, MinimumFontSize);
        }
        
        private ThemeVariant? _themeVariant;
        public ThemeVariant Theme
        {
            get => _themeVariant ?? DefaultThemeVariant;
            //set => _themeVariant = value;
            set
            {
                // The ThemeVariant deserialized from the JSON settings file isn't 
                // right, so we do this to compensate. It also assures that null or
                // a defective value is interpreted as the default theme.
                var themeKey = value.Key.ToString() ?? string.Empty;
                _themeVariant = themeKey.Equals((string)ThemeVariant.Dark.Key)
                            ? ThemeVariant.Dark
                            : themeKey.Equals(ThemeVariant.Light.Key)
                            ? ThemeVariant.Light
                            : ThemeVariant.Default;
            }
        }
    }
}

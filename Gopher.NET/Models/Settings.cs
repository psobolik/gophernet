using Avalonia.Media;
using Avalonia.Styling;
using Gopher.NET.Helpers;
using System;
using System.Linq;

namespace Gopher.NET.Models
{
    public class Settings
    {
        private static readonly int _minimumFontSize = 1;
        public static readonly string DefaultFontFamilyName = "avares://Gopher.NET/Assets/Fonts#Source Code Pro";
        private static readonly ThemeVariant DefaultThemeVariant = ThemeVariant.Default;

        public string? HomePage { get; set; }
        private string? _fontFamilyName;
        public string FontFamilyName
        {
            get => _fontFamilyName ?? DefaultFontFamilyName;
            set => _fontFamilyName = FontManager.Current.SystemFonts.Contains(value) ? value : null;
        }
        public int FontSize 
        { 
            get; 
            set; 
        }
        
        private ThemeVariant? _themeVariant;
        public ThemeVariant Theme
        {
            get => _themeVariant ?? DefaultThemeVariant;
            //set => _themeVariant = value;
            set
            {
                // The ThemeVariant deserialized from the JSON settngs file isn't 
                // right, so we do this to compensate. It also assues that null or
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

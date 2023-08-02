using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Gopher.NET.Events;
using Gopher.NET.Helpers;
using Gopher.NET.Models;
using Gopher.NET.Views;
using GopherLib.Models;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Gopher.NET.ViewModels
{
    // Radio buttons: https://github.com/AvaloniaUI/Avalonia/issues/3016#issuecomment-799349256
    /* Controls in the Settings Dialog are bound to properties in the SettingsViewModel.
     * When something is changed, the property setter invokes the view model's 
     * SettingsChangedEvent handler. 
     * 
     * The Settings Dialog subscribes to the view model's SettingChangedEvent, and 
     * responds to it by invoking its own SettingChangedEvent handler. 
     * 
     * The Main Window subscribes to the Settings Dialog's SettingChangedEvent, and
     * responds to it setting the Main Window View Model's AppSettings property,
     * which notifies each of its bound properties.
     * 
     * It's magic!
     */
    public class SettingsViewModel : ReactiveObject
    {
        public event EventHandler<SettingsChangedEventArgs>? SettingsChangedEvent;

        private Settings AppSettings { get; set; }

        public int FontSize
        {
            get => AppSettings.FontSize;
            set
            {
                if (AppSettings.FontSize != value)
                {
                    AppSettings.FontSize = value;
                    SettingsChangedEvent?.Invoke(this, new SettingsChangedEventArgs(AppSettings));
                    ApplicationDataStore<Settings>.Write(AppSettings);
                    this.RaisePropertyChanged(nameof(AppSettings.FontSize));
                }
            }
        }

        private FontFamily _fontFamily;
        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    this.RaisePropertyChanged(nameof(FontFamily));
                    FontFamilyName = _fontFamily.Name;
                }
            }
        }

        public string? FontFamilyName
        {
            get => AppSettings.FontFamilyName;
            set
            {
                if (AppSettings.FontFamilyName != value)
                {
                    AppSettings.FontFamilyName = value ?? string.Empty;
                    ApplicationDataStore<Settings>.Write(AppSettings);
                    this.RaisePropertyChanged(nameof(AppSettings.FontFamilyName));
                    SettingsChangedEvent?.Invoke(this, new SettingsChangedEventArgs(AppSettings));
                }
            }
        }

        public ThemeVariant Theme
        {
            get => AppSettings.Theme ?? ThemeVariant.Default;
            set
            {
                if (AppSettings.Theme != value)
                {
                    AppSettings.Theme = value;
                    ApplicationDataStore<Settings>.Write(AppSettings);
                    SettingsChangedEvent?.Invoke(this, new SettingsChangedEventArgs(AppSettings));
                    this.RaisePropertyChanged(nameof(AppSettings.Theme));
                }
            }
        }

        public ReactiveCommand<string?, Unit> SetFontCommand { get; }
        public ReactiveCommand<int, Unit> SetFontSizeCommand { get; }
        public ReactiveCommand<ThemeVariant, Unit> SetThemeCommand { get; }

        public List<FontFamily> FontFamilies { get; set; }

        public SettingsViewModel()
        {
            FontFamilies = new() { new FontFamily(Settings.DefaultFontFamilyName) };
            FontFamilies.AddRange(FontManager.Current.SystemFonts
                .OrderBy(f => f.Name));
            AppSettings = ApplicationDataStore<Settings>.Read();
            _fontFamily = new FontFamily(AppSettings.FontFamilyName);

            SetFontCommand = ReactiveCommand.Create<string?, Unit>(SetFont);
            SetFontSizeCommand = ReactiveCommand.Create<int, Unit>(SetFontSize);
            SetThemeCommand = ReactiveCommand.Create<ThemeVariant, Unit>(SetTheme);
        }

        private Unit SetFont(string? font)
        {
            AppSettings.FontFamilyName = font ?? Settings.DefaultFontFamilyName;
            ApplicationDataStore<Settings>.Write(AppSettings);
            return new();
        }

        private Unit SetFontSize(int fontSize)
        {
            AppSettings.FontSize = fontSize;
            ApplicationDataStore<Settings>.Write(AppSettings);
            return new();
        }

        private Unit SetTheme(ThemeVariant themeVariant)
        {
            AppSettings.Theme = themeVariant;
            ApplicationDataStore<Settings>.Write(AppSettings);
            return new();
        }
    }
}

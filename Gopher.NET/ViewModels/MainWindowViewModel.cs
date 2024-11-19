using Avalonia.Media;
using Avalonia.Styling;
using Gopher.NET.Helpers;
using Gopher.NET.Models;
using GopherLib.Models;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Gopher.NET.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> GoHomeCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleHomePageCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToUrlCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowSettingsCommand { get; }
        public ReactiveCommand<Unit, string?> GetSearchTermCommand { get; }
        public ReactiveCommand<GopherEntity, string?> GetSaveFilenameCommand { get; }
        public ReactiveCommand<Unit, string?> GetOpenFilenameCommand { get; }

        public Interaction<AboutViewModel, Unit> ShowAbout { get; }
        public Interaction<SettingsViewModel, Unit> ShowSettings { get; }
        public Interaction<SearchTermViewModel, SearchTermViewModel> GetSearchTerm { get; }
        public Interaction<GopherEntity, string?> GetSaveFilename { get; }
        public Interaction<Unit, string?> GetOpenFilename { get; }

        private static readonly SizeLimitedStack<GopherEntity> EntityHistory = new(10);

        public GopherEntity? PopGopherEntity()
        {            
            if (EntityHistory.Count > 0)
            {
                this.RaisePropertyChanged(nameof(CanGoBack));
                return EntityHistory.Pop();
            }
            return null;
        }

        public void PushGopherEntity(GopherEntity gopherEntity)
        { 
            EntityHistory.Push(gopherEntity);
            this.RaisePropertyChanged(nameof(CanGoBack));
        }

        private GopherMenu? _gopherMenu;
        public GopherMenu? GopherMenu
        {
            get => _gopherMenu;
            set
            {
                this.RaiseAndSetIfChanged(ref _gopherMenu, value);
                this.RaisePropertyChanged(nameof(ShowMenu));
                this.RaisePropertyChanged(nameof(ShowDocument));
            }
        }

        private GopherDocument? _gopherDocument;
        public GopherDocument? GopherDocument
        {
            get => _gopherDocument;
            set
            {
                this.RaiseAndSetIfChanged(ref _gopherDocument, value);
                this.RaisePropertyChanged(nameof(ShowMenu));
                this.RaisePropertyChanged(nameof(ShowDocument));
            }
        }

        public string ToggleHomeCommandHeader => CanGoHome ? "Clear Home Page" : "Set Home Page";

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        private string _urlText = string.Empty;
        public string UrlText
        {
            get => _urlText;
            set
            {
                this.RaiseAndSetIfChanged(ref _urlText, value);
                this.RaisePropertyChanged(nameof(CanGoToUrl));
            }
        }

        public bool ShowMenu => GopherMenu != null && GopherDocument == null;
        public bool ShowDocument => GopherDocument != null && GopherMenu == null;
        public static bool CanSave => EntityHistory.Count != 0;
        public static bool CanGoBack => EntityHistory.Count > 1; 
        public bool CanGoHome => !string.IsNullOrWhiteSpace(AppSettings.HomePage);

        public bool CanGoToUrl { 
            get
            {
                if (string.IsNullOrWhiteSpace(UrlText)) return false;
                if (Uri.TryCreate(UrlText, UriKind.Absolute, out var uri))
                {
                    return uri.Scheme == "gopher";
                }
                return false;
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set =>  this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public int FontSize
        {
            get => AppSettings.FontSize;
            set
            {
                if (AppSettings.FontSize != value)
                {
                    AppSettings.FontSize = value;
                    ApplicationDataStore<Settings>.Write(AppSettings);
                    this.RaisePropertyChanged();
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
                    this.RaisePropertyChanged();
                }
            }
        }

        private FontFamily? _fontFamily;
        public FontFamily FontFamily
        {
            get => _fontFamily ?? Settings.DefaultFontFamilyName;
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    this.RaisePropertyChanged();
                    FontFamilyName = _fontFamily.Name;
                }
            }
        }

        private ThemeVariant _themeVariant = ThemeVariant.Default;
        public ThemeVariant ThemeVariant
        {
            get => _themeVariant;
            set
            {
                if (_themeVariant != value)
                {
                    _themeVariant = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private Settings _settings = new();
        public Settings AppSettings
        {
            get => _settings;
            set
            {
                var fontFamilyChanged = FontFamilyName != value.FontFamilyName;
                var fontSizeChanged = FontSize != value.FontSize;
                var themeVariantChanged = ThemeVariant != value.Theme;

                _settings.HomePage = value.HomePage;
                _settings.FontFamilyName = value.FontFamilyName;
                _settings.FontSize = value.FontSize;
                _settings.Theme = value.Theme;

                if (fontFamilyChanged)
                {
                    FontFamily = new FontFamily(FontFamilyName ?? Settings.DefaultFontFamilyName);
                    this.RaisePropertyChanged(nameof(FontFamilyName));
                }
                if (fontSizeChanged) this.RaisePropertyChanged(nameof(FontSize));
                if (themeVariantChanged) ThemeVariant = _settings.Theme;
            }
        }

        public MainWindowViewModel()
        {
            AppSettings = ApplicationDataStore<Settings>.Read();
            ShowAbout = new Interaction<AboutViewModel, Unit>();
            ShowSettings = new Interaction<SettingsViewModel, Unit>();
            GetSearchTerm = new Interaction<SearchTermViewModel, SearchTermViewModel>();
            GetSaveFilename = new();
            GetOpenFilename = new();

            OpenCommand = ReactiveCommand.Create(Open);
            SaveCommand = ReactiveCommand.Create(Save);
            GoBackCommand = ReactiveCommand.Create(GoBack);
            GoHomeCommand = ReactiveCommand.Create(GoHome);
            ToggleHomePageCommand = ReactiveCommand.Create(ToggleHomePage);
            GoToUrlCommand = ReactiveCommand.Create(GoToUrl);
            ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAbout.Handle(new AboutViewModel()));
            ShowSettingsCommand = ReactiveCommand.CreateFromTask(async () => await ShowSettings.Handle(new SettingsViewModel()));
            GetSearchTermCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var input = new SearchTermViewModel();
                var result = await GetSearchTerm.Handle(input);
                return result.SearchTerm;
            });
            GetSaveFilenameCommand = ReactiveCommand.CreateFromTask<GopherEntity, string?>(async (gopherEntity) => await GetSaveFilename.Handle(gopherEntity));
            GetOpenFilenameCommand = ReactiveCommand.CreateFromTask<Unit, string?>(async (u) => await GetOpenFilename.Handle(u));

            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            if (args.Length <= 0) return;
            UrlText = args[0];
            GoToUrl();
        }

        private async void Open()
        {
            var filename = await GetOpenFilenameCommand.Execute();
            if (filename == null) return;

            await GetGopherEntity(filename);
        }

        private void Save()
        {
            if (CanSave)
                Save(EntityHistory.Peek());
        }

        private async void Save(GopherEntity entity)
        {
            var filename = await GetSaveFilenameCommand.Execute(entity);
            if (filename == null) return;

            var text = entity.IsDirectory ? GopherMenu?.ToString() : GopherDocument?.ToString();
            await File.WriteAllTextAsync(filename, text);
        }

        private async void GoBack()
        {
            // The current entity is at the top of the stack,
            // so we get the one before that
            if (CanGoBack)
            {
                _ = PopGopherEntity();
                GopherEntity entity = PopGopherEntity()!;
                await GetGopherEntity(entity);
            }
        }

        public async void GoHome()
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.HomePage))
                await GetGopherEntity(AppSettings.HomePage);
        }

        private void ToggleHomePage()
        {
            if (CanGoHome) ClearHome();
            else SetHome();
        }

        private void ClearHome()
        {
            AppSettings.HomePage = null;
            ApplicationDataStore<Settings>.Write(AppSettings);
            this.RaisePropertyChanged(nameof(CanGoHome));
        }

        private void SetHome()
        {
            AppSettings.HomePage = EntityHistory.Peek().UriString;
            ApplicationDataStore<Settings>.Write(AppSettings);
            this.RaisePropertyChanged(nameof(CanGoHome));
        }

        private async void GoToUrl()
        {
            await GetGopherEntity(UrlText);
        }

        public async Task GetGopherEntity(string entityUrlString)
        {
            await GetGopherEntity(new GopherEntity(entityUrlString, String.Empty)).ConfigureAwait(false);
        }

        public async Task GetGopherEntity(GopherEntity gopherEntity)
        {
            if (IsBusy) return;

            if (gopherEntity.IsIndexSearch && string.IsNullOrWhiteSpace(gopherEntity.SearchTerms))
            {
                gopherEntity.SearchTerms = await GetSearchTermCommand.Execute();
                if (string.IsNullOrWhiteSpace(gopherEntity.SearchTerms)) return;
            }

            StatusText = String.Empty;
            IsBusy = true;
            try
            {
                if (gopherEntity.IsLink)
                {
                    UrlText = gopherEntity.UriString;
                    PushGopherEntity(gopherEntity);
                }
                if (gopherEntity.IsHtml)
                {
                    Process.Start(new ProcessStartInfo(gopherEntity.UriString) { UseShellExecute = true });
                    return;
                }
                var bytes = await GopherLib.GopherClient.GetGopherEntity(gopherEntity).WaitAsync(new TimeSpan(10 * 10000000));
                if (gopherEntity.IsDirectory || gopherEntity.IsIndexSearch)
                {
                    GopherDocument = null;
                    GopherMenu = new GopherMenu(gopherEntity, bytes);
                }
                else if (gopherEntity.IsDocument)
                {
                    GopherMenu = null;
                    GopherDocument = new GopherDocument(gopherEntity, bytes);
                }
                else if (gopherEntity.IsBinary)
                {
                    var filename = await GetSaveFilenameCommand.Execute(gopherEntity);
                    if (filename == null) return;

                    await File.WriteAllBytesAsync(filename, bytes);
                    StatusText = $@"""{filename}"" saved!";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

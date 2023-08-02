using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Gopher.NET.Events;
using Gopher.NET.ViewModels;
using GopherLib.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Gopher.NET.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public static readonly RoutedEvent<LoadGopherRoutedEventArgs> LoadGopherRoutedEvent =
            RoutedEvent.Register<MainWindow, LoadGopherRoutedEventArgs>(nameof(LoadGopherEvent), RoutingStrategies.Bubble);

        // Provide CLR accessors for the event
        public event EventHandler<RoutedEventArgs> LoadGopherEvent
        {
            add => AddHandler(LoadGopherRoutedEvent, value);
            remove => RemoveHandler(LoadGopherRoutedEvent, value);
        }

        public MainWindow()
        {
            // Add a handler for the LoadGopherRoutedEvent event, raised by the GopherMenuView
            LoadGopherRoutedEvent.AddClassHandler<MainWindow>(OnLoadGopherEntity);

            this.WhenActivated(disposable =>
            {
                disposable(ViewModel!.ShowAbout.RegisterHandler(DoShowAboutDialogAsync));
                disposable(ViewModel!.ShowSettings.RegisterHandler(DoShowSettingsDialogAsync));
                disposable(ViewModel!.GetSearchTerm.RegisterHandler(DoShowGetSearchTermDialogAsync));
                disposable(ViewModel!.GetSaveFilename.RegisterHandler(DoShowSaveFileDialogAsync));
                disposable(ViewModel!.GetOpenFilename.RegisterHandler(DoShowOpenFileDialogAsync));
            });
            Opened += (s, e) => ViewModel!.GoHome();
            InitializeComponent();
        }

        public void OnLoadGopherEntity(object sender, LoadGopherRoutedEventArgs args)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.GetGopherEntity(args.GopherEntity).ConfigureAwait(false);
        }

        private async Task DoShowSettingsDialogAsync(InteractionContext<SettingsViewModel, Unit> interaction)
        {
            var dialog = new SettingsDialog
            {
                DataContext = interaction.Input,
            };
            dialog.SettingsChangedEvent += ((sender, args) =>
            {
                if (ViewModel != null) ViewModel.AppSettings = args.Settings;
            });
            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowAboutDialogAsync(InteractionContext<AboutViewModel, Unit> interaction)
        {
            var dialog = new AboutDialog
            {
                DataContext = interaction.Input,
            };
            var result = await dialog.ShowDialog<Unit>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowGetSearchTermDialogAsync(InteractionContext<SearchTermViewModel, SearchTermViewModel> interaction)
        {
            var dialog = new GetSearchTermDialog
            {
                DataContext = interaction.Input,
            };

            var result = await dialog.ShowDialog<SearchTermViewModel>(this);
            interaction.SetOutput(result);
        }

        private static FilePickerFileType GetGopherFileDialogFilter()
        {
            return new FilePickerFileType("Gopher files") { Patterns = new string[] { "*.gopher" } };
        }
        private static FilePickerFileType GetTextFileDialogFilter()
        {
            return new FilePickerFileType("Text files") { Patterns = new string[] { "*.txt" } };
        }
        private static FilePickerFileType GetAllFileDialogFilter()
        {
            return new FilePickerFileType("All files") { Patterns = new string[] { "*.*" } };
        }
        private static string CleanFileName(string filename)
        {
            var invalidFileChars = Path.GetInvalidFileNameChars();
            var result = new String(filename.Where(c => !invalidFileChars.Contains(c))?.ToArray());
            return result;
        }
        private async Task DoShowSaveFileDialogAsync(InteractionContext<GopherEntity, string?> interaction)
        {
            var gopherEntity = interaction.Input;

            string defaultExt;
            var fileTypes = new List<FilePickerFileType>();

            if (gopherEntity.IsDirectory)
            {
                defaultExt = "gopher";
                fileTypes.Add(GetGopherFileDialogFilter());
            }
            else if (gopherEntity.IsDocument)
            {
                defaultExt = "txt";
                fileTypes.Add(GetTextFileDialogFilter());
            }
            else if (gopherEntity.IsBinary)
            {
                defaultExt = String.Empty;
                fileTypes.Add(GetAllFileDialogFilter());
            }
            else
            {
                interaction.SetOutput(null);
                return;
            }
            var options = new FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
                DefaultExtension = defaultExt,
                FileTypeChoices = fileTypes,
                ShowOverwritePrompt = true,
                SuggestedFileName = CleanFileName(gopherEntity.DisplayText),
            };
            var result = await this.StorageProvider.SaveFilePickerAsync(options);
            interaction.SetOutput(result?.Path.LocalPath ?? null);
        }

        private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit, string?> interaction)
        {
            var fileTypes = new List<FilePickerFileType>
            {
                GetGopherFileDialogFilter(),
                GetTextFileDialogFilter(),
                GetAllFileDialogFilter(),
            };

            var options = new FilePickerOpenOptions
            {
                Title = "Open File",
                SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents),
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            };
            var results = await this.StorageProvider.OpenFilePickerAsync(options);
            interaction.SetOutput(results != null && results.Any() ? results[0].Path.ToString() : null);
        }
    }
}

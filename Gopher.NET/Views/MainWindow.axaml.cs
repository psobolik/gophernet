using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Gopher.NET.ViewModels;
using GopherLib.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Gopher.NET.Events
{
    public class LoadGopherRoutedEventArgs : RoutedEventArgs
    {
        public GopherEntity GopherEntity { get; set; }
        public LoadGopherRoutedEventArgs(GopherEntity gopherEntity)
        {
            GopherEntity = gopherEntity;
        }
    }
}

namespace Gopher.NET.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public static readonly RoutedEvent<Events.LoadGopherRoutedEventArgs> LoadGopherRoutedEvent =
            RoutedEvent.Register<MainWindow, Events.LoadGopherRoutedEventArgs>(nameof(LoadGopherEvent), RoutingStrategies.Bubble);

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

            this.WhenActivated(d =>
            {
                d(ViewModel!.ShowAbout.RegisterHandler(DoShowAboutDialogAsync));
                d(ViewModel!.GetSearchTerm.RegisterHandler(DoShowGetSearchTermDialogAsync));
                d(ViewModel!.GetSaveFilename.RegisterHandler(DoShowSaveFileDialogAsync));
                d(ViewModel!.GetOpenFilename.RegisterHandler(DoShowOpenFileDialogAsync));
            });
            this.Opened += (s, e) => ViewModel!.GoHome();
            InitializeComponent();

        }

        public void OnLoadGopherEntity(object sender, Events.LoadGopherRoutedEventArgs args)
        {
            if (DataContext is MainWindowViewModel viewModel)
                viewModel.GetGopherEntity(args.GopherEntity).ConfigureAwait(false);
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

        private static FileDialogFilter GetGopherFileDialogFilter()
        {
            return new FileDialogFilter { Name = "Gopher files", Extensions = new List<string> { "gopher" } };
        }
        private static FileDialogFilter GetTextFileDialogFilter()
        {
            return new FileDialogFilter { Name = "Text files", Extensions = new List<string> { "txt" } };
        }
        private static FileDialogFilter GetAllFileDialogFilter()
        {
            return new FileDialogFilter { Name = "All files", Extensions = new List<string> { "*" } };
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
            var filters = new List<FileDialogFilter>();

            if (gopherEntity.IsDirectory)
            {
                defaultExt = "gopher";
                filters.Add(GetGopherFileDialogFilter());
            }
            else if (gopherEntity.IsDocument)
            {
                defaultExt = "txt";
                filters.Add(GetTextFileDialogFilter());
            }
            else if (gopherEntity.IsBinary)
            {
                defaultExt = String.Empty;
                filters.Add(GetAllFileDialogFilter());
            }
            else
            {
                interaction.SetOutput(null);
                return;
            }

            var result = await new SaveFileDialog
            {
                Title = "Save File",
                Filters = filters,
                // Directory
                DefaultExtension = defaultExt,
                InitialFileName = CleanFileName(gopherEntity.DisplayText),
            }.ShowAsync(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowOpenFileDialogAsync(InteractionContext<Unit, string?> interaction)
        {
            var filters = new List<FileDialogFilter>
            {
                GetGopherFileDialogFilter(),
                GetTextFileDialogFilter(),
                GetAllFileDialogFilter(),
            };

            var results = await new OpenFileDialog
            {
                Title = "Open File",
                Filters = filters,
            }.ShowAsync(this);
            interaction.SetOutput(results != null && results.Any() ? results[0] : null);
        }
    }
}

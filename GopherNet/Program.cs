using CommandLine;
using CommandLine.Text;
using NStack;
using GopherLib;
using GopherLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using System.Reflection;

namespace GopherNet
{
    internal struct Program
    {
        //private const string GopherUrlFreaknet = @"gopher://medialab.freaknet.org/1/";
        //private const string GopherUrlFloodgap = @"gopher://gopher.floodgap.com/1/";
        //private const string GopherDocumentUrl = @"gopher://gopher.floodgap.com/0/gopher/proxy";

        public class Options
        {
            [Value(0, HelpText = "Gopher URL or file name.", MetaName = "<gopher-url>")]
            public string FileOrUrl {  get; set; }
            [Usage]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>
                    {
                        new Example("Browse Gopherspace", new Options { FileOrUrl = "[file name or gopher URL]" }),
                        new Example("Open a gopher URL", new Options { FileOrUrl = "gopher://gopher.floodgap.com"}),
                        new Example("Open a saved gopher page", new Options { FileOrUrl = "filename.gopher"}),
                    };
                }
            }

        }
        private static TextView _textView;
        private static ListView _listView;
        private static Window _window;
        private static SizeLimitedStack<GopherEntity> _entityStack;
        private static GopherContentBase _gopherContent;

        static Program()
        {
            _entityStack = new SizeLimitedStack<GopherEntity>(10);
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options =>
                {
                    MainApp(options.FileOrUrl);
                });
        }

        private static void MainApp(string gopherUriString)
        {
            Application.Init();

            _listView = CreateListView();
            _textView = CreateTextView();

            _window = CreateWindow();
            _window.Add(_textView, _listView);

            var statusBar = new StatusBar(new StatusItem[] {
                    new StatusItem(Key.Backspace, "~Backspace~ Back", () => { }), // This is for decoration only
                    new StatusItem(Key.CtrlMask | Key.O, "~^O~ Open", async () => { await Open(); }),
                    new StatusItem(Key.CtrlMask | Key.S, "~^S~ Save", SaveAs),
                    new StatusItem(Key.CtrlMask | Key.A, "~^A~ About", About),
                    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", Quit),
                });

            var top = Application.Top;
            top.Add(_window);
            top.Add(statusBar);
            top.EnsureFocus();
            top.Loaded += async () =>
            {
                if (string.IsNullOrWhiteSpace(gopherUriString))
                    await Open();
                else
                    await GetGopherEntity(gopherUriString);
            };
            Application.Run();

            static Window CreateWindow()
            {
                var window = new Window("GopherNet")
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill() - 1,
                };
                window.KeyPress += async (args) =>
                {
                    if (_listView != null)
                    {
                        if (args.KeyEvent.Key == Key.Tab)
                        {
                            args.Handled = true;
                            SelectNextGopherEntity();
                        }
                        else if (args.KeyEvent.Key == Key.BackTab)
                        {
                            args.Handled = true;
                            SelectPreviousGopherEntity();
                        }
                        else if (args.KeyEvent.Key == Key.Backspace)
                        {
                            args.Handled = true;
                            await GoBack();
                        }
                    }
                };
                return window;
            }

            static ListView CreateListView()
            {
                var listView = new ListView
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    AllowsMarking = false,
                    Visible = false,
                };
                listView.OpenSelectedItem += async (args) =>
                {
                    await GetGopherEntity(args.Value as GopherEntity);
                };
                return listView;
            }

            static TextView CreateTextView()
            {
                var textView = new TextView
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                    Visible = false,
                    ReadOnly = true,
                };
                return textView;
            }
        }

        private static void SetSelectedItem(int selectedItem, bool movingDown)
        {
            _listView.SelectedItem = selectedItem;
            _listView.SetNeedsDisplay();

            var height = _listView.Bounds.Height;
            var topItem = _listView.TopItem;

            if (selectedItem < topItem || selectedItem >= topItem + height)
            {
                _listView.TopItem = movingDown 
                    ? Math.Max((selectedItem - height) + 1, 0) 
                    : selectedItem;
            }
        }

        private static void SelectNextGopherEntity()
        {
            var list = _listView.Source.ToList();
            if (list == null || list.Count == 0) return;

            var i = _listView.SelectedItem;
            var next = i;
            do
            {
                ++next;
                if (next >= _listView.Source.Length) next = 0;
                if ((list[next] as GopherEntity).IsFetchable)
                {
                    SetSelectedItem(next, true);
                }
            }
            while (i == _listView.SelectedItem && next != i);
        }

        private static void SelectPreviousGopherEntity()
        {
            var list = _listView.Source.ToList();
            if (list == null || list.Count == 0) return;

            var i = _listView.SelectedItem;
            var next = i;
            do
            {
                --next;
                if (next < 0) next = _listView.Source.Length - 1;
                if ((list[next] as GopherEntity).IsFetchable)
                {
                    SetSelectedItem(next, false);
                }
            }
            while (i == _listView.SelectedItem && next != i);
        }

        static async Task GoBack()
        {
            var gopherEntity = PopPreviousEntity();
            if (gopherEntity != null)
            {
                await GetGopherEntity(gopherEntity);
            }
        }

        static void SaveAs()
        {
            var gopherEntity = _entityStack.Any() ? _entityStack.Peek() : null;
            SaveGopherContent(gopherEntity, System.Text.Encoding.UTF8.GetBytes(_gopherContent.ToString()));
        }

        private static void SaveGopherContent(GopherEntity gopherEntity, byte[] gopherContent)
        {
            var dlg = new SaveDialog("Save Content", "Select the path for the content")
            {
                FilePath = gopherEntity?.DisplayText,
            };
            Application.Run(dlg);

            if (!dlg.Canceled)
            {
                var filePath = dlg.FilePath.ToString();
                var extension = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension))
                {
                    if (gopherEntity.IsDirectory) filePath += ".gopher";
                    else if (gopherEntity.IsDocument) filePath += ".txt";
                }
                if (!File.Exists(filePath) || MessageBox.Query("Save File", "File already exists. Overwrite anyway?", "No", "Ok") == 1)
                {
                    try
                    {
                        File.WriteAllBytes(filePath, gopherContent);
                    }
                    catch (Exception ex)
                    {
                        ShowException(ex, "Error saving gopher content");
                    }
                }
            }
        }

        static async Task Open()
        {
            var gopherUri = GetInput("Enter the URL to open.", "gopher://");
            if (gopherUri != null) 
                await GetGopherEntity(gopherUri);
        }

        private static void About()
        {
            const string logo = @"
╔═══╗        ╔╗         ╔═╗ ╔╗     ╔╗ 
║╔═╗║        ║║         ║║╚╗║║    ╔╝╚╗
║║ ╚╝╔══╗╔══╗║╚═╗╔══╗╔═╗║╔╗╚╝║╔══╗╚╗╔╝
║║╔═╗║╔╗║║╔╗║║╔╗║║╔╗║║╔╝║║╚╗║║║╔╗║ ║║ 
║╚╩═║║╚╝║║╚╝║║║║║║║═╣║║ ║║ ║║║║║═╣ ║╚╗
╚═══╝╚══╝║╔═╝╚╝╚╝╚══╝╚╝ ╚╝ ╚═╝╚══╝ ╚═╝
         ║║                           
         ╚╝                           ";
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var copyright = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            var description = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
            MessageBox.Query("About GopherNet", $"{logo}\n{description}\nVersion {version}\n{copyright}\n\n", "Ok");
        }

        private static void Quit()
        {
            // Unless we do something to prevent it, the app is going to quit
            // when you hit ^Q anyway.
            Application.RequestStop();
        }

        private static string GetSearchTerms()
        {
            return GetInput("Enter Search Terms");
        }

        private static async Task GetGopherEntity(GopherEntity gopherEntity)
        {
            try
            {
                if (gopherEntity.IsDocument)
                {
                    ShowGopherDocument(new GopherDocument(gopherEntity, await GopherClient.GetGopherEntity(gopherEntity)));
                }
                else if (gopherEntity.IsDirectory)
                {
                    ShowGopherMenu(new GopherMenu(gopherEntity, await GopherClient.GetGopherEntity(gopherEntity)));
                }
                else if (gopherEntity.IsIndexSearch)
                {
                    // Prompt for search terms if there are none
                    if (string.IsNullOrWhiteSpace(gopherEntity.SearchTerms))
                    {
                        var searchTerms = GetSearchTerms();
                        if (!string.IsNullOrWhiteSpace(searchTerms))
                        {
                            gopherEntity.SearchTerms = searchTerms;
                        }
                        else
                        {
                            return; // Cancel if no search terms
                        }
                    }
                    ShowGopherMenu(new GopherMenu(gopherEntity, await GopherClient.GetGopherEntity(gopherEntity)));
                }
                else if (gopherEntity.IsSaveable)
                {
                    SaveGopherContent(gopherEntity, await GopherClient.GetGopherEntity(gopherEntity));
                }
            }
            catch (Exception ex)
            {
                ShowException(ex, $"Error getting gopher entity");
            }
        }

        private static async Task GetGopherEntity(string entityUrlString)
        {
            try
            {
                await GetGopherEntity(new GopherEntity(entityUrlString, null));
            }
            catch (Exception ex)
            {
                ShowException(ex, $"Error parsing URL string");
            }
        }

        private static void SetGopherEntity(GopherEntity gopherEntity)
        {
            PushEntity(gopherEntity);
            Application.MainLoop.Invoke(() =>
            {
                _window.Title = gopherEntity.ToUriString();
            });

        }

        private static void ShowGopherDocument(GopherDocument gopherDocument)
        {
            Application.MainLoop.Invoke(() =>
            {
                _gopherContent = gopherDocument;
                SetGopherEntity(gopherDocument.GopherEntity);
                _textView.Text = gopherDocument.ToString();
                _textView.Visible = true;
                _listView.Visible = false;
                _textView.SetFocus();
            });
        }

        private static void ShowGopherMenu(GopherMenu gopherMenu)
        {
            Application.MainLoop.Invoke(() =>
            {
                _gopherContent = gopherMenu;
                SetGopherEntity(gopherMenu.GopherEntity);
                _listView.Source = new GopherMenuDataSource(gopherMenu);
                _textView.Visible = false;
                _listView.Visible = true;
                _listView.SetFocus();
            });
        }

        public static void PushEntity(GopherEntity gopherEntity)
        {
            _entityStack.Push(gopherEntity);
        }

        public static GopherEntity PopPreviousEntity()
        {
            // The current entity is at the top of the stack,
            // so we get the one before that
            GopherEntity result = null;
            if (_entityStack.Count > 1)
            {
                _entityStack.Pop();
                result = _entityStack.Pop();
            }
            return result;
        }

        private static Dialog MakeInputDialog(string prompt, string seed)
        {
            var dlg = new Dialog
            {
                Title = prompt,
                Width = 50,
                Height = 6,
            };
            var textField = new TextField
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill() - 1,
                Height = 1,
                Text = seed ?? string.Empty,
            };
            dlg.Add(textField);

            var okBtn = new Button("Ok", is_default: true);
            okBtn.Clicked += () =>
            {
                dlg.Data = textField.Text.ToString();
                Application.RequestStop();
            };
            dlg.AddButton(okBtn);

            var cancelBtn = new Button("Cancel");
            cancelBtn.Clicked += () => { Application.RequestStop(); };
            dlg.AddButton(okBtn);

            textField.SetFocus();
            return dlg;
        }

        private static string GetInput(string prompt, string seed = null)
        {
            var dlg = MakeInputDialog(prompt, seed);
            Application.Run(dlg);

            return dlg.Data?.ToString();
        }

        private static void ShowException(Exception ex, string message)
        {
            MessageBox.ErrorQuery("Error", $"{message}\n{ex.Message}", "Ok");
        }
    }
}

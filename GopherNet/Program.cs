using CommandLine;
using CommandLine.Text;
using GopherLib;
using GopherLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using System.Reflection;
using Color = Terminal.Gui.Color;
using System.Diagnostics;

namespace GopherNet
{
    internal struct Program
    {
        //private const string GopherUrlFreaknet = @"gopher://medialab.freaknet.org/1/";
        //private const string GopherUrlFloodgap = @"gopher://gopher.floodgap.com/1/";
        //private const string GopherDocumentUrl = @"gopher://gopher.floodgap.com/0/gopher/proxy";

        private class Options
        {
            [Value(0, HelpText = "Gopher URL or file name.", MetaName = "<gopher-url>")]
            public string FileOrUrl { get; private init; }

            [Usage]
            // ReSharper disable once UnusedMember.Local
            // This is used in the command line parser's help text, so ignore ReSharper
            public static IEnumerable<Example> Examples =>
                new List<Example>
                {
                    new Example("Browse Gopherspace", new Options { FileOrUrl = "[file name or gopher URL]" }),
                    new Example("Open a gopher URL", new Options { FileOrUrl = "gopher://gopher.floodgap.com" }),
                    new Example("Open a saved gopher page", new Options { FileOrUrl = "filename.gopher" }),
                };
        }

        private static TextView _textView;
        private static ListView _listView;
        private static Label _label;
        private static Window _window;
        private static readonly SizeLimitedStack<GopherEntity> EntityStack;
        private static GopherContentBase _gopherContent;

        static Program()
        {
            EntityStack = new SizeLimitedStack<GopherEntity>(10);
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => { MainApp(options.FileOrUrl); });
        }

        private static void MainApp(string gopherUriString)
        {
            Application.Init();

            _window = CreateWindow();
            _listView = CreateListView();
            _textView = CreateTextView();
            _label = CreateLabel(_window);

            _window.Add(_textView, _listView, _label);

            var statusBar = new StatusBar([
                new StatusItem(Key.F1, "~F1~ About", About),
                new StatusItem(Key.Esc, "~ESC~ Back", GoBack),
                new StatusItem(Key.CtrlMask | Key.O, "~^O~ Open", Open),
                new StatusItem(Key.CtrlMask | Key.S, "~^S~ Save", SaveAs),
                new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", Quit),
            ]);

            var top = Application.Top;
            top.Add(_window);
            top.Add(statusBar);
            top.EnsureFocus();
            top.Loaded += async () =>
            {
                if (string.IsNullOrWhiteSpace(gopherUriString))
                    Open();
                else
                    await GetGopherEntity(gopherUriString);
            };
            Application.Run();
            Application.Shutdown();
            return;

            static Window CreateWindow()
            {
                var window = new Window("GopherNet")
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill() - 1,
                };
                window.KeyPress += (args) =>
                {
                    if (_listView != null)
                    {
                        if (args.KeyEvent.Key == Key.Tab || args.KeyEvent.Key == Key.n)
                        {
                            args.Handled = true;
                            SelectNextGopherEntity();
                        }
                        else if (args.KeyEvent.Key == Key.BackTab || args.KeyEvent.Key == Key.p)
                        {
                            args.Handled = true;
                            SelectPreviousGopherEntity();
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
                    Height = Dim.Fill() - 1,
                    AllowsMarking = false,
                    Visible = false,
                };
                listView.OpenSelectedItem += async (args) => { await GetGopherEntity(args.Value as GopherEntity); };
                listView.SelectedItemChanged += (args) => { ShowInfo(args.Value as GopherEntity); };
                return listView;
            }

            static TextView CreateTextView()
            {
                var textView = new TextView
                {
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill() - 0,
                    Visible = false,
                    ReadOnly = true,
                    DesiredCursorVisibility = CursorVisibility.UnderlineFix,
                    ColorScheme = new ColorScheme
                    {
                        Focus = Application.Driver.MakeAttribute(Color.White, Color.Blue),
                        Disabled = Application.Driver.MakeAttribute(Color.White, Color.Black),
                    }
                };
                return textView;
            }

            static Label CreateLabel(View view)
            {
                var label = new Label
                {
                    X = 0,
                    Y = Pos.Bottom(view) - 3,
                    Width = Dim.Fill(),
                    Height = 1,
                    Visible = true,
                    Text = "This is a test...",
                    ColorScheme = new ColorScheme
                    {
                        Normal = Terminal.Gui.Attribute.Make(Color.White, Color.BrightBlue)
                    },
                };
                return label;
            }
        }

        private static void ShowInfo(GopherEntity gopherEntity)
        {
            // Show the URI of the selected item
            if (gopherEntity.IsInfo)
            {
                _label.Text = string.Empty;
                _label.Visible = false;
            }
            else
            {
                string hint = string.Empty;
                if (gopherEntity.IsDirectory)
                {
                    hint = "Directory";
                }
                else if (gopherEntity.IsDocument)
                {
                    hint = "Document";
                }
                else if (gopherEntity.IsIndexSearch)
                {
                    hint = "Index Search";
                }
                else if (gopherEntity.IsBinary)
                {
                    hint = "Binary";
                }
                else if (gopherEntity.IsEncodedText)
                {
                    hint = "Encoded Text";
                }
                else if (gopherEntity.IsHtml)
                {
                    hint = "Web Page";
                }
                else if (!gopherEntity.IsInfo)
                {
                    hint = "Unsupported";
                }

                _label.Text = $"{gopherEntity.UriString} ({hint})";
                _label.Visible = true;
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
                if (list[next] is GopherEntity { IsClickable: true })
                {
                    SetSelectedItem(next, true);
                }
            } while (i == _listView.SelectedItem && next != i);
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
                if (list[next] is GopherEntity { IsClickable: true })
                {
                    SetSelectedItem(next, false);
                }
            } while (i == _listView.SelectedItem && next != i);
        }

        private static async void GoBack()
        {
            try
            {
                var gopherEntity = PopPreviousEntity();
                if (gopherEntity != null)
                {
                    await GetGopherEntity(gopherEntity);
                }
            }
            catch (Exception ex)
            {
                ShowException(ex, "Error opening gopher entity");
            }
        }

        private static void SaveAs()
        {
            var gopherEntity = EntityStack.Any() ? EntityStack.Peek() : null;
            SaveGopherContent(gopherEntity,
                System.Text.Encoding.UTF8.GetBytes(_gopherContent.ToString() ?? string.Empty));
        }

        private static string GenerateFileName(GopherEntity gopherEntity)
        {
            var result = string.IsNullOrEmpty(gopherEntity.DisplayText) ? "Untitled" : gopherEntity.DisplayText;
            if (result.All(c => c != '.')) result += gopherEntity.IsDirectory ? ".gopher" : ".txt";
            var invalidFileChars = Path.GetInvalidFileNameChars();
            result = new String(result.Where(c => !invalidFileChars.Contains(c)).ToArray());
            return result;
        }

        private static void SaveGopherContent(GopherEntity gopherEntity, byte[] gopherContent)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            var dlg = new SaveDialog("Save Content", "Select the path for the content")
            {
                DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
#pragma warning restore IDE0017 // Simplify object initialization
            // It doesn't work to initialize this in the constructor
            dlg.FilePath = GenerateFileName(gopherEntity);
            Application.Run(dlg);

            if (dlg.Canceled || dlg.FilePath == null) return;
            
            var filePath = dlg.FilePath.ToString();
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                if (gopherEntity is { IsDirectory: true }) filePath += ".gopher";
                else if (gopherEntity is { IsDocument: true }) filePath += ".txt";
            }
            if (!File.Exists(filePath) ||
                MessageBox.Query("Save File", "File already exists. Overwrite anyway?", "No", "Ok") == 1)
            {
                try
                {
                    if (filePath != null)
                    {
                        File.WriteAllBytes(filePath, gopherContent);
                        MessageBox.Query("Success", $"Saved '{filePath}'", "Ok");
                    }
                }
                catch (Exception ex)
                {
                    ShowException(ex, "Error saving gopher content");
                }
            }
        }

        private static async void Open()
        {
            try
            {
                var gopherUri = GetInput("Enter the URL to open.", "gopher://");
                if (gopherUri != null)
                    await GetGopherEntity(gopherUri);
            }
            catch (Exception ex)
            {
                ShowException(ex, "Error opening gopher URL");
            }
        }

        private static void About()
        {
            const string logo = """
╔═══╗        ╔╗         ╔═╗ ╔╗     ╔╗ 
║╔═╗║        ║║         ║║╚╗║║    ╔╝╚╗
║║ ╚╝╔══╗╔══╗║╚═╗╔══╗╔═╗║╔╗╚╝║╔══╗╚╗╔╝
║║╔═╗║╔╗║║╔╗║║╔╗║║╔╗║║╔╝║║╚╗║║║╔╗║ ║║ 
║╚╩═║║╚╝║║╚╝║║║║║║║═╣║║ ║║ ║║║║║═╣ ║╚╗
╚═══╝╚══╝║╔═╝╚╝╚╝╚══╝╚╝ ╚╝ ╚═╝╚══╝ ╚═╝
         ║║                           
         ╚╝                           
""";
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly == null
                ? string.Empty 
                : assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var copyright = (assembly == null)
                ? string.Empty
                : assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            var description = (assembly == null)
                ? string.Empty
                : assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?
                    .Description;
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
            _label.Text = string.Empty;
            _label.Visible = false;
            try
            {
                if (gopherEntity.IsHtml)
                {
                    Process.Start(new ProcessStartInfo(gopherEntity.UriString) { UseShellExecute = true });
                }
                else if (gopherEntity.IsDocument)
                {
                    ShowGopherDocument(new GopherDocument(gopherEntity,
                        await GopherClient.GetGopherEntity(gopherEntity)));
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
            Application.MainLoop.Invoke(() => { _window.Title = gopherEntity.UriString; });
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

        private static void PushEntity(GopherEntity gopherEntity)
        {
            EntityStack.Push(gopherEntity);
        }

        private static GopherEntity PopPreviousEntity()
        {
            // The current entity is at the top of the stack,
            // so we get the one before that
            
            // Nothing above current, do nothing
            if (EntityStack.Count <= 1) return null;
            
            EntityStack.Pop();
            return EntityStack.Pop();
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
            textField.CursorPosition = textField.Text.Length;
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
using GopherLib.Models;
using NStack;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace GopherNet
{
    public class GopherMenuDataSource : IListDataSource
    {
        private const char DirectoryIcon = '\x25b6'; // Black Right-Pointing Triangle
        private const char IndexSearchIcon = '\x263c'; // White Sun with Rays
        private const char DocumentIcon = '\x25a0'; // Black Square
        private const char BinaryIcon = '\x2193'; // Downwards Arrow
        private const char EncodedTextIcon = '\x2193'; // Downwards Arrow
        private const char UnsupportedIcon = '\x00d7'; // Multiplication Sign
        private const char BlankIcon = '\x0020'; // Space
        private const char WebPageIcon = '\x25a1'; // White Square

        List<GopherEntity> GopherEntities { get; set; }

        public GopherMenuDataSource(GopherMenu gopherMenu)
        {
            GopherEntities = gopherMenu.GopherEntities.ToList();
        }

        public int Count => GopherEntities?.Count ?? 0;

        public int Length => Count;

        public bool IsMarked(int item)
        {
            // Marking isn't supported
            return false;
        }

        // This is lifted from Terminal.GUI's ListView implementation
        void RenderUstr(ConsoleDriver driver, ustring ustr, int width)
        {
            var byteLen = ustr.Length;
            var used = 0;
            for (var i = 0; i < byteLen;)
            {
                var (rune, size) = Utf8.DecodeRune(ustr, i, i - byteLen);
                var count = System.Rune.ColumnWidth(rune);
                if (used + count > width)
                    break;
                driver.AddRune(rune);
                used += count;
                i += size;
            }

            for (; used < width; used++)
            {
                driver.AddRune(' ');
            }
        }

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line,
            int width, int start = 0)
        {
            container.Move(col, line);
            var gopherEntity = GopherEntities[item];
            if (gopherEntity == null)
            {
                RenderUstr(driver, ustring.Make(""), width);
            }
            else
            {
                {
                    char icon;
                    if (gopherEntity.IsDirectory)
                    {
                        icon = DirectoryIcon;
                    }
                    else if (gopherEntity.IsDocument)
                    {
                        icon = DocumentIcon;
                    }
                    else if (gopherEntity.IsIndexSearch)
                    {
                        icon = IndexSearchIcon;
                    }
                    else if (gopherEntity.IsBinary)
                    {
                        icon = BinaryIcon;
                    }
                    else if (gopherEntity.IsEncodedText)
                    {
                        icon = EncodedTextIcon;
                    }
                    else if (gopherEntity.IsInfo)
                    {
                        icon = BlankIcon;
                    }
                    else if (gopherEntity.IsHtml)
                    {
                        icon = WebPageIcon;
                    }
                    else // Interface or unknown; Unsupported
                    {
                        icon = UnsupportedIcon;
                    }
                    var text = $"{icon} {gopherEntity.DisplayText}";
                    RenderUstr(driver, text, width);
                }
            }
        }

        public void SetMark(int item, bool value)
        {
            // Do nothing, marking isn't supported
        }

        public IList ToList()
        {
            return GopherEntities;
        }
    }
}
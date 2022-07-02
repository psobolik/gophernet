using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopherLib.Models
{
    public class GopherEntity
    {
        // Canonical types (per RFC 1436)
        public const char DocumentTypeChar = '0';
        public const char DirectoryTypeChar = '1';
        public const char PhoneBookTypeChar = '2';
        public const char ErrorTypeChar = '3';
        public const char BinHexTypeChar = '4';
        public const char DosBinaryTypeChar = '5';
        public const char UuencodedTypeChar = '6';
        public const char IndexSearchTypeChar = '7';
        public const char TelnetTypeChar = '8';
        public const char BinaryTypeChar = '9';
        public const char MirrorTypeChar = '+';
        public const char GifTypeChar = 'g';
        public const char ImageTypeChar = 'I';
        public const char Telnet3270TypeChar = 'T';
        // Gopher+ types (per Wikipedia)
        public const char BitmapTypeChar = ':';
        public const char MovieTypeChar = ';';
        public const char SoundTypeChar = '<';
        // Non-canonical types (per Wikipedia)
        public const char DocTypeChar = 'd';
        public const char HtmlTypeChar = 'h';
        public const char WavTypeChar = 's';
        public const char InfoTypeChar = 'i';

        public string UriString { get => ToUriString(); }
        public Uri Uri { get => ToUri(); }

        public char Type { get; set; }
        public string DisplayText { get; set; }
        public string Selector { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Ignore { get; set; }
        public string SearchTerms { get; set; }

        public bool IsDirectory { get { return Type == DirectoryTypeChar; } }
        public bool IsDocument { get { return Type == DocumentTypeChar; } }
        public bool IsIndexSearch { get { return Type == IndexSearchTypeChar; } }
        public bool IsInfo { get { return Type == InfoTypeChar; } }
        public bool IsLink { get { return IsDirectory || IsDocument || IsIndexSearch; } }
        public bool IsInterface // Unsupported
        {
            get
            {
                return Type == PhoneBookTypeChar
                    || Type == ErrorTypeChar
                    || Type == TelnetTypeChar
                    || Type == InfoTypeChar
                    || Type == Telnet3270TypeChar
                    || Type == HtmlTypeChar
                    ;
            }
        }
        public bool IsBinary
        {
            get
            {
                return Type == DosBinaryTypeChar
                    || Type == BinaryTypeChar
                    || Type == GifTypeChar
                    || Type == ImageTypeChar
                    || Type == BitmapTypeChar
                    || Type == MovieTypeChar
                    || Type == SoundTypeChar
                    || Type == DocTypeChar
                    || Type == WavTypeChar
                    ;
            }
        }
        public bool IsEncodedText
        {
            get
            {
                return Type == BinHexTypeChar
                    || Type == UuencodedTypeChar
                    ;
            }
        }

        public bool IsFetchable { get { return IsLink || IsBinary || IsEncodedText; } }

        public bool IsSaveable { get {  return IsBinary || IsEncodedText; } }

        public bool IsGopherScheme { get { return Scheme == null || Scheme.ToLower() == "gopher"; } }
        public bool IsFileScheme { get { return Scheme != null && Scheme.ToLower() == "file"; } }

        // Create a Gopher Entity from an entity string
        public GopherEntity(string entityString)
        {
            string firstPart = null;
            string selector = null;
            string host = null;
            string portString = null;
            string ignore = null;

            var parts = entityString.Split(new char[] { '\t' }, 5);
            switch (parts.Length)
            {
                case 5:
                    ignore = parts[4];
                    goto case 4;
                case 4:
                    portString = parts[3];
                    goto case 3;
                case 3:
                    host = parts[2];
                    goto case 2;
                case 2:
                    selector = parts[1];
                    goto case 1;
                case 1:
                    firstPart = parts[0];
                    goto default;
                default:
                    break;
            }

            (Type, DisplayText) = ExtractType(firstPart);
            Selector = selector;
            Host = host;
            Port = ParsePort(portString);
            Ignore = ignore;
        }

        // Create a Gopher Entity from a uri string, with a display text value
        public GopherEntity(string uriString, string displayText)
        {
            // Assumes a relative path is a local file
            if (Uri.IsWellFormedUriString(uriString, UriKind.Relative))
            {
                uriString = System.IO.Path.GetFullPath(uriString);
            }
            Init(new Uri(uriString), displayText);
        }

        public void Init(Uri uri, string displayText = null)
        {
            Scheme = uri.Scheme;
            if (IsFileScheme)
            {
                var localPath = uri.LocalPath;
                Type = localPath.EndsWith(".gopher") ? DirectoryTypeChar : DocumentTypeChar;
                DisplayText = Uri.UnescapeDataString(System.IO.Path.GetFullPath(localPath));
            }
            else
            {
                (Type, Selector) = ParsePath(uri.AbsolutePath);
                SearchTerms = Uri.UnescapeDataString(ExtractSearchTerm(uri.Fragment));
                DisplayText = displayText ?? string.Empty;
                Host = uri.Host;
                Port = uri.Port;
            }
        }

        public GopherEntity(Uri uri, string displayText = null)
        {
            Init(uri, displayText);
        }
        // Remove the # from a URI fragment if there is one
        private static string ExtractSearchTerm(string query)
        {
            return query != null && query.Length > 1 && query[0] == '#'
                ? query.Substring(1)
                : query;
        }

        // Separate the Type and the Selector from a URL path string
        private static (char, string) ParsePath(string path)
        {
            char typeChar = GopherEntity.DirectoryTypeChar;
            string selector = path;

            var pathComponents = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathComponents.Length > 0)
            {
                if (pathComponents[0].Length == 1)
                {
                    typeChar = pathComponents[0][0];
                    selector = pathComponents.Length > 1
                        ? $"/{string.Join("/", pathComponents.Skip(1).ToArray())}"
                        : selector = string.Empty;
                }
            }
            return (typeChar, selector);
        }

        private static int ParsePort(string portString)
        {
            int port = 70;
            int.TryParse(portString, out port);
            return port;
        }

        private static (char, string) ExtractType(string value)
        {
            char type = ErrorTypeChar;
            string displayText = string.Empty;

            if (value.Length > 0)
            {
                type = value[0];
                if (value.Length > 1) displayText = value.Substring(1);
            }
            return (type, displayText);
        }

        public Uri ToUri()
        {
            if (IsGopherScheme)
            {
                var selector = $"/{Type}";
                if (!string.IsNullOrWhiteSpace(Selector))
                {
                    if (!Selector.StartsWith('/'))
                        selector += '/';
                    selector += Selector;
                }
                string extraValue = string.IsNullOrWhiteSpace(SearchTerms) ? null : $"#{SearchTerms}";

                return new UriBuilder("gopher", Host, Port, selector, extraValue).Uri;
            }
            else
            {
                return new UriBuilder("file", null, 0, DisplayText).Uri;
            }
        }

        public string ToUriString()
        {
            return ToUri().ToString();
        }

        public override string ToString()
        {
            var result = $"{Type}{DisplayText}\t{Selector}\t{Host}\t{Port}";
            if (!string.IsNullOrEmpty(Ignore))
                result += $"\t{Ignore}";
            return result;
        }
    }
}

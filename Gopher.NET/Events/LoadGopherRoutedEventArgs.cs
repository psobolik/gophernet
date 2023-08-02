using Avalonia.Interactivity;
using GopherLib.Models;

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

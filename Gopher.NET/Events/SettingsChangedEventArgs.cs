using Avalonia.Interactivity;
using Gopher.NET.Models;

namespace Gopher.NET.Events
{
    public class SettingsChangedEventArgs : RoutedEventArgs
    {
        public Settings Settings { get; set; }
        public SettingsChangedEventArgs(Settings settings)
        {
            Settings = settings;
        }
    }
}

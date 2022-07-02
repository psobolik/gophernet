using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GopherLib.Models;
using ReactiveUI;
using System;
using System.Reactive;

namespace Gopher.NET.Controls
{
    public partial class GopherMenuView : UserControl
    {
        // A command for the link buttons to bind to
        public ReactiveCommand<GopherEntity, Unit> GetGopherEntityCommand { get; }

        public GopherMenuView()
        {
            // Create the command, Linked a method to handle it
            GetGopherEntityCommand = ReactiveCommand.Create<GopherEntity>(RaiseLoadGopherEvent);
            InitializeComponent();
        }

        // When a link button is clicked, we raise an event and let its listeners
        // handle it. (That will be the Main Window)
        public void RaiseLoadGopherEvent(GopherEntity gopherEntity)
        {
            var eventArgs = new Events.LoadGopherRoutedEventArgs(gopherEntity)
            {
                Source = this,
                RoutedEvent = Views.MainWindow.LoadGopherRoutedEvent,
            };
            RaiseEvent(eventArgs);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

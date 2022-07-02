using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gopher.NET.ViewModels;
using ReactiveUI;
using System;

namespace Gopher.NET.Views
{
    public partial class AboutDialog : ReactiveWindow<AboutViewModel>
    {
        public AboutDialog()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.OkCommand.Subscribe((u) => { this.Close(); })));

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}


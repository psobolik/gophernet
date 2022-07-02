using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Gopher.NET.ViewModels;
using ReactiveUI;
using System;

namespace Gopher.NET.Views
{
    public partial class GetSearchTermDialog : ReactiveWindow<SearchTermViewModel>
    {
        public GetSearchTermDialog()
        {
            InitializeComponent();
            Opened += (s, e) => this.Get<TextBox>("SearchTermTextBox")?.Focus();
            this.WhenActivated(d => d(ViewModel!.OkCommand.Subscribe(Close)));
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

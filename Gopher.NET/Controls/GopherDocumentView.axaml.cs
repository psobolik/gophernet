using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Gopher.NET.Controls
{
    public partial class GopherDocumentView : UserControl
    {
        public GopherDocumentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

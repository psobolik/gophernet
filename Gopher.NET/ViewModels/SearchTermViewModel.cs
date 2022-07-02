using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Gopher.NET.ViewModels
{
    public class SearchTermViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, SearchTermViewModel> OkCommand { get; }

        private string? _searchTerm;

        public string? SearchTerm 
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        public SearchTermViewModel()
        {
            OkCommand = ReactiveCommand.Create(() => { return this; });
        }
    }
}

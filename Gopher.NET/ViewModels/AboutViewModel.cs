using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;

namespace Gopher.NET.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        public string Name { get; } = String.Empty;
        public string Version { get; } = String.Empty;
        public string Copyright { get; } = String.Empty;

        public AboutViewModel()
        {
            OkCommand = ReactiveCommand.Create(() => { return Unit.Default; });

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            if (assemblyName != null)
            {
                Version = $"{assemblyName!.Version!.Major}.{assemblyName!.Version!.Minor}.{assemblyName!.Version!.MajorRevision}.{assemblyName!.Version!.MinorRevision}";
            }
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fileVersionInfo != null)
            {
                Copyright = fileVersionInfo!.LegalCopyright!;
                Name = fileVersionInfo!.ProductName!;
            }
        }
    }
}

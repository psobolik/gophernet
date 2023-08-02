using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Gopher.NET.Events;
using Gopher.NET.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gopher.NET.Views;

public partial class SettingsDialog : ReactiveWindow<SettingsViewModel>
{
    public event EventHandler<SettingsChangedEventArgs>? SettingsChangedEvent;

    public SettingsDialog()
    {
        this.WhenActivated(disposable =>
        {
            ViewModel!.SettingsChangedEvent += ((s, a) =>
            {
                SettingsChangedEvent?.Invoke(this, new SettingsChangedEventArgs(a.Settings));
            });
        });
        InitializeComponent();
    }
}
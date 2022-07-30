using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class PersonSelectDialogueWindow : ReactiveWindow<PersonSelectWindowViewModel> {
    public PersonSelectDialogueWindow() {
        InitializeComponent();
        #if DEBUG
        this.AttachDevTools();
        #endif

        this.WhenActivated(async (disposables) => { /* Handle view activation etc. */
            await ViewModel.RefreshDataAsync();
            disposables(ViewModel!.DoneCommand.Subscribe(Close));
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
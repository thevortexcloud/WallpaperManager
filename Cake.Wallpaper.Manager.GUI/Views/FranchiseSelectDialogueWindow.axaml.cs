using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class FranchiseSelectDialogueWindow : ReactiveWindow<FranchiseSelectWindowViewModel> {
    public FranchiseSelectDialogueWindow() {
        InitializeComponent();
        #if DEBUG
        this.AttachDevTools();
        #endif

        this.WhenActivated(d => d(ViewModel!.DoneCommand.Subscribe(Close)));
        this.WhenActivated(async o => { o(this.ViewModel!.RefreshDataAsync()); });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
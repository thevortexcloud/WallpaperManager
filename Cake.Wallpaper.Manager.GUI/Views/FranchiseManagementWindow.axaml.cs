using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class FranchiseManagementWindow : ReactiveWindow<FranchiseManagementViewModel> {
    public FranchiseManagementWindow() {
        InitializeComponent();
        #if DEBUG
        this.AttachDevTools();
        #endif

        this.WhenActivated(d => d(ViewModel!.LoadDataAsync()));
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
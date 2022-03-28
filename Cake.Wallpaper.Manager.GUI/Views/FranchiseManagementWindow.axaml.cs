using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cake.Wallpaper.Manager.GUI.Views; 

public partial class FranchiseManagementWindow : Window {
    public FranchiseManagementWindow() {
        InitializeComponent();
        #if DEBUG
        this.AttachDevTools();
        #endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
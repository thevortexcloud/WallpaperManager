using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cake.Wallpaper.Manager.GUI.Views; 

public partial class PersonManagamentWindow : Window {
    public PersonManagamentWindow() {
        InitializeComponent();
        #if DEBUG
        this.AttachDevTools();
        #endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
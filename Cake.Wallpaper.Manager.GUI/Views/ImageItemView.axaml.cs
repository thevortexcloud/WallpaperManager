using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class ImageItemView : UserControl {
    public ImageItemView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
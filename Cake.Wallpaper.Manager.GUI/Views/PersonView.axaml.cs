using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class PersonView : UserControl {
    public PersonView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) {
    }
}
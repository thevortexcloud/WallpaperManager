using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_OnClick(object? sender, RoutedEventArgs e) {
            if (this.DataContext is MainWindowViewModel mainWindowViewModel) {
                mainWindowViewModel.SelectedImage?.People?.Remove(mainWindowViewModel.SelectedImage.SelectedPerson);
            }
        }

        private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e) {
        }
    }
}
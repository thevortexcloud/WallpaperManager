using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e) {
            (DataContext as MainWindowViewModel).LoadInitialData(sender, e);
        }
    }
}
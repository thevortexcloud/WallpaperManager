using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            InitializeComponent();
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e) {
            (DataContext as MainWindowViewModel).LoadInitialData(sender, e);
        }

        private void MenuItem_OpenPersonManagement_OnClick(object? sender, RoutedEventArgs e) {
            new PersonManagamentWindow() {
                DataContext = new PersonManagementViewModel()
            }.Show(this);
        }

        private void MenuItem_OnClick(object? sender, RoutedEventArgs e) {
            new FranchiseManagementWindow() {
                DataContext = new FranchiseManagementViewModel()
            }.Show(this);
        }
    }
}
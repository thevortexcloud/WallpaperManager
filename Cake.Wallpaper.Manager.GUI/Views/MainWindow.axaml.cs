using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowFranchiseSelectDialog.RegisterHandler(DoShowFranchiseDialogAsync)));
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e) {
            (DataContext as MainWindowViewModel).LoadInitialData(sender, e);
        }

        private async Task DoShowFranchiseDialogAsync(InteractionContext<Unit, FranchiseSelectDialogueWindowViewModel?> interaction) {
            var dialog = new FranchiseSelectDialogueWindow();
            dialog.DataContext = new FranchiseSelectDialogueWindowViewModel();

            var result = await dialog.ShowDialog<FranchiseSelectDialogueWindowViewModel?>(this);
            interaction.SetOutput(result);
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
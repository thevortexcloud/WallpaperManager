using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.ViewModel!.ShowFranchiseSelectDialog.RegisterHandler(DoShowFranchiseDialogAsync)); });
            this.WhenActivated(d => d(this.ViewModel!.ShowPersonSelectDialog.RegisterHandler(DoShowPersonDialogueAsync)));
        }

        private async Task DoShowPersonDialogueAsync(InteractionContext<Unit, PersonSelectWindowViewModel?> interactionContext) {
            //TODO: Figure out how to resolve this without using a locator
            //   var window = Locator.Current.GetService<PersonSelectDialogueWindow>();
            var window = new PersonSelectDialogueWindow() {
                DataContext = new PersonSelectWindowViewModel(Locator.Current.GetService<IWallpaperRepository>())
            };
            var result = await window.ShowDialog<PersonSelectWindowViewModel>(this);
            interactionContext.SetOutput(result);
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
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        public MainWindow() {
            InitializeComponent();
            this.WhenActivated(d => { d(this.ViewModel!.ShowFranchiseSelectDialog.RegisterHandler(DoShowFranchiseDialogAsync)); });
            this.WhenActivated(d => d(this.ViewModel!.ShowPersonSelectDialog.RegisterHandler(DoShowPersonDialogueAsync)));
        }

        private async Task DoShowPersonDialogueAsync(InteractionContext<Unit, PersonSelectWindowViewModel?> interactionContext) {
            try {
                interactionContext.SetOutput(await WindowHelper.ShowDialogueAsync<PersonSelectDialogueWindow, PersonSelectWindowViewModel>(this));
            } catch (Exception ex) {
                await this.ShowExceptionMessageBoxAsync("There was a problem showing the dialogue", ex);
            }
        }

        private async Task DoShowFranchiseDialogAsync(InteractionContext<Unit, FranchiseSelectWindowViewModel?> interaction) {
            try {
                interaction.SetOutput(await WindowHelper.ShowDialogueAsync<FranchiseSelectDialogueWindow, FranchiseSelectWindowViewModel>(this));
            } catch (Exception ex) {
                await this.ShowExceptionMessageBoxAsync("There was a problem showing the dialogue", ex);
            }
        }

        private async void MenuItem_OpenPersonManagement_OnClick(object? sender, RoutedEventArgs e) {
            try {
                WindowHelper.ShowWindow<PersonManagementWindow>(this);
            } catch (Exception ex) {
                await this.ShowExceptionMessageBoxAsync("There was a problem showing the window", ex);
            }
        }

        private async void MenuItem_OpenFranchiseManagement_OnClick(object? sender, RoutedEventArgs e) {
            try {
                WindowHelper.ShowWindow<FranchiseManagementWindow>(this);
            } catch (Exception ex) {
                await this.ShowExceptionMessageBoxAsync("There was a problem showing the window", ex);
            }
        }
    }
}
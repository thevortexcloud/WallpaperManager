using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.Views;

public partial class PersonView : ReactiveUserControl<PersonViewModel> {
    public PersonView() {
        InitializeComponent();

        this.WhenActivated(d => d(this.ViewModel!.LoadDataAsync()));
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}
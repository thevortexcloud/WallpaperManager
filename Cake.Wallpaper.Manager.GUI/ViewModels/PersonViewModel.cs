using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class PersonViewModel : ViewModelBase {
    private bool _shouldDelete;
    public string? Name { get; set; }
    public string? Franchise { get; set; }

    public bool ShouldDelete {
        get => this._shouldDelete;
        set => this.RaiseAndSetIfChanged(ref this._shouldDelete, value);
    }

}
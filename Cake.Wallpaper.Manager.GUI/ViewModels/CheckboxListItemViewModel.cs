using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class CheckboxListItemViewModel : ViewModelBase {
    private bool _selected;
    private string? _label;

    public bool Selected {
        get => this._selected;
        set => this.RaiseAndSetIfChanged(ref this._selected, value);
    }

    public string? Label {
        get => this._label;
        set => this.RaiseAndSetIfChanged(ref this._label, value);
    }

    public object? Value { get; set; }
}
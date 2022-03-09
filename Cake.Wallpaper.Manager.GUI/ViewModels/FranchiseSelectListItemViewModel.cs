using System.Collections.ObjectModel;
using System.Linq;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class FranchiseSelectListItemViewModel : ViewModelBase {
    private readonly Franchise _franchise;
    private bool _selected;
    public Franchise? Franchise => this._franchise;

    public string Name => this._franchise.Name;

    public bool Selected {
        get => this._selected;
        set => this.RaiseAndSetIfChanged(ref this._selected, value);
    }

    public ObservableCollection<FranchiseSelectListItemViewModel> ChildFranchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();


    public FranchiseSelectListItemViewModel(Franchise franchise, bool selected = false) {
        this._franchise = franchise;
        this.Selected = selected;

        if (this._franchise.ChildFranchises.Any()) {
            ChildFranchises.AddRange(this._franchise.ChildFranchises.Select(o => new FranchiseSelectListItemViewModel(o, selected)));
        }
    }
}
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Templates;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class FranchiseSelectListItemViewModel : ViewModelBase {
    private readonly Franchise _franchise;
    private bool _selected;
    public Franchise? Franchise => this._franchise;

    public int? ParentID {
        get => this.Franchise?.ParentID;
        set {
            if (value != this.Franchise?.ParentID && this.Franchise is not null) {
                this.Franchise.ParentID = value;
                this.RaisePropertyChanged(nameof(ParentID));
            }
        }
    }

    public string? Name {
        get => Franchise?.Name;
        set {
            if (value != this.Franchise?.Name && this.Franchise is not null) {
                this.Franchise.Name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }
    }

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
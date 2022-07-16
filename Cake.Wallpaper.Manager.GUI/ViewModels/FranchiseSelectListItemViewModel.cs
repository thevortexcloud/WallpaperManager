using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Templates;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class FranchiseSelectListItemViewModel : ViewModelBase {
    #region Private readonly variables
    private readonly Franchise _franchise;
    #endregion

    #region Private variables
    private bool _selected;
    #endregion

    #region Public properties
    /// <summary>
    /// Returns the current backing franchise for this instance
    /// </summary>
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

    /// <summary>
    /// Returns the ID of the current franchise, or 0 if this is a new franchise
    /// </summary>
    public int ID => this.Franchise?.ID ?? 0;

    /// <summary>
    /// Returns the name of the franchise
    /// </summary>
    public string? Name {
        get => Franchise?.Name;
        set {
            if (value != this.Franchise?.Name && this.Franchise is not null) {
                this.Franchise.Name = value;
                this.RaisePropertyChanged(nameof(Name));
            }
        }
    }

    /// <summary>
    /// Gets/sets a value indicating if this option is selected
    /// </summary>
    public bool Selected {
        get => this._selected;
        set => this.RaiseAndSetIfChanged(ref this._selected, value);
    }

    /// <summary>
    /// Returns the child franchises for this franchise
    /// </summary>
    public ObservableCollection<FranchiseSelectListItemViewModel> ChildFranchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();
    #endregion

    #region Public constructor
    public FranchiseSelectListItemViewModel(Franchise franchise, bool selected = false) {
        this._franchise = franchise;
        this.Selected = selected;

        if (this._franchise.ChildFranchises.Any()) {
            ChildFranchises.AddRange(this._franchise.ChildFranchises.Select(o => new FranchiseSelectListItemViewModel(o, selected)));
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Returns a list of all selected child franchises for the current instance
    /// </summary>
    /// <returns>A list of all selected and child franchises</returns>
    public List<FranchiseSelectListItemViewModel> FindSelectedChildFranchises() {
        return FindSelectedFranchises(this.ChildFranchises);
    }
    #endregion

    #region Private static methods
    /// <summary>
    /// Returns a list of all selected child franchises for the given instance
    /// </summary>
    /// <returns>A list of all selected and child franchises</returns>
    /// <param name="franchiseManagementViewModels">The list of franchises to search</param>
    /// <remarks>
    /// Due to all the nesting we have going on, we have to use recursion to figure this one out
    /// </remarks>
    private static List<FranchiseSelectListItemViewModel> FindSelectedFranchises(IEnumerable<FranchiseSelectListItemViewModel> franchiseManagementViewModels) {
        if (!franchiseManagementViewModels?.Any() ?? true) {
            return new List<FranchiseSelectListItemViewModel>();
        }

        var result = new List<FranchiseSelectListItemViewModel>();
        foreach (var model in franchiseManagementViewModels) {
            if (model.ChildFranchises?.Any() ?? false) {
                result.Add(FindSelectedFranchises(model.ChildFranchises));
            }

            //Need to clear the observable list since it's used for display reasons
            model.ChildFranchises?.Clear();
            //And we also need to clear the backing list in case something tries to use it
            model._franchise.ChildFranchises.Clear();

            if (model.Selected) {
                result.Add(model);
            }
        }

        return result;
    }
    #endregion
}
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
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
    /// Returns the margin that should be used to indent this instance in the UI
    /// </summary>
    public Thickness Margin { get; }

    /// <summary>
    /// Returns the current backing franchise for this instance
    /// </summary>
    public Franchise? Franchise => this._franchise;

    /// <summary>
    /// Returns the depth in the hierarchy of the franchise this instance represents
    /// </summary>
    public int Depth => this.Franchise.Depth;

    /// <summary>
    /// Gets/sets the parent ID of the franchise this instance represents
    /// </summary>
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
    #endregion

    #region Public constructor
    public FranchiseSelectListItemViewModel(Franchise franchise, bool selected = false) {
        this._franchise = franchise;
        this.Selected = selected;
        //For every level down the hierarchy, we need to add 15 to the margins
        //This makes the franchise list significantly easier to read and to easily understand the hierachy
        this.Margin = new Thickness(this.Franchise.Depth * 15, 0, 0, 0);
    }
    #endregion

    #region Ovrride default object methods
    public override bool Equals(object? obj) {
        //We only care if we are equal to another instance, and we consider anything with the same ID to functionally refer to the same bit of data
        if (obj is FranchiseSelectListItemViewModel viewModel) {
            return this.ID == viewModel.ID;
        } else {
            return false;
        }
    }

    public override int GetHashCode() {
        //We consider anything with the same ID to functionally refer to the same bit of data, as such the hashcode can just be the ID to make sure anything that relies on it will ensure some level of uniqueness
        return this.ID;
    }
    #endregion
}
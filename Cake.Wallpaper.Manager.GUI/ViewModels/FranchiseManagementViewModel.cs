using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public sealed class FranchiseManagementViewModel : ViewModelBase {
    #region Private readonly variables
    private readonly IWallpaperRepository _wallpaperRepository;
    #endregion

    #region Private variables
    private FranchiseSelectListItemViewModel? _selectedFranchise;
    private string? _searchText;
    #endregion

    #region Public properties
    /// <summary>
    /// Returns the list of franchises to display
    /// </summary>
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    /// <summary>
    /// Gets/sets the current search filter text
    /// </summary>
    public string? SearchText {
        get => this._searchText;
        set => this.RaiseAndSetIfChanged(ref this._searchText, value);
    }

    /// <summary>
    /// Returns a command which allows for the user to set the parent ID of a franchise
    /// </summary>
    public ReactiveCommand<Unit, Unit> SetParent { get; }

    /// <summary>
    /// Returns a command which allows for a user to create a new franchise
    /// </summary>
    public ReactiveCommand<Unit, Unit> NewFranchise { get; }

    /// <summary>
    /// Returns a command which allows for a user to delete the selected franchise
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteFranchise { get; }

    /// <summary>
    /// Returns a command which allows for a user to save all franchises
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveFranchises { get; }

    /// <summary>
    /// Gets/sets the currently selected franchise
    /// </summary>
    public FranchiseSelectListItemViewModel? SelectedFranchise {
        get => this._selectedFranchise;
        set => this.RaiseAndSetIfChanged(ref this._selectedFranchise, value);
    }
    #endregion

    #region Public constructor
    public FranchiseManagementViewModel(IWallpaperRepository wallpaperRepository) {
        this._wallpaperRepository = wallpaperRepository;
        SetParent = ReactiveCommand.Create(SetParentHandler);
        NewFranchise = ReactiveCommand.Create(NewFranchiseHandler);
        SaveFranchises = ReactiveCommand.Create(SaveFranchisesHandler);
        DeleteFranchise = ReactiveCommand.CreateFromTask(DeleteFranchisesHandlerAsync);

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(600))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(this.DoSearchAsync);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Attempts to delete all selected franchises from the repository
    /// </summary>
    private async Task DeleteFranchisesHandlerAsync() {
        try {
            var toDelete = this.Franchises.Where(o => o.Selected).ToList();
            foreach (var franchise in toDelete) {
                await this._wallpaperRepository.DeleteFranchiseAsync(franchise.ID);
            }

//Just remove from the list if everything passes without error
            this.Franchises.RemoveMany(toDelete);
        } catch (Exception ex) {
            await Common.ShowExceptionMessageBoxAsync("There was a problem deleting the franchise", ex);
            //However if we do get an error we need to reload since this could have failed part way through
            await this.LoadDataAsync(this.SearchText);
        }
    }

    /// <summary>
    /// Attempts to perform a search operation with the given search term
    /// </summary>
    /// <param name="value">The value to search</param>
    private async void DoSearchAsync(string? value) {
        if (value is null) {
            return;
        }

        await this.LoadDataAsync(value);
    }

    /// <summary>
    /// Attempts to save all franchises to the repository
    /// </summary>
    private async void SaveFranchisesHandler() {
        await _wallpaperRepository.SaveFranchiseInfosAsync(Franchises.Select(o => o.Franchise));
        await this.LoadDataAsync();
    }

    /// <summary>
    /// Adds a new franchise to the franchise list and selects it
    /// </summary>
    private void NewFranchiseHandler() {
        var newfranchise = new FranchiseSelectListItemViewModel(new Franchise());
        this.Franchises.Add(newfranchise);
        this.SelectedFranchise = newfranchise;
    }

    /// <summary>
    /// Attempts to set the checked franchise's parent to the selected franchise
    /// </summary>
    private void SetParentHandler() {
        if (this.SelectedFranchise is null) {
            return;
        }

        var selectedfranchises = Franchises.Where(o => o.Selected).ToList();
        //Make sure the user has exactly one selected
        if (selectedfranchises.Count != 1) {
            return;
        }

        //A franchise can't be a parent of itself
        if (this.SelectedFranchise == selectedfranchises.FirstOrDefault()) {
            return;
        }

        this.SelectedFranchise.ParentID = selectedfranchises.FirstOrDefault()?.Franchise?.ID ?? 0;
    }

    /// <summary>
    /// Attempts to load data to display with the given search term
    /// </summary>
    /// <param name="searchTerm">The search term to filter with</param>
    public async Task LoadDataAsync(string? searchTerm = null) {
        this.Franchises.Clear();
        IAsyncEnumerable<Franchise>? franchises = null;
        franchises = string.IsNullOrWhiteSpace(searchTerm) ? this._wallpaperRepository.RetrieveFranchises() : this._wallpaperRepository.RetrieveFranchises(searchTerm);
        if (franchises is null || !await franchises.AnyAsync()) {
            return;
        }

        await foreach (var franchise in franchises.Select(o => new FranchiseSelectListItemViewModel(o))) {
            Franchises.Add(franchise);
        }
    }
    #endregion
}
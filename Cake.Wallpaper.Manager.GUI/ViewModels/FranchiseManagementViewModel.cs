using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using System.Linq;
using System.Reactive;
using Cake.Wallpaper.Manager.Core;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData.Binding;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public sealed class FranchiseManagementViewModel : ViewModelBase {
    private readonly IWallpaperRepository _wallpaperRepository;
    private FranchiseSelectListItemViewModel? _selectedFranchise;
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    public ReactiveCommand<Unit, Unit> SetParent { get; }
    public ReactiveCommand<Unit, Unit> NewFranchise { get; }
    public ReactiveCommand<Unit, Unit> SaveFranchises { get; }

    public FranchiseSelectListItemViewModel? SelectedFranchise {
        get => this._selectedFranchise;
        set => this.RaiseAndSetIfChanged(ref this._selectedFranchise, value);
    }

    public FranchiseManagementViewModel() {
        this._wallpaperRepository = (IWallpaperRepository?) Locator.Current.GetService(typeof(IWallpaperRepository));
        SetParent = ReactiveCommand.Create(SetParentHandler);
        NewFranchise = ReactiveCommand.Create(NewFranchiseHandler);
        SaveFranchises = ReactiveCommand.Create(SaveFranchisesHandler);
        LoadDataAsync();
    }

    private async void SaveFranchisesHandler() {
        await _wallpaperRepository.SaveFranchiseInfosAsync(Franchises.Select(o => o.Franchise));
        await this.LoadDataAsync();
    }

    private void NewFranchiseHandler() {
        var newfranchise = new FranchiseSelectListItemViewModel(new Franchise());
        this.Franchises.Add(newfranchise);
        this.SelectedFranchise = newfranchise;
    }

    private void SetParentHandler() {
        if (this.SelectedFranchise is null) {
            return;
        }

        var selectedfranchises = Franchises.Where(o => o.Selected).ToList();
        if (selectedfranchises.Count != 1) {
            return;
        }

        if (this.SelectedFranchise == selectedfranchises.FirstOrDefault()) {
            return;
        }

        this.SelectedFranchise.ParentID = selectedfranchises.FirstOrDefault()?.Franchise?.ID ?? 0;
    }

    private async Task LoadDataAsync() {
        this.Franchises.Clear();
        foreach (var franchise in DataUtilities.FlattenFranchiseList(this._wallpaperRepository.RetrieveFranchises().ToEnumerable()).Select(o => new FranchiseSelectListItemViewModel(o))) {
            Franchises.Add(franchise);
        }
    }
}
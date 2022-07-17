using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData.Binding;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public sealed class FranchiseManagementViewModel : ViewModelBase {
    private readonly IWallpaperRepository _wallpaperRepository;
    private FranchiseSelectListItemViewModel? _selectedFranchise;
    private string _searchText;
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    public string SearchText {
        get => this._searchText;
        set => this.RaiseAndSetIfChanged(ref this._searchText, value);
    }

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

        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(600))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(this.DoSearchAsync);

        LoadDataAsync();
    }

    private async void DoSearchAsync(string? value) {
        if (value is null) {
            return;
        }

        await this.LoadDataAsync(value);
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

    private async Task LoadDataAsync(string? searchTerm = null) {
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
}
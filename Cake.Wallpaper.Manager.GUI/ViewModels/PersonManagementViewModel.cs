using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class PersonManagementViewModel : ViewModelBase {
    private readonly IWallpaperRepository _wallpaperRepository;
    private PersonViewModel? _selectedPerson;
    private string _franchiseSearchTerm;

    public ObservableCollection<PersonViewModel> People { get; } = new ObservableCollection<PersonViewModel>();
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    public PersonViewModel? SelectedPerson {
        get => this._selectedPerson;
        set => this.RaiseAndSetIfChanged(ref this._selectedPerson, value);
    }

    public ReactiveCommand<Unit, Unit> SavePerson { get; }

    public ReactiveCommand<Unit, Unit> NewPerson { get; }

    public string FranchiseSearchTerm {
        get => this._franchiseSearchTerm;
        set => this.RaiseAndSetIfChanged(ref this._franchiseSearchTerm, value);
    }

    public PersonManagementViewModel() {
        this._wallpaperRepository = Locator.Current.GetService<IWallpaperRepository>();

        SavePerson = ReactiveCommand.Create(SavePersonAsync);
        NewPerson = ReactiveCommand.Create(CreateNewPersonAsync);

        this.WhenAnyValue(o => o.FranchiseSearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(HandleFranchiseSearchAsync);

        this.WhenAnyValue(o => o.SelectedPerson)
            .Subscribe(HandleSelectedPersonAsync);

        this.LoadDataAsync();
    }

    private async void HandleSelectedPersonAsync(PersonViewModel? personViewModel) {
        if (personViewModel is null) {
            return;
        }

        if (personViewModel.ID == 0) {
            foreach (var model in personViewModel.FlattenFranchiseList(Franchises)) {
                model.Selected = false;
            }

            return;
        }

        var flatfranchiselist = personViewModel.FlattenFranchiseList();
        foreach (var model in personViewModel.FlattenFranchiseList(Franchises)) {
            if (flatfranchiselist.Select(o => o.Franchise).Contains(model.Franchise)) {
                model.Selected = true;
            } else {
                model.Selected = false;
            }
        }
    }

    private async void HandleFranchiseSearchAsync(string term) {
        if (SelectedPerson is null) {
            return;
        }

        if (term is null) {
            return;
        }
    }

    private async void CreateNewPersonAsync() {
        var person = new Person();
        var viewmodel = new PersonViewModel(person, this._wallpaperRepository);
        People.Add(viewmodel);
        SelectedPerson = viewmodel;
    }

    public async void SavePersonAsync() {
        await SelectedPerson.SavePersonAsync(SelectedPerson.FlattenFranchiseList(this.Franchises).Where(o => o.Selected).Select(o => o.Franchise));
    }


    private async void LoadDataAsync() {
        await foreach (var person in this._wallpaperRepository.RetrievePeopleAsync()) {
            var personviewmodel = new PersonViewModel(person, this._wallpaperRepository);
            People.Add(personviewmodel);
        }

        var franchises = this._wallpaperRepository.RetrieveFranchises();
        await foreach (var franchise in franchises) {
            this.Franchises.Add(new FranchiseSelectListItemViewModel(franchise));
        }
    }
}
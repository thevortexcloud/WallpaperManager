using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class PersonManagementViewModel : ViewModelBase {
    #region Private readonly variables
    private readonly IWallpaperRepository _wallpaperRepository;
    #endregion

    #region Private variables
    private PersonViewModel? _selectedPerson;
    private string? _franchiseSearchTerm;
    private string? _personSearchTerm;
    #endregion

    #region Public properties
    public ObservableCollection<PersonViewModel> People { get; } = new ObservableCollection<PersonViewModel>();
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    public PersonViewModel? SelectedPerson {
        get => this._selectedPerson;
        set => this.RaiseAndSetIfChanged(ref this._selectedPerson, value);
    }

    public ReactiveCommand<Unit, Unit> SavePerson { get; }

    public ReactiveCommand<Unit, Unit> NewPerson { get; }
    public ReactiveCommand<Unit, Unit> DeletePerson { get; }

    public string? FranchiseSearchTerm {
        get => this._franchiseSearchTerm;
        set => this.RaiseAndSetIfChanged(ref this._franchiseSearchTerm, value);
    }

    public string? PersonSearchTerm {
        get => this._personSearchTerm;
        set => this.RaiseAndSetIfChanged(ref this._personSearchTerm, value);
    }
    #endregion

    #region Public constructor
    public PersonManagementViewModel(IWallpaperRepository wallpaperRepository) {
        this._wallpaperRepository = wallpaperRepository;

        SavePerson = ReactiveCommand.CreateFromTask(SavePersonAsync);
        NewPerson = ReactiveCommand.Create(CreateNewPerson);
        DeletePerson = ReactiveCommand.CreateFromTask(DeletePersonAsync);

        this.WhenAnyValue(o => o.FranchiseSearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(HandleFranchiseSearchAsync);

        this.WhenAnyValue(o => o.PersonSearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(HandlePersonSearchAsync);


        this.WhenAnyValue(o => o.SelectedPerson)
            .Subscribe(OnSelectedPersonChange);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Deletes the currently selected person from the current <see cref="_wallpaperRepository"/>
    /// </summary>
    private async Task DeletePersonAsync() {
        if (this.SelectedPerson is null) {
            return;
        }

        if (this.SelectedPerson.ID is 0) {
            this.People.Remove(this.SelectedPerson);
            return;
        }

        await (this.SelectedPerson?.DeletePersonAsync() ?? Task.CompletedTask);
        await this.RefreshDataAsync();
    }

    /// <summary>
    ///  Handles updating all related values to the selected person being changes
    /// </summary>
    /// <param name="personViewModel">The new person to update the this instance with</param>
    private void OnSelectedPersonChange(PersonViewModel? personViewModel) {
        if (personViewModel is null) {
            return;
        }

        if (personViewModel.ID == 0) {
            foreach (var model in Franchises) {
                model.Selected = false;
            }

            return;
        }

        var franchiseList = personViewModel.FranchiseSelectListItemViewModels;
        foreach (var model in Franchises) {
            model.Selected = franchiseList.Select(o => o.Franchise).Contains(model.Franchise);
        }
    }

    private async void HandleFranchiseSearchAsync(string? term) {
        //If this is actually null we don't want to do anything, however empty string is perfectly valid
        if (term is null) {
            return;
        }

        await this.RefreshDataAsync();
    }

    private async void HandlePersonSearchAsync(string? term) {
        try {
            //If this is actually null we don't want to do anything, however empty string is perfectly valid
            if (term is null) {
                return;
            }

            await this.RefreshDataAsync();
        } catch (Exception ex) {
            await Common.ShowExceptionMessageBoxAsync("There was a problem filtering the person list", ex);
        }
    }

    /// <summary>
    /// Attempts to create a new person view model and add it to the current instance
    /// </summary>
    private void CreateNewPerson() {
        var person = new Person();
        var viewmodel = new PersonViewModel(person, false, this._wallpaperRepository);
        People.Add(viewmodel);
        SelectedPerson = viewmodel;
    }

    /// <summary>
    /// Reloads all the data in the current instance
    /// </summary>
    public async Task RefreshDataAsync() {
        //TODO: We can optimise this by caching the old search terms and checking if anything actually changed
        //Clear out the old stuff we no longer need
        People.Clear();
        Franchises.Clear();

        //Fetch the people based on the current search term and then add them to the people list
        var peopleViewModels = this.PersonSearchTerm is null ? this._wallpaperRepository.RetrievePeopleAsync() : this._wallpaperRepository.RetrievePeopleAsync(this.PersonSearchTerm);
        if (peopleViewModels is not null) {
            await foreach (var person in peopleViewModels) {
                var personviewmodel = new PersonViewModel(person, true, this._wallpaperRepository);
                People.Add(personviewmodel);
            }
        }

        //Fetch the franchises and add them to the franchise list
        var franchises = this.FranchiseSearchTerm is null ? this._wallpaperRepository.RetrieveFranchises() : this._wallpaperRepository.RetrieveFranchises(this.FranchiseSearchTerm);
        //If we have nothing to load, don't do anything
        if (franchises is null) {
            return;
        }

        await foreach (var franchise in franchises) {
            this.Franchises.Add(new FranchiseSelectListItemViewModel(franchise));
        }
    }

    /// <summary>
    /// Attempts to saves the currently selected person to the wallpaper repository
    /// </summary>
    private async Task SavePersonAsync() {
        try {
            if (this.SelectedPerson is null) {
                return;
            }

            await SelectedPerson.SavePersonAsync(this.Franchises.Where(o => o.Selected).Select(o => o.Franchise));
            await this.RefreshDataAsync();
        } catch (Exception ex) {
            await Common.ShowExceptionMessageBoxAsync("There was a problem saving a person", ex);
        }
    }
    #endregion
}
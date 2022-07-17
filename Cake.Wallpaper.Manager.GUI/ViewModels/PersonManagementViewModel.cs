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
    private string _franchiseSearchTerm;
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

    public string FranchiseSearchTerm {
        get => this._franchiseSearchTerm;
        set => this.RaiseAndSetIfChanged(ref this._franchiseSearchTerm, value);
    }
    #endregion

    #region Public constructor
    public PersonManagementViewModel() {
        //TODO: DON'T USE LOCATOR PATTERN
        this._wallpaperRepository = Locator.Current.GetService<IWallpaperRepository>();

        SavePerson = ReactiveCommand.Create(SavePersonAsync);
        NewPerson = ReactiveCommand.Create(CreateNewPerson);
        DeletePerson = ReactiveCommand.Create(DeletePersonAsync);

        this.WhenAnyValue(o => o.FranchiseSearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(HandleFranchiseSearchAsync);

        this.WhenAnyValue(o => o.SelectedPerson)
            .Subscribe(OnSelectedPersonChange);

        this.LoadDataAsync();
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Deletes the currently selected person from the current <see cref="_wallpaperRepository"/>
    /// </summary>
    private async void DeletePersonAsync() {
        if (this.SelectedPerson is null) {
            return;
        }

        if (this.SelectedPerson.ID is 0) {
            this.People.Remove(this.SelectedPerson);
            return;
        }

        await this.SelectedPerson?.DeletePersonAsync();
        await this.LoadDataAsync();
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

    private async void HandleFranchiseSearchAsync(string term) {
        //TODO: Finish implementing this
        if (SelectedPerson is null) {
            return;
        }

        if (term is null) {
            return;
        }
    }

    /// <summary>
    /// Attempts to create a new person view model and add it to the current instance
    /// </summary>
    private void CreateNewPerson() {
        var person = new Person();
        var viewmodel = new PersonViewModel(person, this._wallpaperRepository);
        People.Add(viewmodel);
        SelectedPerson = viewmodel;
    }

    /// <summary>
    /// Reloads all the data in the current instance
    /// </summary>
    private async Task LoadDataAsync() {
        //Clear out the old stuff we no longer need
        People.Clear();
        Franchises.Clear();

        //Fetch the people and then add them to the people list
        await foreach (var person in this._wallpaperRepository.RetrievePeopleAsync()) {
            var personviewmodel = new PersonViewModel(person, this._wallpaperRepository);
            People.Add(personviewmodel);
        }

        //Fetch the franchises and add them to the franchise list
        var franchises = this._wallpaperRepository.RetrieveFranchises();
        await foreach (var franchise in franchises) {
            this.Franchises.Add(new FranchiseSelectListItemViewModel(franchise));
        }
    }
    #endregion

    #region Public methods
    public async void SavePersonAsync() {
        await SelectedPerson.SavePersonAsync(this.Franchises.Where(o => o.Selected).Select(o => o.Franchise));
        await this.LoadDataAsync();
    }
    #endregion
}
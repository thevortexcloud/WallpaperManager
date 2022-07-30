using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class PersonViewModel : ViewModelBase {
    #region Public Properties
    /// <summary>
    /// Gets/sets the name of the person
    /// </summary>
    public string? Name {
        get => this._person?.Name;
        set {
            //TODO: Look into why this is not just using set if changed?
            if (value != this._person.Name) {
                this.RaisePropertyChanged();
                this._person.Name = value;
            }
        }
    }

    public Franchise? PrimaryFranchise => this._person.PrimaryFranchise;
    public Person Person => this._person;
    public ObservableCollection<FranchiseSelectListItemViewModel> FranchiseSelectListItemViewModels { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();
    public int? ID => this._person.ID;
    #endregion

    #region Private readonly variables
    private readonly Person _person;
    private readonly IWallpaperRepository _wallpaperRepository;
    #endregion

    #region Public constructor
    public PersonViewModel(Person person, IWallpaperRepository wallpaperRepository) {
        this._person = person;
        this._wallpaperRepository = wallpaperRepository;
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Loads the list of franchises this person belongs to
    /// </summary>
    public async Task LoadDataAsync() {
        FranchiseSelectListItemViewModels.Clear();
        await foreach (var franchise in this._wallpaperRepository.RetrieveFranchisesForPerson(this._person.ID)) {
            FranchiseSelectListItemViewModels.Add(new FranchiseSelectListItemViewModel(franchise, true));
        }
    }

    /// <summary>
    /// Attempts to delete the person from the repository
    /// </summary>
    public async Task DeletePersonAsync() {
        try {
            //Prevent people from trying to delete stuff from the DB if this is a new person
            if (this._person.ID == 0) {
                return;
            }

            await this._wallpaperRepository.DeletePersonAsync(this._person.ID);
        } catch (Exception ex) {
            await Common.ShowExceptionMessageBoxAsync("There was a problem deleting the person", ex);
        }
    }

    /// <summary>
    /// Attempts to save the person with the selected franchise list
    /// </summary>
    /// <param name="franchises">The franchises the person has been assigned to</param>
    public async Task SavePersonAsync(IEnumerable<Franchise> franchises) {
        _person.Franchises.Clear();
        _person.Franchises.UnionWith(franchises);
        await this._wallpaperRepository.SavePersonInfoAsync(this._person);

        this.FranchiseSelectListItemViewModels.Clear();
        await LoadDataAsync();
    }
    #endregion

    #region Default object method overrides
    public override bool Equals(object? obj) {
        //We only care if we are equal to another instance, and we consider anything with the same ID to functionally refer to the same bit of data
        if (obj is PersonViewModel viewModel) {
            return this.ID == viewModel.ID;
        } else {
            return false;
        }
    }

    public override int GetHashCode() {
        return this.ID ?? 0;
    }
    #endregion
}
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
    public string? Name {
        get => this._person?.Name;
        set {
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

    private readonly Person _person;
    private readonly IWallpaperRepository _wallpaperRepository;


    public PersonViewModel(Person person, IWallpaperRepository wallpaperRepository) {
        this._person = person;
        this._wallpaperRepository = wallpaperRepository;
        //TODO:Move this into an activator
        LoadDataAsync().ConfigureAwait(false).GetAwaiter();
    }

    public async Task LoadDataAsync() {
        FranchiseSelectListItemViewModels.Clear();
        await foreach (var franchise in this._wallpaperRepository.RetrieveFranchisesForPerson(this._person.ID)) {
            FranchiseSelectListItemViewModels.Add(new FranchiseSelectListItemViewModel(franchise, true));
        }
    }

    public async Task SavePersonAsync() {
        _person.Franchises.Clear();
        _person.Franchises.UnionWith(this.FranchiseSelectListItemViewModels.Where(o => o.Selected).Select(o => o.Franchise));
        await this._wallpaperRepository.SavePersonInfoAsync(this._person);
    }

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

    public async Task SavePersonAsync(IEnumerable<Franchise> franchises) {
        _person.Franchises.Clear();
        _person.Franchises.UnionWith(franchises);
        await this._wallpaperRepository.SavePersonInfoAsync(this._person);

        this.FranchiseSelectListItemViewModels.Clear();
        await LoadDataAsync();
    }
}
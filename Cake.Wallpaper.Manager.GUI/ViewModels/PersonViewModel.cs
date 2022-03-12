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
    public ObservableCollection<FranchiseSelectListItemViewModel> FranchiseSelectListItemViewModels { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();
    public int? ID => this._person.ID;

    private readonly Person _person;
    private readonly IWallpaperRepository _wallpaperRepository;


    public PersonViewModel(Person person, IWallpaperRepository wallpaperRepository) {
        this._person = person;
        this._wallpaperRepository = wallpaperRepository;
        LoadDataAsync();
    }

    public async Task LoadDataAsync() {
        FranchiseSelectListItemViewModels.Clear();
        await foreach (var franchise in this._wallpaperRepository.RetrieveFranchisesForPerson(this._person.ID)) {
            FranchiseSelectListItemViewModels.Add(new FranchiseSelectListItemViewModel(franchise, true));
        }
    }

    public IEnumerable<FranchiseSelectListItemViewModel> FlattenFranchiseList() {
        return ViewModelUtilities.FlattenFranchiseList(this.FranchiseSelectListItemViewModels);
    }

    public async Task SavePersonAsync() {
        _person.Franchises.Clear();
        _person.Franchises.UnionWith(ViewModelUtilities.FlattenFranchiseList(this.FranchiseSelectListItemViewModels).Where(o => o.Selected).Select(o => o.Franchise));
        await this._wallpaperRepository.SavePersonInfoAsync(this._person);
    }

    public async Task DeletePersonAsync() {
        if (this._person.ID == 0) {
            return;
        }

        await this._wallpaperRepository.DeletePersonAsync(this._person.ID);
    }

    public async Task SavePersonAsync(IEnumerable<Franchise> franchises) {
        _person.Franchises.Clear();
        _person.Franchises.UnionWith(franchises);
        await this._wallpaperRepository.SavePersonInfoAsync(this._person);

        this.FranchiseSelectListItemViewModels.Clear();
        await LoadDataAsync();
    }
}
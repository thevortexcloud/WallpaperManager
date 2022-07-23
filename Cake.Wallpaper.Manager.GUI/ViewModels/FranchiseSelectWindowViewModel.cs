using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class FranchiseSelectWindowViewModel : ViewModelBase {
    private readonly IWallpaperRepository _wallpaperRepository;

    public ObservableCollection<FranchiseSelectListItemViewModel> DbFranchises { get; set; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    //public List<FranchiseSelectListItemViewModel> SelectedFranchiseSelectListItemViewModels => this.FindSelectedFranchises();
    public ReactiveCommand<Unit, FranchiseSelectWindowViewModel> DoneCommand { get; }

    public FranchiseSelectWindowViewModel(IWallpaperRepository wallpaperRepository) {
        this._wallpaperRepository = wallpaperRepository;

        this.LoadData();
        this.DoneCommand = ReactiveCommand.Create(() => this);
    }

    private void LoadData() {
        this.DbFranchises = new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
    }
}
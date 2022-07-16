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

public class FranchiseSelectDialogueWindowViewModel : ViewModelBase {
    private readonly IWallpaperRepository _wallpaperRepository;

    public ObservableCollection<FranchiseSelectListItemViewModel> DbFranchises { get; set; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    //public List<FranchiseSelectListItemViewModel> SelectedFranchiseSelectListItemViewModels => this.FindSelectedFranchises();
    public ReactiveCommand<Unit, FranchiseSelectDialogueWindowViewModel> DoneCommand { get; }

    public FranchiseSelectDialogueWindowViewModel() {
        this._wallpaperRepository = Locator.Current.GetService<IWallpaperRepository>();

        this.LoadData();
        this.DoneCommand = ReactiveCommand.CreateFromTask(() => {
            //this.SelectedFranchiseSelectListItemViewModels.AddRange(this.SelectedFranchiseSelectListItemViewModels);
            return Task.FromResult(this);
        });
    }

    /// <summary>
    /// Finds all currently selected child franchises on the window
    /// </summary>
    /// <returns>A hierarchical list of selected franchises</returns>
    public List<FranchiseSelectListItemViewModel> FindSelectedFranchises() {
        var result = new List<FranchiseSelectListItemViewModel>();
        foreach (var model in this.DbFranchises) {
            result.AddRange(model.FindSelectedFranchises());
            model.ChildFranchises.Clear();
            //Add top level franchises
            if (model.Selected) {
                result.Add(model);
            }
        }

        return result;
    }


    private void LoadData() {
        this.DbFranchises = new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
    }
}
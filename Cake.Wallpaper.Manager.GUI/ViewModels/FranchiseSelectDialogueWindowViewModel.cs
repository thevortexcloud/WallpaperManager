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
    public ObservableCollection<FranchiseSelectListItemViewModel> SelectedFranchiseSelectListItemViewModels { get; set; } = new ObservableCollection<FranchiseSelectListItemViewModel>();
    public ReactiveCommand<Unit, FranchiseSelectDialogueWindowViewModel> DoneCommand { get; }

    public FranchiseSelectDialogueWindowViewModel() {
        this._wallpaperRepository = Locator.Current.GetService<IWallpaperRepository>();

        this.LoadData();
        this.DoneCommand = ReactiveCommand.CreateFromTask(() => {
            this.SelectedFranchiseSelectListItemViewModels.AddRange(this.DbFranchises.Where(o => o.Selected));
            return Task.FromResult(this);
        });
    }


    private void LoadData() {
        this.DbFranchises = new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class FranchiseSelectWindowViewModel : ViewModelBase {
    #region Private readonly methods
    private readonly IWallpaperRepository _wallpaperRepository;
    #endregion

    #region Public properties
    /// <summary>
    /// Returns the list of franchises to display to the user that they may select from
    /// </summary>
    /// <remarks>This list may be filtered and not contain all possible values that exist in the repository</remarks>
    public SourceCache<FranchiseSelectListItemViewModel, int> DbFranchises { get; private set; } = new SourceCache<FranchiseSelectListItemViewModel, int>(o => o.ID);

    /// <summary>
    /// Returns a list of franchises the user has selected
    /// </summary>
    public ObservableCollection<FranchiseSelectListItemViewModel> SelectedFranchiseSelectListItemViewModels { get; private set; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    /// <summary>
    /// A command that fires when the user clicks done
    /// </summary>
    public ReactiveCommand<Unit, FranchiseSelectWindowViewModel> DoneCommand { get; }
    #endregion

    #region Public constructor
    public FranchiseSelectWindowViewModel(IWallpaperRepository wallpaperRepository) {
        this._wallpaperRepository = wallpaperRepository;
        //Set up our observer on the franchise list so we can be informed whenever someone checks the selected box
        this.DbFranchises.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .AutoRefresh(o => o.Selected) //This will not work without the auto refresh watching the specific property we want
            .Subscribe(OnFranchiseSelected);

        this.LoadData();
        //When the user clicks done, we just want to return this entire instance so they can decide what to do with the data
        this.DoneCommand = ReactiveCommand.Create(() => this);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Handles the given list of list changes
    /// </summary>
    /// <param name="changes">The changes to check and act on</param>
    private void OnFranchiseSelected(IChangeSet<FranchiseSelectListItemViewModel, int> changes) {
        foreach (var change in changes) {
            switch (change.Reason) {
                //Currently we only care about the refresh state
                case ChangeReason.Refresh:
                    //Check if the user has selected or not, and if they have add it to the selected list
                    if (change.Current.Selected) {
                        this.SelectedFranchiseSelectListItemViewModels.Add(change.Current);
                    } else {
                        //Otherwise remove it from the list 
                        this.SelectedFranchiseSelectListItemViewModels.Remove(change.Current);
                    }

                    break;
            }
        }
    }

    //TODO:Make this async and load with an activator
    //TODO:Allow for filtering
    /// <summary>
    /// Attempts to load the list of franchises for the user
    /// </summary>
    private void LoadData() {
        this.DbFranchises.AddOrUpdate(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
        //this.DbFranchises = new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
    }
    #endregion
}
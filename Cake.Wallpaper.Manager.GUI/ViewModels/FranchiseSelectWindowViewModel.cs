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
    #region Private variables
    private string _searchText;
    #endregion

    #region Private readonly variables
    private readonly IWallpaperRepository _wallpaperRepository;
    private readonly ReadOnlyObservableCollection<FranchiseSelectListItemViewModel> _dbFranchises;
    #endregion

    #region Private properties
    /// <summary>
    /// Returns the list of franchises to display to the user that they may select from
    /// </summary>
    /// <remarks>This list may be filtered and not contain all possible values that exist in the repository</remarks>
    private SourceCache<FranchiseSelectListItemViewModel, int> DbFranchisesSourceCache { get; } = new SourceCache<FranchiseSelectListItemViewModel, int>(o => o.ID);

    private ReactiveCommand<string?, Unit> SearchCommand { get; }
    #endregion

    #region Public properties
    /// <summary>
    /// Returns the bound list of franchises to display to the user that they may select from. This is intended to be used by external consumers such as a GUI
    /// </summary>
    /// <remarks>This list may be filtered and not contain all possible values that exist in the repository</remarks>
    public ReadOnlyObservableCollection<FranchiseSelectListItemViewModel> DbFranchises => this._dbFranchises;

    /// <summary>
    /// Returns a list of franchises the user has selected
    /// </summary>
    public ObservableCollection<FranchiseSelectListItemViewModel> SelectedFranchiseSelectListItemViewModels { get; private set; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    /// <summary>
    /// A command that fires when the user clicks done
    /// </summary>
    public ReactiveCommand<Unit, FranchiseSelectWindowViewModel> DoneCommand { get; }

    /// <summary>
    /// Gets/sets the current search filter
    /// </summary>
    public string? SearchText {
        get => this._searchText;
        set => this.RaiseAndSetIfChanged(ref this._searchText, value, nameof(SearchText));
    }
    #endregion

    #region Public constructor
    public FranchiseSelectWindowViewModel(IWallpaperRepository wallpaperRepository) {
        this._wallpaperRepository = wallpaperRepository;
        //Set up our observer on the franchise list so we can be informed whenever someone checks the selected box
        this.DbFranchisesSourceCache.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .AutoRefresh(o => o.Selected) //This will not work without the auto refresh watching the specific property we want
            .Bind(out this._dbFranchises) //We have to bind or the search filtering does not cause the UI to update
            .Subscribe(OnFranchiseSelected);

        //Set up our search handler
        SearchCommand = ReactiveCommand.CreateFromTask<string?>(this.OnSearchAsync);

        this.WhenAnyValue(o => o.SearchText)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .InvokeCommand(SearchCommand);

        //When the user clicks done, we just want to return this entire instance so they can decide what to do with the data
        this.DoneCommand = ReactiveCommand.Create(() => this);
    }
    #endregion
    #region Public methods
    /// <summary>
    /// Attempts tp refresh the currently displayed data 
    /// </summary>
    public async Task RefreshDataAsync() {
        await this.LoadDataAsync(this.SearchText);
    }
    #endregion
    
    #region Private methods
    /// <summary>
    /// Attempts to filter the current franchise list based on the given search term
    /// </summary>
    /// <param name="term">The search term to filter by</param>
    private async Task OnSearchAsync(string? term) {
        if (term is null) {
            return;
        }

        await this.LoadDataAsync(term);
    }

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

    /// <summary>
    /// Attempts to load the list of franchises for the user
    /// </summary>
    /// <param name="term">The search term to search with</param>
    private async Task LoadDataAsync(string? term = null) {
        this.DbFranchisesSourceCache.Clear();
        var franchises = string.IsNullOrWhiteSpace(term) ? this._wallpaperRepository.RetrieveFranchises() : this._wallpaperRepository.RetrieveFranchises(term);
        if (franchises != null) {
            await foreach (var item in franchises.Select(o => new FranchiseSelectListItemViewModel(o))) {
                this.DbFranchisesSourceCache.AddOrUpdate(item);
                //this.DbFranchises = new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaperRepository.RetrieveFranchises().Select(o => new FranchiseSelectListItemViewModel(o)).ToEnumerable());
            }
        }
    }
    #endregion
}
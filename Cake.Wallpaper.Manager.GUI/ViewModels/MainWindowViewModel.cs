using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        #region Private variables
        private string _searchText;
        private ImageItemViewModel? _selectedImage;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentPage = 0;
        #endregion

        #region Private readonly variables
        /// <summary>
        /// The current wallpaper repository
        /// </summary>
        private readonly IWallpaperRepository _wallpaperRepository;

        /// <summary>
        /// A shared semaphore to prevent race conditions during save and data retrieval operations
        /// </summary>
        private readonly SemaphoreSlim _slim = new SemaphoreSlim(1, 1);
        #endregion

        #region Private properties
        /// <summary>
        /// Gets/sets a value indicating we are currently busy loading images
        /// </summary>
        private bool IsLoadingImages { get; set; }

        /// <summary>
        /// Gets/sets a list of images retrieved from the <see cref="_wallpaperRepository"/>
        /// </summary>
        private List<ImageItemViewModel> Images { get; } = new List<ImageItemViewModel>();

        /// <summary>
        /// Gets/sets the last value the user searched for using the main search box
        /// </summary>
        private string LastSearchTerm { get; set; }
        #endregion

        #region Private constants
        /// <summary>
        /// The maximum number of images the user can see at a time
        /// </summary>
        /// <remarks>
        /// Setting this to a high value will significantly increase memory consumption and load times for pages
        /// </remarks>
        private const int PAGE_SIZE = 60;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets/sets the current page for the image list that the user is on
        /// </summary>
        public int CurrentPage {
            get => this._currentPage;
            private set {
                this.RaiseAndSetIfChanged(ref this._currentPage, value);
                //HACK: Raise this change here as it will really only ever change when the current page changes
                this.RaisePropertyChanged(nameof(PageInfoString));
            }
        }

        /// <summary>
        /// Returns the total number of pages the user can see
        /// </summary>
        public int TotalPages => (int) Math.Ceiling(((double?) this.Images?.Count / (double) PAGE_SIZE) ?? 0);

        /// <summary>
        /// Returns a string showing the current page and the total number of pages
        /// </summary>
        public string PageInfoString => $"{CurrentPage}/{TotalPages}";

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextImagePage { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PreviousImagePage { get; }
        public ReactiveCommand<Unit, Unit> Refresh { get; }

        /// <summary>
        /// Returns a list of images that the user can see and interact with
        /// </summary>
        public ObservableCollection<ImageItemViewModel> CurrentPageData { get; } = new ObservableCollection<ImageItemViewModel>();

        /// <summary>
        /// Gets/sets the current image a user has selected
        /// </summary>
        public ImageItemViewModel? SelectedImage {
            get => this._selectedImage;
            set => this.RaiseAndSetIfChanged(ref this._selectedImage, value);
        }

        /// <summary>
        /// Gets/sets the current search text for the main search box on the image list
        /// </summary>
        public string SearchText {
            get => this._searchText;
            set => this.RaiseAndSetIfChanged(ref this._searchText, value);
        }

        public Interaction<Unit, FranchiseSelectWindowViewModel?> ShowFranchiseSelectDialog { get; } = new Interaction<Unit, FranchiseSelectWindowViewModel>();
        public Interaction<Unit, PersonSelectWindowViewModel?> ShowPersonSelectDialog { get; } = new Interaction<Unit, PersonSelectWindowViewModel>();
        public ReactiveCommand<Unit, IEnumerable<PersonViewModel>?> SelectPersonCommand { get; }
        public ReactiveCommand<Unit, Unit> DeletePersonCommand { get; }
        public ReactiveCommand<Unit, IEnumerable<FranchiseSelectListItemViewModel>?> SelectFranchiseCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteSelectedFranchiseCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        #endregion

        #region Public constructor
        public MainWindowViewModel(IWallpaperRepository wallpaperRepository) {
            //Subscribe to the main search box
            //NOTE: For some reason this fires when the window first opens, and then sends us a null value. It's very annoying
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.DoSearch);

            this.WhenAnyValue(o => o.SelectedImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.SelectedImageChanged);

            //Set up our button handlers
            this.NextImagePage = ReactiveCommand.CreateFromTask(NextPageAsync);
            this.PreviousImagePage = ReactiveCommand.CreateFromTask(PreviousPageAsync);
            this.Refresh = ReactiveCommand.CreateFromTask(RefreshAsync);
            this.SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
            this.DeleteSelectedFranchiseCommand = ReactiveCommand.CreateFromTask(async () => {
                try {
                    if (this.SelectedImage is null || !(this.SelectedImage.Franchises?.Any() ?? false)) {
                        return;
                    }

                    //Find every selected franchise
                    var selectedFranchises = this.SelectedImage.Franchises.Where(o => o.Selected).ToList();
                    //Now remove them from the list, this will get saved properly when the user hits save
                    this.SelectedImage.Franchises.RemoveMany(selectedFranchises);
                } catch (Exception ex) {
                    await Common.ShowExceptionMessageBoxAsync("There was a problem removing a franchise", ex);
                }
            });

            this.SelectFranchiseCommand = ReactiveCommand.CreateFromTask(async () => {
                //var store = new MainWindowViewModel();

                var result = await ShowFranchiseSelectDialog?.Handle(Unit.Default);
                var franchises = result?.DbFranchises.Where(o => o.Selected);
                if (franchises != null) {
                    this.SelectedImage?.Franchises?.AddRange(franchises);
                    return franchises;
                }

                return null;
            });

            this.SelectPersonCommand = ReactiveCommand.CreateFromTask(async () => {
                //Can't do anything if nothing is selected
                if (this.SelectedImage is null) {
                    return null;
                }

                var result = await ShowPersonSelectDialog?.Handle(Unit.Default);
                var franchises = (IEnumerable<PersonViewModel>) result?.SelectedPeople;
                if (franchises != null) {
                    this.SelectedImage?.People?.AddRange(franchises);
                    return franchises;
                }

                return null;
            });

            this.DeletePersonCommand = ReactiveCommand.Create(() => {
                //Can't do anything if nothing is selected
                if (this.SelectedImage is null) {
                    return;
                }

                //Just do a simple remove from the people list. the save will fix everything up when it hits the storage
                this.SelectedImage.People.RemoveMany(this.SelectedImage.SelectedPeople);
            });
            this._wallpaperRepository = wallpaperRepository;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Attempts to save the current page of data to the current <see cref="_wallpaperRepository"/>
        /// </summary>
        private async Task SaveAsync() {
            try {
                //Waiting for this semaphore will wait for any page changes to complete, which should prevent any partial page saves
                await this._slim.WaitAsync();
                foreach (var wallpaper in CurrentPageData) {
                    try {
                        await this._wallpaperRepository.SaveWallpaperInfoAsync(wallpaper.ConvertToWallpaper());
                    } catch (Exception ex) {
                        await Common.ShowExceptionMessageBoxAsync("There was a problem saving the wallpaper", ex);
                        break;
                    }
                }
            } finally {
                //TODO: Safer handling of the release, this is potentially dangerous as there may be other methods waiting on this
                this._slim.Release();
                await this.RefreshAsync();
            }
        }

        /// <summary>
        /// Refreshes the current page
        /// </summary>
        private async Task RefreshAsync() {
            if (IsLoadingImages) {
                this._cancellationTokenSource?.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            await this.HandleSearchTerm(this.SearchText);
            await this.ChangePageAsync(null, token);
        }


        public async void SelectedImageChanged(ImageItemViewModel? model) {
            if (model is null) {
                return;
            }

            //TODO: CANCEL THIS WHEN WE NEED TO
            var token = new CancellationToken();
            await model.LoadBigImageAsync(token);
        }

        /// <summary>
        /// Advances the current image list to the next page
        /// </summary>
        private async Task NextPageAsync() {
            if (IsLoadingImages) {
                this._cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            await this.ChangePageAsync(true, token);
        }

        /// <summary>
        /// Advances the current image list to the previous page
        /// </summary>
        private async Task PreviousPageAsync() {
            if (IsLoadingImages) {
                this._cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            await this.ChangePageAsync(false, token);
        }

        /// <summary>
        /// Handles the user's search input and cancels the loading of any pending operations
        /// </summary>
        /// <param name="term"></param>
        private async void DoSearch(string term) {
            try {
                if (IsLoadingImages) {
                    this._cancellationTokenSource?.Cancel();
                }

                await this.HandleSearchTerm(term);

                _cancellationTokenSource = new CancellationTokenSource();
                var token = this._cancellationTokenSource.Token;
                //Always go back to the first page if this method gets called as the user will have initiated a new search
                await this.SetPageAsync(1, true, token);
            } catch (Exception ex) {
                await Common.ShowExceptionMessageBoxAsync("There was a problem running the search process", ex);
            }
        }

        /// <summary>
        /// Handles the user's search input and populates the backing lists
        /// </summary>
        /// <param name="term">The search term provided by the user to search for an image by</param>
        private async Task HandleSearchTerm(string term) {
            try {
                if (term != this.LastSearchTerm || string.IsNullOrWhiteSpace(term)) {
                    Images.Clear();

                    var wallpapers = string.IsNullOrWhiteSpace(term) ? this._wallpaperRepository.RetrieveWallpapersAsync() : this._wallpaperRepository.RetrieveWallpapersAsync(term);

                    await foreach (var wallpaper in wallpapers) {
                        Images.Add(new ImageItemViewModel(wallpaper, this._wallpaperRepository));
                    }

                    //this.CurrentPage = 1;
                    this.LastSearchTerm = term;
                }
            } catch (Exception ex) {
                await Common.ShowExceptionMessageBoxAsync("There was a problem processing the search request", ex);
            }
        }

        private async Task SetPageAsync(int pageNumber, bool allowRefresh, CancellationToken token) {
            //TODO:THIS IS NOT GUARANTEED TO RELEASE THE SEMAPHORE. EG IF AN ERROR HAPPENS
            try {
                //Need a semaphore to make sure we only do one of these at a time and to prevent all sorts of weird counting issues
                await this._slim.WaitAsync();
                //We can safely abort at this point if somebody asks
                if (token.IsCancellationRequested) {
                    this._slim.Release();
                    return;
                }

                //Set a flag so other things know we are busy, since this is in a semaphore it SHOULD be thread safe
                IsLoadingImages = true;

                //Make sure we have some page data to handle, and if not we need clear everything
                if (!Images.Any()) {
                    //Clear any data we have on the current page as we are trying to display a blank page
                    Parallel.ForEach(this.CurrentPageData,
                        (o, p) => {
                            o.Image?.Dispose();
                            o.ThumbnailImage?.Dispose();
                            o.Image = null;
                            o.ThumbnailImage = null;
                        });
                    //Finally clean up the list itself
                    this.CurrentPageData.Clear();

                    //Set this to 0 as we have no pages to display
                    this.CurrentPage = 0;

                    this._slim.Release();
                    return;
                }

                if (token.IsCancellationRequested) {
                    this._slim.Release();
                    return;
                }

                //Can't have a negative page or go over the page limits
                if (pageNumber > TotalPages || pageNumber < 1 || CurrentPage < 0) {
                    this._slim.Release();
                    return;
                }

                //Need to save the last page number the user was on, and if it's still the same page we don't need to do anything
                var lastpage = CurrentPage;

                this.CurrentPage = pageNumber;

                //Don't change the page data if the page number has not changed unless something explicitly requests a page refresh 
                if (CurrentPage == lastpage && !allowRefresh) {
                    this._slim.Release();
                    return;
                }

                if (token.IsCancellationRequested) {
                    this._slim.Release();
                    return;
                }

                //Iterate through all the image data we have and dispose of it or we will run out of memory very quickly
                Parallel.ForEach(Images,
                    (o, p) => {
                        o.Image?.Dispose();
                        o.ThumbnailImage?.Dispose();
                        o.Image = null;
                        o.ThumbnailImage = null;
                    });
                //Force a garbage collection since we are dealing with a lot of data and it seems to take a long time for it to get auto removed
                GC.Collect();

                CurrentPageData.Clear();

                if (token.IsCancellationRequested) {
                    this._slim.Release();
                    return;
                }

                //Grab the data for the current page from the backing list
                IEnumerable<ImageItemViewModel>? newPageData = this.Images.Skip((CurrentPage - 1) * PAGE_SIZE).Take(PAGE_SIZE);

                //Create a list of tasks we can await at the end, as this is the very, very slow part
                var imgtasks = new List<Task>();
                foreach (var data in newPageData) {
                    //Check if we have a cancellation and break out of the loop if we do
                    //This allows for cleaner code than a return
                    if (token.IsCancellationRequested) {
                        break;
                    }

                    //Load the images into memory and add it to the task list
                    imgtasks.Add(data.LoadThumbnailImageAsync(token));
                    //Add the view model to the current page so the user can actually see it
                    CurrentPageData.Add(data);
                }

                //Await everything (which may not be a whole lot if the task was cancelled) to prevent people from spamming the next page and causing all sorts of weird issues
                await Task.WhenAll(imgtasks);
                //Finally release the semaphore as we are no longer doing anything
                this._slim.Release();
            } finally {
                IsLoadingImages = false;
            }
        }

        /// <summary>
        /// Asynchronously changes the current image list page backwards or forwards
        /// </summary>
        /// <param name="forward">True to move forward, false to move backwards, null to keep the current page</param>
        /// <param name="token">The cancellation token to cancel loading the next page</param>
        private async Task ChangePageAsync(bool? forward, CancellationToken token) {
            if (forward.HasValue) {
                await this.SetPageAsync(forward.Value ? this.CurrentPage + 1 : this.CurrentPage - 1, false, token);
            } else {
                await this.SetPageAsync(this.CurrentPage, true, token);
            }
        }
        #endregion
    }
}
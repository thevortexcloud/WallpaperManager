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
        private ImageItemViewModel _selectedImage;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentPage = 0;
        #endregion

        #region Private readonly variables
        private readonly IWallpaperRepository _wallpaperRepository;
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
        private const int PAGE_SIZE = 150;
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
        public ImageItemViewModel SelectedImage {
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
        #endregion

        public MainWindowViewModel() {
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
            NextImagePage = ReactiveCommand.Create(NextPageAsync);
            PreviousImagePage = ReactiveCommand.Create(PreviousPageAsync);
            Refresh = ReactiveCommand.Create(RefreshAsync);

            this._wallpaperRepository = new DiskRepository();
        }

        /// <summary>
        /// Refreshes the current page
        /// </summary>
        private async void RefreshAsync() {
            if (IsLoadingImages) {
                this._cancellationTokenSource?.Cancel();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            await this.HandleSearchTerm(this.SearchText);
            await this.ChangePage(null, token);
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
        /// Called when the window opens
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public async void LoadInitialData(Object obj, EventArgs e) {
            this.Images.Clear();
        }

        /// <summary>
        /// Advances the current image list to the next page
        /// </summary>
        private async void NextPageAsync() {
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
        private async void PreviousPageAsync() {
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
            if (IsLoadingImages) {
                this._cancellationTokenSource?.Cancel();
            }

            await this.HandleSearchTerm(term);

            _cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;
            //Always go back to the first page if this method gets called as the user will have initiated a new search
            await this.SetPageAsync(1, true, token);
        }

        /// <summary>
        /// Handles the user's search input and populates the backing lists
        /// </summary>
        /// <param name="term">The search term provided by the user to search for an image by</param>
        private async Task HandleSearchTerm(string term) {
            if (term != this.LastSearchTerm || string.IsNullOrWhiteSpace(term)) {
                Images.Clear();

                var wallpapers = string.IsNullOrWhiteSpace(term) ? this._wallpaperRepository.RetrieveWallpapersAsync() : this._wallpaperRepository.RetrieveWallpapersAsync(term);

                await foreach (var wallpaper in wallpapers) {
                    Images.Add(new ImageItemViewModel(wallpaper, this._wallpaperRepository));
                }

                //this.CurrentPage = 1;
                this.LastSearchTerm = term;
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
    }
}
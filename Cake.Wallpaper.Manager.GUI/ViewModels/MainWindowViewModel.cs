using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using DynamicData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private string _searchText;
        private ImageItemViewModel _selectedImage;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentPage = 1;
        private SemaphoreSlim _slim = new SemaphoreSlim(1, 1);

        private bool IsLoadingImages { get; set; }

        public int CurrentPage {
            get => this._currentPage;
            private set {
                this.RaiseAndSetIfChanged(ref this._currentPage, value);
                this.RaisePropertyChanged(nameof(PageInfoString));
            }
        }

        private const int PAGE_SIZE = 150;

        public int TotalPages => this.Images?.Count / PAGE_SIZE ?? 0;

        public string PageInfoString => $"{CurrentPage}/{TotalPages}";

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextImagePage { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PreviousImagePage { get; }

        private List<ImageItemViewModel> Images { get; } = new List<ImageItemViewModel>();
        public ObservableCollection<ImageItemViewModel> CurrentPageData { get; } = new ObservableCollection<ImageItemViewModel>();

        public string SearchText {
            get => this._searchText;
            set => this.RaiseAndSetIfChanged(ref this._searchText, value);
        }

        public MainWindowViewModel() {
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.DoSearch);

            NextImagePage = ReactiveCommand.Create(NextPage);
            PreviousImagePage = ReactiveCommand.Create(PreviousPage);
        }

        private async void NextPage() {
            if (!IsLoadingImages) {
                _cancellationTokenSource = new CancellationTokenSource();
                var token = this._cancellationTokenSource.Token;
                await this.ChangePage(true, token);
            } else {
                this._cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var token = this._cancellationTokenSource.Token;
                await this.ChangePage(true, token);
            }
        }

        private async void PreviousPage() {
            if (!IsLoadingImages) {
                _cancellationTokenSource = new CancellationTokenSource();
                var token = this._cancellationTokenSource.Token;
                await this.ChangePage(false, token);
            } else {
                this._cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                var token = this._cancellationTokenSource.Token;
                await this.ChangePage(false, token);
            }
        }

        public async void DoSearch(string term) {
            foreach (var data in CurrentPageData) {
                data.Image?.Dispose();
                data.ThumbnailImage?.Dispose();

                data.Image = null;
                data.ThumbnailImage = null;
            }

            this.Images.Clear();
            this.CurrentPage = 0;
            if (string.IsNullOrWhiteSpace(term)) {
                await foreach (var wallpaper in new DiskRepository().RetrieveWallpapersAsync()) {
                    this.Images.Add(new ImageItemViewModel(wallpaper));
                }
            } else {
                await foreach (var wallpaper in new DiskRepository().RetrieveWallpapersAsync(term)) {
                    this.Images.Add(new ImageItemViewModel(wallpaper));
                }
            }

            this.NextPage();

            //await this.LoadImages();
        }


        private async Task ChangePage(bool forward, CancellationToken token) {
            try {
                //Need a sempapphore to make sure we only do one of these at a time and to prevent all sorts of weird counting issues
                await this._slim.WaitAsync();
                IsLoadingImages = true;

                foreach (var data in CurrentPageData) {
                    data.Image?.Dispose();
                    data.ThumbnailImage?.Dispose();

                    data.Image = null;
                    data.ThumbnailImage = null;
                }

                CurrentPageData.Clear();

                var newpagedata = this.Images.Skip((CurrentPage - 1) * PAGE_SIZE).Take(PAGE_SIZE);

                var imgtasks = new List<Task>();
                foreach (var data in newpagedata) {
                    if (token.IsCancellationRequested) {
                        if (forward) {
                            CurrentPage++;
                        } else {
                            CurrentPage--;
                        }

                        await Task.WhenAll(imgtasks);
                        this._slim.Release();
                        return;
                    }

                    imgtasks.Add(data.LoadImageAsync(token));
                    CurrentPageData.Add(data);
                }

                //CurrentPageData.AddRange(newpagedata);
                if (forward) {
                    CurrentPage++;
                } else {
                    CurrentPage--;
                }

                await Task.WhenAll(imgtasks);
                this._slim.Release();
            } finally {
                IsLoadingImages = false;
            }
        }

        public ImageItemViewModel SelectedImage {
            get => this._selectedImage;
            set => this.RaiseAndSetIfChanged(ref this._selectedImage, value);
        }
    }
}
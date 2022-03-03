using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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

        private int CurrentPage { get; set; } = 1;
        private const int PAGE_SIZE = 150;

        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> NextImagePage { get; }
        public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> PreviousImagePage { get; }

        public ObservableCollection<ImageItemViewModel> Images { get; } = new ObservableCollection<ImageItemViewModel>();
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
                imgtasks.Add(data.LoadImageAsync());
                CurrentPageData.Add(data);
            }

            //CurrentPageData.AddRange(newpagedata);
            CurrentPage++;
            await Task.WhenAll(imgtasks);
        }

        private async void PreviousPage() {
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
                imgtasks.Add(data.LoadImageAsync());
                CurrentPageData.Add(data);
            }

//            CurrentPageData.AddRange(newpagedata);
            await Task.WhenAll(imgtasks);
            //CurrentPageData.AddRange(newpagedata);
            CurrentPage--;
        }

        public async void DoSearch(string term) {
            await foreach (var wallpaper in new DiskRepository().RetrieveWallpapersAsync()) {
                this.Images.Add(new ImageItemViewModel(wallpaper));
            }

            //await this.LoadImages();
        }

        private async Task LoadImages() {
            foreach (var image in Images) {
                await image.LoadImageAsync();
            }
        }

        public ImageItemViewModel SelectedImage {
            get => this._selectedImage;
            set => this.RaiseAndSetIfChanged(ref this._selectedImage, value);
        }
    }
}
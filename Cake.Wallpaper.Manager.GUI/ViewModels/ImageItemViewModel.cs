using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.GUI.ViewModels;

public class ImageItemViewModel : ViewModelBase {
    private readonly Core.Models.Wallpaper _wallpaper;
    private Bitmap? _thumbnailImage;
    private Bitmap? _image;
    private Thickness _imageBorderThickness;

    public Bitmap? ThumbnailImage {
        get => this._thumbnailImage;
        set {
            if (value is not null && (value.Size.AspectRatio <= 1.6 || value.Size.AspectRatio >= 1.8)) {
                this.ImageBorderThickness = new Thickness(2);
            }

            this.RaiseAndSetIfChanged(ref this._thumbnailImage, value);
        }
    }

    public Bitmap? Image {
        get { return this._image; }
        set => this.RaiseAndSetIfChanged(ref this._image, value);
    }

    public ObservableCollection<FranchiseSelectListItemViewModel>? Franchises => new ObservableCollection<FranchiseSelectListItemViewModel>(this._wallpaper?.Franchises?.Select(o => new FranchiseSelectListItemViewModel(o)));


    public string? FileName => this._wallpaper.FileName;

    public string? Author {
        get {
            return this._wallpaper.Author;
        }
        set {
            if (value != this._wallpaper.Author) {
                this._wallpaper.Author = value;
                this.RaisePropertyChanged(nameof(this._wallpaper.Author));
            }
        }
    }

    public string? Source {
        get {
            return this._wallpaper.Source;
        }
        set {
            if (value != this._wallpaper.Source) {
                this._wallpaper.Source = value;
                this.RaisePropertyChanged(nameof(this._wallpaper.Source));
            }
        }
    }

    
    public string? Name {
        get {
            if (!string.IsNullOrWhiteSpace(this._wallpaper.Name)) {
                return this._wallpaper.Name;
            } else {
                return this._wallpaper.FileName;
            }
        }
        set {
            if (value != this._wallpaper.Name) {
                this._wallpaper.Name = value;
                this.RaisePropertyChanged(nameof(this._wallpaper.Name));
            }
        }
    }

    public Franchise PrimaryFranchise => this?._wallpaper?.Franchises?.FirstOrDefault();


    public Thickness ImageBorderThickness {
        get => this._imageBorderThickness;
        set => this.RaiseAndSetIfChanged(ref this._imageBorderThickness, value);
    }

    public ImageItemViewModel(Core.Models.Wallpaper wallpaper, IWallpaperRepository repository) {
        this._wallpaper = wallpaper;
        foreach (var person in this._wallpaper.People) {
            this.People.Add(new PersonViewModel(person, repository));
        }
    }

    public ObservableCollection<PersonViewModel> People { get; } = new ObservableCollection<PersonViewModel>();

    private async Task<Bitmap?> LoadImageAsync(Stream? img, bool landscape, int size, CancellationToken token) {
        if (token.IsCancellationRequested) {
            return null;
        }

        if (img is null) {
            return null;
        }

        await using (img) {
            //SKCodec codec = SKCodec.Create(img);
            //width = codec.Info.Width;
            //height = codec.Info.Height;
            return await Task.Run<Bitmap?>(() => {
                if (token.IsCancellationRequested) {
                    return null;
                }

                if (landscape) {
                    return Bitmap.DecodeToWidth(img, size, BitmapInterpolationMode.MediumQuality);
                } else {
                    return Bitmap.DecodeToHeight(img, size);
                }

                //          img.Seek(0, SeekOrigin.Begin);
                //this.Image = this.ThumbnailImage.Size.AspectRatio <= 1 ? Bitmap.DecodeToHeight(img, 400) : Bitmap.DecodeToWidth(img, 840);
            });
        }
    }

    public async Task LoadBigImageAsync(CancellationToken token) {
        if (token.IsCancellationRequested) {
            return;
        }

        var img = await this._wallpaper.LoadImageAsync();
        if (img is null) {
            return;
        }

        await using (img) {
            if (token.IsCancellationRequested) {
                return;
            }

            if (this.ThumbnailImage?.Size.AspectRatio >= 1) {
                this.Image = await this.LoadImageAsync(img, true, 840, token);
            } else {
                this.Image = await this.LoadImageAsync(img, false, 600, token);
            }

            /*//SKCodec codec = SKCodec.Create(img);
            //width = codec.Info.Width;
            //height = codec.Info.Height;
            await Task.Run(() => {
                if (token.IsCancellationRequested) {
                    return;
                }

                this.ThumbnailImage = Bitmap.DecodeToWidth(img, 400, BitmapInterpolationMode.MediumQuality);
                if (token.IsCancellationRequested) {
                    return;
                }

                img.Seek(0, SeekOrigin.Begin);
                this.Image = this.ThumbnailImage.Size.AspectRatio <= 1 ? Bitmap.DecodeToHeight(img, 400) : Bitmap.DecodeToWidth(img, 840);
            });*/
        }
    }

    public async Task LoadThumbnailImageAsync(CancellationToken token) {
        if (token.IsCancellationRequested) {
            return;
        }

        var img = await this._wallpaper.LoadImageAsync();
        if (img is null) {
            return;
        }

        await using (img) {
            if (token.IsCancellationRequested) {
                return;
            }

            this.ThumbnailImage = await this.LoadImageAsync(img, true, 400, token);
            /*//SKCodec codec = SKCodec.Create(img);
            //width = codec.Info.Width;
            //height = codec.Info.Height;
            await Task.Run(() => {
                if (token.IsCancellationRequested) {
                    return;
                }

                this.ThumbnailImage = Bitmap.DecodeToWidth(img, 400, BitmapInterpolationMode.MediumQuality);
                if (token.IsCancellationRequested) {
                    return;
                }

                img.Seek(0, SeekOrigin.Begin);
                this.Image = this.ThumbnailImage.Size.AspectRatio <= 1 ? Bitmap.DecodeToHeight(img, 400) : Bitmap.DecodeToWidth(img, 840);
            });*/
        }
    }
}
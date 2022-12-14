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

public sealed class ImageItemViewModel : ViewModelBase, IDisposable {
    #region Private readonly variables
    /// <summary>
    /// The underlying wallpaper instance that this model is backed by. Directly changing values on here will not allow for proper binding
    /// </summary>
    private readonly Core.Models.Wallpaper _wallpaper;

    private readonly IWallpaperRepository _repository;
    #endregion

    #region Private variables
    private Bitmap? _thumbnailImage;
    private Bitmap? _image;
    private Thickness _imageBorderThickness;
    #endregion

    #region Public properties
    /// <summary>
    /// Gets the current thumbnail image
    /// </summary>
    public Bitmap? ThumbnailImage {
        get => this._thumbnailImage;
        private set {
            if (value is not null && (value.Size.AspectRatio <= 1.6 || value.Size.AspectRatio >= 1.8)) {
                this.ImageBorderThickness = new Thickness(2);
            }

            this.RaiseAndSetIfChanged(ref this._thumbnailImage, value);
        }
    }

    /// <summary>
    /// Gets the current large version of the image
    /// </summary>
    public Bitmap? Image {
        get { return this._image; }
        private set => this.RaiseAndSetIfChanged(ref this._image, value);
    }

    /// <summary>
    /// Returns the current ID of the wallpaper
    /// </summary>
    public int ID => this._wallpaper.ID;

    /// <summary>
    /// Returns the franchises that this image belongs to
    /// </summary>
    public ObservableCollection<FranchiseSelectListItemViewModel> Franchises { get; } = new ObservableCollection<FranchiseSelectListItemViewModel>();

    /// <summary>
    /// Returns the name of the file that this instance represents
    /// </summary>
    public string? FileName => this._wallpaper.FileName;

    /// <summary>
    /// Gets/sets the name of the author who created the image
    /// </summary>
    public string? Author {
        get { return this._wallpaper.Author; }
        set {
            if (value != this._wallpaper.Author) {
                this._wallpaper.Author = value;
                this.RaisePropertyChanged(nameof(this._wallpaper.Author));
            }
        }
    }

    /// <summary>
    /// Gets/sets the source of the wallpaper (EG Imgur, DeviantArt, Art Station, etc)
    /// </summary>
    public string? Source {
        get { return this._wallpaper.Source; }
        set {
            if (value != this._wallpaper.Source) {
                this._wallpaper.Source = value;
                this.RaisePropertyChanged(nameof(this._wallpaper.Source));
            }
        }
    }

    /// <summary>
    /// Gets/sets the name of the wallpaper
    /// </summary>
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

    /// <summary>
    /// Returns the primary franchise of the wallpaper
    /// </summary>
    public Franchise PrimaryFranchise => this?._wallpaper?.Franchises?.FirstOrDefault();

    /// <summary>
    /// Returns the border thickness of the image
    /// </summary>
    /// <remarks>This provides a visual indication if an image has an incorrect aspect ratio</remarks>
    public Thickness ImageBorderThickness {
        get => this._imageBorderThickness;
        private set => this.RaiseAndSetIfChanged(ref this._imageBorderThickness, value);
    }

    /// <summary>
    /// Returns the list of people to display
    /// </summary>
    public ObservableCollection<PersonViewModel> People { get; } = new ObservableCollection<PersonViewModel>();

    /// <summary>
    /// Returns the list of people the user has selected
    /// </summary>
    public ObservableCollection<PersonViewModel> SelectedPeople { get; } = new ObservableCollection<PersonViewModel>();
    #endregion

    #region Public constructor
    public ImageItemViewModel(Core.Models.Wallpaper wallpaper, IWallpaperRepository repository) {
        this._wallpaper = wallpaper ?? throw new ArgumentNullException(nameof(wallpaper));
        this._repository = repository;

        var wallpaperPeople = this._wallpaper.People;
        if (wallpaperPeople != null) {
            foreach (var person in wallpaperPeople) {
                this.People.Add(new PersonViewModel(person, true, repository));
            }
        }

        foreach (var franchise in this._wallpaper.Franchises.Select(o => new FranchiseSelectListItemViewModel(o))) {
            Franchises.Add(franchise);
        }
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Attempts to load the given binary stream into a bitmap object
    /// </summary>
    /// <param name="img">The stream to load</param>
    /// <param name="landscape">Value indicating if the image is landscape or portrait</param>
    /// <param name="size">The width if <paramref name="landscape"/> is true, or height if it is false, that the image should be resized to</param>
    /// <param name="token">A cancellation used to stop loading the image</param>
    /// <returns>A bitmap which can be used to display the image in the UI</returns>
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
            //This is the slow bit, run it in the background
            return await Task.Run<Bitmap?>(() => {
                if (token.IsCancellationRequested) {
                    return null;
                }

                if (landscape) {
                    return Bitmap.DecodeToWidth(img, size, BitmapInterpolationMode.MediumQuality);
                } else {
                    return Bitmap.DecodeToHeight(img, size, BitmapInterpolationMode.MediumQuality);
                }

                //          img.Seek(0, SeekOrigin.Begin);
                //this.Image = this.ThumbnailImage.Size.AspectRatio <= 1 ? Bitmap.DecodeToHeight(img, 400) : Bitmap.DecodeToWidth(img, 840);
            });
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Attempts to load the image as a large resolution
    /// </summary>
    /// <param name="token"></param>
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

    /// <summary>
    /// Attempts to load the image as a small resolution
    /// </summary>
    /// <param name="token"></param>
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

    /// <summary>
    /// Returns a copy of the underlying <see cref="Wallpaper"/>  instance 
    /// </summary>
    /// <returns></returns>
    public Core.Models.Wallpaper ConvertToWallpaper() {
        //Do a clone of the backing object
        return this._wallpaper with {
            //HACK: FOR NOW JUST COPY THE FRANCHISE AND PEOPLE LISTS
            Franchises = this.Franchises.Select(o => o.Franchise).ToList(),
            People = this.People.Select(o => o.Person).ToList()
        };
    }
    #endregion

    #region IDisposable implementation
    public void Dispose() {
        this._thumbnailImage?.Dispose();
        this._image?.Dispose();

        //Make sure to null these if we can have an unusable reference left that could cause all sorts of problems to a caller
        this.ThumbnailImage = null;
        this.Image = null;
    }
    #endregion
}
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using ReactiveUI;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
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

    public ObservableCollection<CheckboxListItemViewModel>? Franchises => new ObservableCollection<CheckboxListItemViewModel>(this._wallpaper?.Franchises?.Select(o => new CheckboxListItemViewModel() {
        Label = o.Name,
        Value = o,
    }));

    public string? FileName => this._wallpaper.FileName;
    public string? Name => this._wallpaper.Name;

    public Franchise PrimaryFranchise => this?._wallpaper?.Franchises.FirstOrDefault();


    public Thickness ImageBorderThickness {
        get => this._imageBorderThickness;
        set => this.RaiseAndSetIfChanged(ref this._imageBorderThickness, value);
    }

    public ImageItemViewModel(Core.Models.Wallpaper wallpaper) {
        this._wallpaper = wallpaper;
    }

    public ObservableCollection<PersonViewModel> People { get; set; } = new ObservableCollection<PersonViewModel>() {
        new PersonViewModel() {
            Franchise = "Fire Emblem",
            Name = "Lucina",
        }
    };

    public PersonViewModel SelectedPerson { get; set; }

    public async Task LoadImageAsync(CancellationToken token) {
        if (token.IsCancellationRequested) {
            return;
        }

        var img = await this._wallpaper.LoadImageAsync();
        if (img is null) {
            return;
        }

        await using (img) {
            //SKCodec codec = SKCodec.Create(img);
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
            });
        }
    }
}
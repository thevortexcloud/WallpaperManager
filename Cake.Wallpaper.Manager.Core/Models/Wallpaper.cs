using System.Diagnostics.CodeAnalysis;

namespace Cake.Wallpaper.Manager.Core.Models;

public sealed record Wallpaper {
    private string _fileName;

    /// <summary>
    /// The unique ID for this wallpaper
    /// </summary>
    public int ID { get; init; }

    /// <summary>
    /// The name of the image
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The path where the actual image data can be located
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// The date the image was added to the system
    /// </summary>
    public DateTime DateAdded { get; init; }

    /// <summary>
    /// The original author of the wallpaper
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// The source of the wallpaper (DeviantArt, imgur, etc)
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Gets/sets the actual file name for the file
    /// </summary>
    /// <remarks>
    /// By default this will be the name of the file in <see cref="FilePath"/>
    /// </remarks>
    public string FileName {
        get {
            if (string.IsNullOrWhiteSpace(this._fileName) && !string.IsNullOrWhiteSpace(FilePath)) {
                return Path.GetFileName(FilePath);
            } else {
                return this._fileName;
            }
        }
        set { _fileName = value; }
    }

    /// <summary>
    /// A list of franchises this wallpaper belongs to
    /// </summary>
    public List<Franchise>? Franchises { get; init; } = new List<Franchise>();

    /// <summary>
    /// A list of people the wallpaper depicts
    /// </summary>
    public List<Person>? People { get; init; } = new List<Person>();

    /// <summary>
    /// Loads the actual binary data the <see cref="FilePath"/> refers to and returns a read only stream of it
    /// </summary>
    /// <returns></returns>
    public async Task<Stream?> LoadImageAsync() {
        if (string.IsNullOrWhiteSpace(this.FilePath)) {
            return null;
        }

        var fileinfo = new FileInfo(this.FilePath);
        if (!fileinfo.Exists) {
            return null;
        }

        return fileinfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public override string ToString() {
        return this.FileName ?? this.FilePath ?? base.ToString();
    }
}
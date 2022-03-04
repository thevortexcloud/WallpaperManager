using System.Diagnostics.CodeAnalysis;

namespace Cake.Wallpaper.Manager.Core.Models;

public class Wallpaper {
    public string Name { get; init; }
    public string? FilePath { get; init; }

    public string? FileName {
        get {
            if (!string.IsNullOrWhiteSpace(FilePath)) {
                return Path.GetFileName(FilePath);
            } else {
                return null;
            }
        }
    }

    public List<Franchise>? Franchises { get; init; }
    public List<Person>? People { get; init; }

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
        return this.FileName ?? FilePath ?? base.ToString();
    }
}
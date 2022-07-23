namespace Cake.Wallpaper.Manager.GUI.Models;

public sealed record Configuration {
    /// <summary>
    /// Returns the connection string for the database
    /// </summary>
    public string ConnectionString { get; init; }

    /// <summary>
    /// Returns the location wallpapers can be found on disk
    /// </summary>
    public string WallpaperPath { get; init; }
}
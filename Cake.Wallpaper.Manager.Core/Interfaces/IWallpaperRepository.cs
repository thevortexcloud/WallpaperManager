namespace Cake.Wallpaper.Manager.Core.Interfaces;

public interface IWallpaperRepository {
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync();
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm);
}
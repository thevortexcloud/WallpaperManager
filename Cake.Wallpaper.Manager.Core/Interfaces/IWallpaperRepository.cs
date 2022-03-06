namespace Cake.Wallpaper.Manager.Core.Interfaces;

public interface IWallpaperRepository {
    /// <summary>
    /// Retrieves all wallpapers asynchronously
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync();

    /// <summary>
    /// Retrieves a filtered list of wallpapers
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm);

    public Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper);
    public Task SavePersonInfoAsync(Models.Person person);
    public Task SaveFranchiseInfoAsync(Models.Franchise franchise);
}
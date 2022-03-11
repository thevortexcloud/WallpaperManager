using Cake.Wallpaper.Manager.Core.Models;

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

    public IAsyncEnumerable<Models.Person> RetrievePeopleAsync();

    /// <summary>
    /// Deletes a person and all related records
    /// </summary>
    /// <param name="personID">The person to delete</param>
    /// <returns></returns>
    public Task DeletePersonAsync(int personID);

    public IAsyncEnumerable<Franchise> RetrieveFranchises();
    public IAsyncEnumerable<Franchise> RetrieveFranchises(string searchTerm);
    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID);
    public Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper);
    public Task SavePersonInfoAsync(Models.Person person);
    public Task SaveFranchiseInfoAsync(Models.Franchise franchise);
}
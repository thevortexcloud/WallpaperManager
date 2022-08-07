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
    /// <param name="searchTerm">The search term to find people with</param>
    /// <returns>A filtered list of people</returns>
    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm);

    /// <summary>
    /// Retrieves a list of people
    /// </summary>
    /// <returns>A list of all people</returns>
    public IAsyncEnumerable<Models.Person>? RetrievePeopleAsync();

    /// <summary>
    /// Retrieves a filtered list of people based on the given search term
    /// </summary>
    /// <returns>A filtered list of all people that match the given search term</returns>
    public IAsyncEnumerable<Models.Person>? RetrievePeopleAsync(string searchTerm);

    /// <summary>
    /// Deletes a person and all related records
    /// </summary>
    /// <param name="personID">The person to delete</param>
    /// <returns></returns>
    public Task DeletePersonAsync(int personID);

    public IAsyncEnumerable<Franchise>? RetrieveFranchises();
    public IAsyncEnumerable<Franchise>? RetrieveFranchises(string searchTerm);
    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID);
    public Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper);
    public Task SavePersonInfoAsync(Models.Person person);
    public Task SaveFranchiseInfoAsync(Models.Franchise franchise);
    public Task SaveFranchiseInfosAsync(IEnumerable<Models.Franchise> franchise);

    /// <summary>
    /// Attempts to delete the given franchise from the repository
    /// </summary>
    /// <param name="franchiseID">The franchise to delete</param>
    /// <returns></returns>
    public Task DeleteFranchiseAsync(int franchiseID);

    /// <summary>
    /// Attempts to remove any wallpapers which no longer exist or don't meet system criteria
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// What constitutes a wallpaper existing is up to each individual implementation. EG it could be the file no longer exists on disk, or the wallpaper is too old
    /// </remarks>
    public Task TrimWallpapersAsync();

    /// <summary>
    /// Attempts to remove any metadata about the wallpaper from the repository while leaving the original file intact
    /// </summary>
    /// <returns></returns>
    /// <param name="wallpaperID">The wallpaper to delete</param>
    public Task SoftDeleteWallpaperAsync(int wallpaperID);
}
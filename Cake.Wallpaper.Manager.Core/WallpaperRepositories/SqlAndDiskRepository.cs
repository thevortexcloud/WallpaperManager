using System.Linq;
using Cake.Wallpaper.Manager.Core.DataAccess;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Core.WallpaperRepositories;

public class SqlAndDiskRepository : Interfaces.IWallpaperRepository {
    #region Private readonly variables
    private readonly string _wallpaperPath;
    private readonly string _connectionString;
    #endregion

    #region Public constructor
    public SqlAndDiskRepository(string wallpaperPath, string connectionString) {
        this._wallpaperPath = wallpaperPath ?? throw new ArgumentNullException(nameof(wallpaperPath));
        this._connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    #endregion

    #region IWallpaperRepository implementation
    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            //Find all wallpapers we already know about and set up the file paths for people to use
            var wallpapers = await sqlLite.RetrieveWallpapersAsync().ToListAsync();
            foreach (var wallpaper in wallpapers) {
                yield return wallpaper with {
                    FilePath = Path.Combine(_wallpaperPath, wallpaper.FileName)
                };
            }

            //Now find every wallpaper we don't know about, making sure to remove any files we have already handled
            foreach (var file in new DirectoryInfo(_wallpaperPath).EnumerateFiles().ExceptBy(wallpapers.Select(o => o.FileName), o => o.Name)) {
                yield return new Models.Wallpaper() {
                    FilePath = file.FullName,
                    Name = file.Name,
                    DateAdded = DateTime.Now,
                };
            }
        }
    }

    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            //Find all wallpapers we already know about and set up the file paths for people to use
            var wallpapers = sqlLite.RetrieveWallpapersAsync(searchTerm);
            var list = await wallpapers.ToListAsync();
            foreach (var wallpaper in list) {
                yield return wallpaper with {
                    FilePath = Path.Combine(_wallpaperPath, wallpaper.FileName)
                };
            }

            //Now find every wallpaper we don't know about, making sure to remove any files we have already handled
            foreach (var file in new DirectoryInfo(_wallpaperPath).EnumerateFiles($"*{searchTerm}*",
                         new EnumerationOptions() {
                             MatchCasing = MatchCasing.CaseInsensitive,
                         }).ExceptBy(list.Select(o => o.FileName), o => o.Name)) {
                yield return new Models.Wallpaper() {
                    FilePath = file.FullName,
                    Name = file.Name,
                    DateAdded = DateTime.Now,
                };
            }
        }
    }

    public IAsyncEnumerable<Person> RetrievePeopleAsync() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            //TODO:Check if this will cause issues? It may be possible for the database connection to close before this actually finishes.
            return sqlLite.RetrievePeopleAsync();
        }
    }

    public IAsyncEnumerable<Person> RetrievePeopleAsync(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            //TODO:Check if this will cause issues? It may be possible for the database connection to close before this actually finishes.
            return sqlLite.RetrievePeopleAsync(searchTerm);
        }
    }

    public async Task DeletePersonAsync(int personID) {
        using (SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.DeletePersonAsync(personID);
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchisesAsync();
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchisesAsync(searchTerm);
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchisesForPersonAsync(personID);
        }
    }

    public async Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.SaveWallpaperAsync(wallpaper);
        }
    }

    public async Task SavePersonInfoAsync(Person person) {
        Console.WriteLine($"Saving {person}");
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.InsertPersonAsync(person);
        }
    }

    public async Task SaveFranchiseInfoAsync(Franchise franchise) {
        await using (SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.InsertFranchiseAsync(franchise);
        }
    }

    public async Task SaveFranchiseInfosAsync(IEnumerable<Franchise> franchises) {
        await using (SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.InsertFranchiseListAsync(franchises);
        }
    }

    public async Task DeleteFranchiseAsync(int franchiseID) {
        await using (SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.DeleteFranchiseAsync(franchiseID);
        }
    }

    public async Task TrimWallpapersAsync() {
        //Check we have anything to validate against
        var dirinfo = new DirectoryInfo(this._wallpaperPath);
        if (!dirinfo.Exists) {
            return;
        }

        await using (SqlLite sqlLite = new SqlLite(this._connectionString)) {
            var wallpapers = sqlLite.RetrieveWallpapersAsync();
            await foreach (var wallpaper in wallpapers) {
                if (wallpaper?.FileName is null) {
                    continue;
                }

                if (!File.Exists(Path.Combine(this._wallpaperPath, wallpaper.FileName))) {
                    await this.SoftDeleteWallpaperAsync(wallpaper.ID, sqlLite);
                }
            }
        }
    }

    public async Task SoftDeleteWallpaperAsync(int wallpaperID) {
        await using (SqlLite sqlLite = new SqlLite(this._connectionString)) {
            await this.SoftDeleteWallpaperAsync(wallpaperID, sqlLite);
        }
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Attempts to remove any metadata about the wallpaper from the repository while leaving the original file intact
    /// </summary>
    /// <returns></returns>
    /// <param name="wallpaperID">The wallpaper to delete</param>
    /// <param name="sqlLite">The database instance to remove the data from</param>
    private async Task SoftDeleteWallpaperAsync(int wallpaperID, SqlLite sqlLite) {
        Console.WriteLine($"Deleting {wallpaperID} from database");
        await sqlLite.DeleteWallpaperAsync(wallpaperID);
    }
    #endregion
}
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
            foreach (var file in new DirectoryInfo(_wallpaperPath).EnumerateFiles().Where(o => !wallpapers.Select(p => p.FileName).Contains(o.Name))) {
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
                         }).ExceptBy(wallpapers.Select(o => o.FileName).ToEnumerable(), o => o.Name)) {
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
            return sqlLite.RetrievePeople();
        }
    }

    public IAsyncEnumerable<Person> RetrievePeopleAsync(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            //TODO:Check if this will cause issues? It may be possible for the database connection to close before this actually finishes.
            return sqlLite.RetrievePeople(searchTerm);
        }
    }

    public async Task DeletePersonAsync(int personID) {
        using (SqlLite sqlLite = new SqlLite(_connectionString)) {
            await sqlLite.DeletePersonAsync(personID);
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchises();
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchises(searchTerm);
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(_connectionString)) {
            return sqlLite.RetrieveFranchisesForPerson(personID);
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
    #endregion
}
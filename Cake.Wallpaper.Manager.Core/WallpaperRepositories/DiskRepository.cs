using System.Linq;
using Cake.Wallpaper.Manager.Core.DataAccess;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Core.WallpaperRepositories;

public class DiskRepository : Interfaces.IWallpaperRepository {
    private const string ConnectionString = @"Data Source=/home/zac/Projects/Cake.Wallpaper.Manager/Cake.Wallpaper.Manager.Core/Cake.Wallpaper.Manager.db;";
    private const string BaseWallpaperPath = "/home/zac/Pictures/Wallpapers/";

    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            //Find all wallpapers we already know about and set up the file paths for people to use
            var wallpapers = await sqlLite.RetrieveWallpapersAsync().ToListAsync();
            foreach (var wallpaper in wallpapers) {
                yield return wallpaper with {
                    FilePath = Path.Combine(BaseWallpaperPath, wallpaper.FileName)
                };
            }

            //Now find every wallpaper we don't know about, making sure to remove any files we have already handled
            foreach (var file in new DirectoryInfo(BaseWallpaperPath).EnumerateFiles().Where(o => !wallpapers.Select(p => p.FileName).Contains(o.Name))) {
                yield return new Models.Wallpaper() {
                    FilePath = file.FullName,
                    Name = file.Name,
                    DateAdded = DateTime.Now,
                };
            }
        }
    }

    public IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm) {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<Person> RetrievePeopleAsync() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            foreach (var person in await sqlLite.RetrievePeople().ToListAsync()) {
                yield return person;
            }
        }
    }

    public async Task DeletePersonAsync(int personID) {
        using (SqlLite sqlLite = new SqlLite(ConnectionString)) {
            await sqlLite.DeletePersonAsync(personID);
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises() {
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            return sqlLite.RetrieveFranchises();
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises(string searchTerm) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            return sqlLite.RetrieveFranchises();
        }
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID) {
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            return sqlLite.RetrieveFranchisesForPerson(personID);
        }
    }

    public Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper) {
        throw new NotImplementedException();
    }

    public async Task SavePersonInfoAsync(Person person) {
        Console.WriteLine($"Saving {person}");
        using (DataAccess.SqlLite sqlLite = new SqlLite(ConnectionString)) {
            await sqlLite.InsertPersonAsync(person);
        }
    }

    public Task SaveFranchiseInfoAsync(Franchise franchise) {
        throw new NotImplementedException();
    }
}
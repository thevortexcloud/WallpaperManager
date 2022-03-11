using System.Linq;
using Cake.Wallpaper.Manager.Core.DataAccess;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Core.WallpaperRepositories;

public class DiskRepository : Interfaces.IWallpaperRepository {
    private const string ConnectionString = @"Data Source=/home/zac/Projects/Cake.Wallpaper.Manager/Cake.Wallpaper.Manager.Core/Cake.Wallpaper.Manager.db;";

    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        foreach (var file in new DirectoryInfo("/home/zac/Pictures/Wallpapers/").EnumerateFiles()) {
            yield return new Models.Wallpaper() {
                FilePath = file.FullName,
                Name = file.Name,
                Franchises = new List<Franchise>() {
                    new Franchise() {
                        Name = "Fire Emblem",
                        ID = 1,
                        ChildFranchises = new HashSet<Franchise>() {
                            new Franchise() {
                                Name = "Fire Emblem Awakening",
                                ParentID = 1,
                                ID = 2
                            }
                        }
                    }
                }
            };
        }
    }

    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm) {
        foreach (var file in new DirectoryInfo("/home/zac/Pictures/Wallpapers/").EnumerateFiles(searchTerm)) {
            yield return new Models.Wallpaper() {
                FilePath = file.FullName,
                Name = file.Name,
                Franchises = new List<Franchise>() {
                    new Franchise() {
                        Name = "Fire Emblem",
                        ID = 1,
                        ChildFranchises = new HashSet<Franchise>() {
                            new Franchise() {
                                Name = "Fire Emblem Awakening",
                                ParentID = 1,
                                ID = 2
                            }
                        }
                    }
                }
            };
        }
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;

public class MemoryRepository : IWallpaperRepository {
    private List<Person> People { get; set; } = new List<Person>();
    private List<Manager.Core.Models.Wallpaper> Wallpapers { get; set; } = new List<Manager.Core.Models.Wallpaper>();
    private List<Franchise> Franchises { get; set; } = new List<Franchise>();


    public IAsyncEnumerable<Manager.Core.Models.Wallpaper> RetrieveWallpapersAsync() {
        return this.Wallpapers.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Manager.Core.Models.Wallpaper> RetrieveWallpapersAsync(string searchTerm) {
        return this.Wallpapers.Where(o => o.Name.Contains(searchTerm)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Person> RetrievePeopleAsync() {
        return this.People.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Person> RetrievePeopleAsync(string searchTerm) {
        return this.People.Where(o => o.Name == searchTerm).ToAsyncEnumerable();
    }

    public Task DeletePersonAsync(int personID) {
        this.People.Remove(this.People.First(o => o.ID == personID));
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises() {
        return this.Franchises.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchises(string searchTerm) {
        return this.Franchises.Where(o => o.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Franchise> RetrieveFranchisesForPerson(int personID) {
        return this.People.Where(o => o.ID == personID).SelectMany(o => o.Franchises).ToAsyncEnumerable();
    }

    public Task SaveWallpaperInfoAsync(Manager.Core.Models.Wallpaper wallpaper) {
        if (wallpaper == null) {
            throw new ArgumentNullException(nameof(wallpaper));
        }

        this.Wallpapers.Add(wallpaper);
        if (wallpaper?.People?.Any() ?? false) {
            this.People.AddRange(wallpaper.People);
        }

        if (wallpaper?.Franchises?.Any() ?? false) {
            this.Franchises.AddRange(wallpaper.Franchises);
        }

        return Task.CompletedTask;
    }

    public Task SavePersonInfoAsync(Person person) {
        this.People.Add(person);
        return Task.CompletedTask;
    }

    public Task SaveFranchiseInfoAsync(Franchise franchise) {
        this.Franchises.Add(franchise);
        return Task.CompletedTask;
    }

    public Task SaveFranchiseInfosAsync(IEnumerable<Franchise> franchise) {
        this.Franchises.AddRange(franchise);
        return Task.CompletedTask;
    }
}
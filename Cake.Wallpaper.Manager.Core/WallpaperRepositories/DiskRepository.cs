using System.Linq;
using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Core.WallpaperRepositories;

public class DiskRepository : Interfaces.IWallpaperRepository {
    public async IAsyncEnumerable<Models.Wallpaper> RetrieveWallpapersAsync() {
        /*var list = new List<Models.Wallpaper>();
        list.Add(new Models.Wallpaper() {
            FilePath = @"/home/zac/Pictures/Wallpapers/steinsgategroup.jpeg",
            Franchise = @"Steins Gate"
        }); 
        list.Add(new Models.Wallpaper() {
               FilePath = @"/home/zac/Pictures/Wallpapers/raven_dc_comic_fanart_2020_4k_hd_superheroes.jpg",
               Franchise = @"Teen Titans"
           });
        list.Add(new Models.Wallpaper() {
            FilePath = @"/home/zac/Pictures/Wallpapers/DzkggKsX4AAlRPi.jpeg",
            Franchise = @"Fire Emblem"
        });
        list.Add(new Models.Wallpaper() {
            FilePath = @"/home/zac/Pictures/Wallpapers/training_by_ian_navarro-d804ifo_[L3][x10.00].png",
            Franchise = @"Avatar Korra"
        });
        foreach (var item in list) {
            yield return item;
        }*/
        foreach (var file in new DirectoryInfo("/home/zac/Pictures/Wallpapers/").EnumerateFiles()) {
            yield return new Models.Wallpaper() {
                FilePath = file.FullName,
                Name = file.Name,
                Franchises = new List<Franchise>() {
                    new Franchise() {
                        Name = "Fire Emblem",
                        ID = 1,
                        ChildFranchises = new List<Franchise>() {
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
                        ChildFranchises = new List<Franchise>() {
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

    public Task SaveWallpaperInfoAsync(Models.Wallpaper wallpaper) {
        throw new NotImplementedException();
    }

    public Task SavePersonInfoAsync(Person person) {
        throw new NotImplementedException();
    }

    public Task SaveFranchiseInfoAsync(Franchise franchise) {
        throw new NotImplementedException();
    }
}
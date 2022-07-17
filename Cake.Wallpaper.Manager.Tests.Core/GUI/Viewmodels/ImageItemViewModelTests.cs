using System;
using System.Linq;
using System.Threading;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public class ImageItemViewModelTests {
    [Fact]
    public void FranchisesAdded() {
        var wallpaper = new Manager.Core.Models.Wallpaper();
        var franchise = new Franchise() {
            Name = "Blah",
            ID = 1,
            ParentID = 0,
        };
        wallpaper.Franchises.Add(franchise);

        //No actual data access should be happening for this test
        var model = new ImageItemViewModel(wallpaper, new MemoryRepository());

        //All we are testing is if something was added, as the viewmodel constructor for the franchise handles most of the conversion
        Assert.NotEmpty(model.Franchises);
        Assert.Single(model.Franchises);
        Assert.Equal(franchise.ID, model.Franchises.First().ID);
    }

    [Fact]
    public void ModelInitalisedCorrectly() {
        var wallpaper = new Manager.Core.Models.Wallpaper() {
            Name = "blah",
            Author = "Yes",
            Source = "The ocean",
            DateAdded = DateTime.Now,
            FileName = "busybodies.jpeg",
            ID = 1,
            FilePath = "/",
        };

        //No actual data access should be happening for this test
        var model = new ImageItemViewModel(wallpaper, new MemoryRepository());

        //Make sure values match our input
        Assert.Empty(model.Franchises);
        Assert.Equal(wallpaper.Name, model.Name);
        Assert.Equal(wallpaper.Author, model.Author);
        Assert.Equal(wallpaper.Source, model.Source);
        Assert.Equal(wallpaper.FileName, model.FileName);
        Assert.Equal(wallpaper.ID, model.ID);
    }

    [Fact]
    public async void LoadEmptyBigImage() {
        var wallpaper = new Manager.Core.Models.Wallpaper();
        var franchise = new Franchise() {
            Name = "Blah",
            ID = 1,
            ParentID = 0,
        };
        wallpaper.Franchises.Add(franchise);

        //No actual data access should be happening for this test
        var model = new ImageItemViewModel(wallpaper, new MemoryRepository());

        Assert.Null(model.Image);

        await model.LoadBigImageAsync(new CancellationToken(false));

        Assert.Null(model.Image);
    }

    [Fact]
    public async void LoadEmptyThumbnailImage() {
        var wallpaper = new Manager.Core.Models.Wallpaper();
        var franchise = new Franchise() {
            Name = "Blah",
            ID = 1,
            ParentID = 0,
        };
        wallpaper.Franchises.Add(franchise);

        //No actual data access should be happening for this test
        var model = new ImageItemViewModel(wallpaper, new MemoryRepository());

        Assert.Null(model.Image);

        await model.LoadThumbnailImageAsync(new CancellationToken(false));

        Assert.Null(model.Image);
    }
}
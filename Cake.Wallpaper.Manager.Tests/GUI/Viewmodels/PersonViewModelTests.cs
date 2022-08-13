using System.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Moq;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public class PersonViewModelTests {
    [Fact]
    public async void LoadsDataLoadsFranchisesIfFlagTrue() {
        int passedPersonId = 0;

        //Create our mock
        var repo = new Mock<IWallpaperRepository>();
        repo.Setup(o => o.RetrieveFranchisesForPerson(It.IsAny<int>()))
            .Returns(() => new Franchise[] {new Franchise()}.ToAsyncEnumerable())
            .Callback((int val) => { passedPersonId = val; });

        //Create our person model
        var person = new Person() {
            Name = "Blah",
            ID = 21,
        };

        //Create our view model based on the person, passing true to the constructor to load franchises 
        var model = new PersonViewModel(person, true, repo.Object);
        //Attempt to load the data
        await model.LoadDataAsync();
        //Verify an attempt was made to actually load the data, and the correct value was passed to the method
        repo.Verify(o => o.RetrieveFranchisesForPerson(It.IsAny<int>()), Times.Once);
        Assert.Equal(person.ID, passedPersonId);
        Assert.NotEmpty(model.FranchiseSelectListItemViewModels);
    }

    [Fact]
    public async void LoadsDataLoadsFranchisesIfFlagFalse() {
        //Create our mock
        var repo = new Mock<IWallpaperRepository>();
        repo.Setup(o => o.RetrieveFranchisesForPerson(It.IsAny<int>()))
            .Returns(() => new Franchise[] {new Franchise()}.ToAsyncEnumerable());

        //Create our person model
        var person = new Person() {
            Name = "Blah",
            ID = 21,
        };

        //Create our view model based on the person, passing true to the constructor to load franchises 
        var model = new PersonViewModel(person, false, repo.Object);
        //Attempt to load the data
        await model.LoadDataAsync();
        //Verify no attempt was made to actually load the data
        repo.Verify(o => o.RetrieveFranchisesForPerson(It.IsAny<int>()), Times.Never);
        Assert.Empty(model.FranchiseSelectListItemViewModels);
        Assert.Null(model.PrimaryFranchise);
    }
}
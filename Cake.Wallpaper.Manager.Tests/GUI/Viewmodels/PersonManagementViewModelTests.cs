using System.Collections.Generic;
using System.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Moq;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public sealed class PersonManagementViewModelTests {
    [Fact]
    public async void AttemptToLoadFilteredPeople() {
        var repoMock = new Mock<IWallpaperRepository>();
        var model = new PersonManagementViewModel(repoMock.Object);
        model.PersonSearchTerm = "Blah";

        await model.RefreshDataAsync();
        repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void AttemptToLoadFilteredFranchises() {
        var repoMock = new Mock<IWallpaperRepository>();
        var model = new PersonManagementViewModel(repoMock.Object);
        model.FranchiseSearchTerm = "Blah";

        await model.RefreshDataAsync();
        repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);
        repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async void AttemptToLoadNullPeopleAndNullFranchises() {
        var repoMock = new Mock<IWallpaperRepository>();
        repoMock.Setup(o => o.RetrieveFranchises()).Returns((IAsyncEnumerable<Franchise>?) null);

        repoMock.Setup(o => o.RetrievePeopleAsync()).Returns((IAsyncEnumerable<Person>?) null);
        var model = new PersonManagementViewModel(repoMock.Object);

        Assert.Empty(model.People);
        Assert.Empty(model.Franchises);
        await model.RefreshDataAsync();
        repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Once);
        repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);

        Assert.Empty(model.People);
        Assert.Empty(model.Franchises);

        Assert.Null(model.SelectedPerson);
    }

    [Fact]
    public async void AttemptToLoadNullFranchises() {
        var repoMock = new Mock<IWallpaperRepository>();
        repoMock.Setup(o => o.RetrieveFranchises()).Returns((IAsyncEnumerable<Franchise>?) null);

        repoMock.Setup(o => o.RetrievePeopleAsync()).Returns(new List<Person>() {
            new Person() {
                Name = "Jah",
                ID = 1,
            }
        }.ToAsyncEnumerable());
        var model = new PersonManagementViewModel(repoMock.Object);

        Assert.Empty(model.People);
        Assert.Empty(model.Franchises);
        await model.RefreshDataAsync();
        repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Once);
        repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);

        Assert.NotEmpty(model.People);
        Assert.Empty(model.Franchises);

        Assert.Null(model.SelectedPerson);
    }

    [Fact]
    public async void AttemptToLoadFilteredFranchisesAndPeople() {
        var repoMock = new Mock<IWallpaperRepository>();
        repoMock.Setup(o => o.RetrieveFranchises(It.IsAny<string>())).Returns(new List<Franchise>() {
            new Franchise() {
                Name = "Jah",
                ID = 1,
            }
        }.ToAsyncEnumerable());

        repoMock.Setup(o => o.RetrievePeopleAsync(It.IsAny<string>())).Returns(new List<Person>() {
            new Person() {
                Name = "Jah",
                ID = 1,
            }
        }.ToAsyncEnumerable());
        var model = new PersonManagementViewModel(repoMock.Object);
        model.FranchiseSearchTerm = "Blah";
        model.PersonSearchTerm = "No";

        Assert.Empty(model.People);
        Assert.Empty(model.Franchises);

        await model.RefreshDataAsync();
        repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);
        repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Once);

        Assert.NotEmpty(model.People);
        Assert.Single(model.People);

        Assert.NotEmpty(model.Franchises);
        Assert.Single(model.Franchises);

        Assert.Null(model.SelectedPerson);
    }
}
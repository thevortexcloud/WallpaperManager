using System.Collections.Generic;
using System.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Moq;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public class FranchiseManagementViewModelTests {
    [Fact]
    public async void AttemptToLoadFilteredData() {
        var repoMock = new Mock<IWallpaperRepository>();
        repoMock.Setup(o => o.RetrieveFranchises(It.IsAny<string>())).Returns(new List<Franchise>() {
            new Franchise()
        }.ToAsyncEnumerable());
        var model = new FranchiseManagementViewModel(repoMock.Object);

        //Ensure we are in a clean initial state
        Assert.Null(model.SearchText);
        Assert.Empty(model.Franchises);
        Assert.Null(model.SelectedFranchise);

        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);

        //Now try to do a search
        await model.LoadDataAsync("Random search data");

        //Check that the correct repo method was called
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Once);

        //Check data was added
        Assert.NotEmpty(model.Franchises);
    }

    [Fact]
    public async void AttemptToLoadData() {
        var repoMock = new Mock<IWallpaperRepository>();
        repoMock.Setup(o => o.RetrieveFranchises()).Returns(new List<Franchise>() {
            new Franchise()
        }.ToAsyncEnumerable());
        var model = new FranchiseManagementViewModel(repoMock.Object);

        //Ensure we are in a clean initial state
        Assert.Null(model.SearchText);
        Assert.Empty(model.Franchises);
        Assert.Null(model.SelectedFranchise);

        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);

        //Now try to do a search
        await model.LoadDataAsync();

        //Check that the correct repo method was called
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Once);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);

        //Check data was added
        Assert.NotEmpty(model.Franchises);
    }
}
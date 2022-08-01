using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI;
using ReactiveUI.Testing;
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

    public async void DoNotAttemptToLoadDataWithNullSearchTerm() {
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
        await model.LoadDataAsync(null);

        //Check that the correct repo method was called
        repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);
        repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);

        //Check data was added
        Assert.Empty(model.Franchises);
    }

    [Fact]
    public async void RepoFranchiseListIsNullOrEmpty() {
        var repoMock = new Mock<IWallpaperRepository>();
        //Set this method up to return null
        repoMock.Setup(o => o.RetrieveFranchises(It.IsAny<string>())).Returns((IAsyncEnumerable<Franchise>?) null!);
        var model = new FranchiseManagementViewModel(repoMock.Object);
        //Verify we are empty before we start
        Assert.Empty(model.Franchises);

        await model.LoadDataAsync();

        //Check we still have no data
        Assert.Empty(model.Franchises);

        repoMock.Setup(o => o.RetrieveFranchises(It.IsAny<string>())).Returns(new List<Franchise>() {
        }.ToAsyncEnumerable());

        //Make sure this is still empty if we get an empty list back from the repo
        await model.LoadDataAsync();

        //Check we still have no data
        Assert.Empty(model.Franchises);
    }


    [Fact]
    public async void AttemptToSetFranchiseParentToSelf() {
        await new TestScheduler().With(async (scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);
            //Verify we are empty before we start
            Assert.Empty(model.Franchises);

            await model.LoadDataAsync();

            //Make sure we have a non 0 ID or this test is not particularly useful
            var franchiseModel = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 10,
                },
                true);

            model.Franchises.Add(franchiseModel);
            model.SelectedFranchise = franchiseModel;

            model.SetParent.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            Assert.Null(franchiseModel.ParentID);
        });
    }

    [Fact]
    public async void AttemptToSetMultipleFranchiseParents() {
        await new TestScheduler().With(async (scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);
            //Verify we are empty before we start
            Assert.Empty(model.Franchises);

            await model.LoadDataAsync();

            //Make sure we have a non 0 ID or this test is not particularly useful
            var franchiseModel = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 10,
                },
                true);

            var franchiseModel2 = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 11,
                },
                true);

            model.Franchises.Add(franchiseModel);
            model.Franchises.Add(franchiseModel2);
            model.SelectedFranchise = franchiseModel;

            model.SetParent.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            Assert.Null(franchiseModel.ParentID);
        });
    }

    [Fact]
    public async void AttemptToSetFranchiseParentToNull() {
        await new TestScheduler().With(async (scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);
            //Verify we are empty before we start
            Assert.Empty(model.Franchises);

            await model.LoadDataAsync();

            //Make sure we have a non 0 ID or this test is not particularly useful
            var franchiseModel = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 10,
                },
                true);

            model.Franchises.Add(franchiseModel);
            model.SelectedFranchise = null;

            model.SetParent.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            Assert.Null(franchiseModel.ParentID);
        });
    }


    [Fact]
    public async void AttemptToSetFranchiseParent() {
        await new TestScheduler().With(async (scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);
            //Verify we are empty before we start
            Assert.Empty(model.Franchises);

            await model.LoadDataAsync();

            //Make sure we have a non 0 ID or this test is not particularly useful
            var parentModel = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 10,
                },
                true);

            var franchiseModel = new FranchiseSelectListItemViewModel(new Franchise() {
                    ID = 11,
                },
                false);

            model.Franchises.Add(parentModel);
            model.Franchises.Add(franchiseModel);
            model.SelectedFranchise = franchiseModel;

            model.SetParent.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            //scheduler.Start();

            Assert.NotNull(model.SelectedFranchise.ParentID);
            Assert.Equal(model.SelectedFranchise.ParentID, parentModel.ID);
        });
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

    [Fact]
    public void NewFranchiseGetsCreated() {
        new TestScheduler().With((scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);

            //Ensure we are in a clean initial state
            Assert.Null(model.SearchText);
            Assert.Empty(model.Franchises);
            Assert.Null(model.SelectedFranchise);

            model.NewFranchise.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            //Check data was added
            Assert.NotEmpty(model.Franchises);
        });
    }

    [Fact]
    public void DeleteSelectedFranchise() {
        new TestScheduler().WithAsync(async (scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();
            var model = new FranchiseManagementViewModel(repoMock.Object);

            //Ensure we are in a clean initial state
            Assert.Null(model.SearchText);
            Assert.Empty(model.Franchises);
            Assert.Null(model.SelectedFranchise);

            model.Franchises.Add(new FranchiseSelectListItemViewModel(new Franchise(), true));

            //This won't do anything unless we can be sure we have something selected
            Assert.True(model.Franchises.Any(o => o.Selected));

            model.DeleteFranchise.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            //Check data was removed
            Assert.Empty(model.Franchises);
            repoMock.Verify(o => o.DeleteFranchiseAsync(It.IsAny<int>()), Times.Once);

            //Add a non selected franchise so we can make sure we are only deleting selected stuff
            model.Franchises.Add(new FranchiseSelectListItemViewModel(new Franchise(), false));

            model.DeleteFranchise.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            //Check data was NOT removed since we are verifying that franchises that are not selected do not get removed
            Assert.NotEmpty(model.Franchises);
            //At this point this method should have been still only been called once
            repoMock.Verify(o => o.DeleteFranchiseAsync(It.IsAny<int>()), Times.Once);
        });
    }

    [Fact]
    public async void AttemptToSaveFranchises() {
        new TestScheduler().With((scheduler) => {
            var repoMock = new Mock<IWallpaperRepository>();

            repoMock.Setup(o => o.SaveFranchiseInfosAsync(It.IsAny<IEnumerable<Franchise>>()))
                .Returns(Task.CompletedTask);

            var model = new FranchiseManagementViewModel(repoMock.Object);

            //Ensure we are in a clean initial state
            Assert.Null(model.SearchText);
            Assert.Empty(model.Franchises);
            Assert.Null(model.SelectedFranchise);

            var franchise = new FranchiseSelectListItemViewModel(new Franchise(), false);
            model.Franchises.Add(franchise);


            model.SaveFranchises.Execute().Subscribe(scheduler.CreateObserver<Unit>());

            repoMock.Verify(o => o.SaveFranchiseInfoAsync(It.IsAny<Franchise>()), Times.Never);
            repoMock.Verify(o => o.SaveFranchiseInfosAsync(It.IsAny<IEnumerable<Franchise>>()), Times.Once);
        });
    }
}
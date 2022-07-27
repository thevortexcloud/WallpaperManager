using System.Collections.Generic;
using System.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Moq;
using ReactiveUI;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels {
    public sealed class FranchiseSelectWindowViewModelTests {
        [Fact]
        public async void AttemptToLoadFilteredData() {
            var repoMock = new Mock<IWallpaperRepository>();
            repoMock.Setup(o => o.RetrieveFranchises(It.IsAny<string>())).Returns(new List<Franchise>() {
                new Franchise()
            }.ToAsyncEnumerable());
            var viewmodel = new FranchiseSelectWindowViewModel(repoMock.Object);

            //Ensure we are in a clean initial state
            Assert.Empty(viewmodel.DbFranchises);
            Assert.Empty(viewmodel.SelectedFranchiseSelectListItemViewModels);
            Assert.Null(viewmodel.SearchText);

            viewmodel.SearchText = "Hello";
            await viewmodel.RefreshDataAsync();
            repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Once);
            repoMock.Verify(o => o.RetrieveFranchises(), Times.Never);

            Assert.NotEmpty(viewmodel.DbFranchises);
            //Nothing should be selected
            Assert.Empty(viewmodel.SelectedFranchiseSelectListItemViewModels);
        }

        [Fact]
        public async void AttemptToLoadData() {
            var repoMock = new Mock<IWallpaperRepository>();
            repoMock.Setup(o => o.RetrieveFranchises()).Returns(new List<Franchise>() {
                new Franchise()
            }.ToAsyncEnumerable());
            var viewmodel = new FranchiseSelectWindowViewModel(repoMock.Object);

            //Ensure we are in a clean initial state
            Assert.Empty(viewmodel.DbFranchises);
            Assert.Empty(viewmodel.SelectedFranchiseSelectListItemViewModels);
            Assert.Null(viewmodel.SearchText);

            await viewmodel.RefreshDataAsync();
            repoMock.Verify(o => o.RetrieveFranchises(It.IsAny<string>()), Times.Never);
            repoMock.Verify(o => o.RetrieveFranchises(), Times.Once);

            Assert.NotEmpty(viewmodel.DbFranchises);
            //Nothing should be selected
            Assert.Empty(viewmodel.SelectedFranchiseSelectListItemViewModels);
        }
    }
}
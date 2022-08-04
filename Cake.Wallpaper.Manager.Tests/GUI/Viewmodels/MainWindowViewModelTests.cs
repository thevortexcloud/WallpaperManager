using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.GUI.ProgramProviders;
using Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI.Testing;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public sealed class MainWindowViewModelTests {
    [Fact]
    public void LoadProgramProviders() {
        new TestScheduler().With((scheduler) => {
            var repo = new Mock<IWallpaperRepository>();
            var providerMock = new Mock<IProgramProvider>();

            var viewModel = new Cake.Wallpaper.Manager.GUI.ViewModels.MainWindowViewModel(repo.Object, new IProgramProvider[] {providerMock.Object});
            //Menu items are loaded on activation, as such make sure we are in a clean state first
            Assert.Empty(viewModel.ProgramProviders);
            //Now activate
            viewModel.Activator.Activate();
            //Now check the providers were actually added
            Assert.NotEmpty(viewModel.ProgramProviders);
        });
    }


    [Theory]
    [InlineData("Gabba")]
    [InlineData("1234")]
    public void AttemptToLoadNextPageWithOnlyOnePageOfData_WithSearch(string term) {
        new TestScheduler().With((scheduler) => {
            var repo = new Mock<IWallpaperRepository>();
            repo.Setup(o => o.RetrieveWallpapersAsync(It.IsAny<string>()))
                .Returns(new List<Manager.Core.Models.Wallpaper>() {
                    new Manager.Core.Models.Wallpaper()
                }.ToAsyncEnumerable());

            var viewModel = new Cake.Wallpaper.Manager.GUI.ViewModels.MainWindowViewModel(repo.Object, new IProgramProvider[] { });
            viewModel.SearchText = term;

            //Default page is 0
            Assert.Equal(0, viewModel.CurrentPage);
            //By default we will have no page data either
            Assert.Empty(viewModel.CurrentPageData);

            viewModel.Refresh.Execute().Subscribe();


            viewModel.NextImagePage.Execute().Subscribe();

            repo.Verify(o => o.RetrieveWallpapersAsync(It.IsAny<string>()), Times.Once);
            Assert.NotEmpty(viewModel.CurrentPageData);
            Assert.Equal(1, viewModel.CurrentPage);
        });
    }

    [Fact]
    public void AttemptToLoadNextPageWithOnlyOnePageOfData() {
        new TestScheduler().With((scheduler) => {
            var repo = new Mock<IWallpaperRepository>();
            repo.Setup(o => o.RetrieveWallpapersAsync())
                .Returns(new List<Manager.Core.Models.Wallpaper>() {
                    new Manager.Core.Models.Wallpaper()
                }.ToAsyncEnumerable());

            var viewModel = new Cake.Wallpaper.Manager.GUI.ViewModels.MainWindowViewModel(repo.Object, new IProgramProvider[] { });
            //Default page is 0
            Assert.Equal(0, viewModel.CurrentPage);
            //By default we will have no page data either
            Assert.Empty(viewModel.CurrentPageData);

            viewModel.Refresh.Execute().Subscribe();


            viewModel.NextImagePage.Execute().Subscribe();

            repo.Verify(o => o.RetrieveWallpapersAsync(), Times.Once);
            Assert.NotEmpty(viewModel.CurrentPageData);
            Assert.Equal(1, viewModel.CurrentPage);
        });
    }
}
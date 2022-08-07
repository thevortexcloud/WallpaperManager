using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Microsoft.Reactive.Testing;
using ReactiveUI.Testing;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels {
    public sealed class FranchiseSelectListItemViewModelTests {
        [Fact]
        public void SelectedSetOnCreation() {
            var model = new FranchiseSelectListItemViewModel(new Franchise(), true);

            Assert.True(model.Selected);
        }

        [Fact]
        public void ViewModelInitialisesCorrectlyFromFranchise() {
            new TestScheduler().With((scheduler) => {
                var franchise = new Franchise() {
                    Depth = 1,
                    Name = "Blah",
                    ID = 23,
                    ParentID = 10
                };
                var model = new FranchiseSelectListItemViewModel(franchise, true);

                Assert.Equal(franchise.Name, model.Name);
                Assert.Equal(franchise.Depth, model.Depth);
                Assert.Equal(franchise.ID, model.ID);
                Assert.Equal(franchise.ParentID, model.ParentID);
            });
        }
    }
}
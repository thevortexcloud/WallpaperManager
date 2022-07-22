using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels {
    public sealed class FranchiseSelectListItemViewModelTests {
        [Fact]
        public void SelectedSet() {
            var model = new FranchiseSelectListItemViewModel(new Franchise(), true);

            Assert.True(model.Selected);
        }
    }
}
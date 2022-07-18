using System.Linq;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels {
    public class PersonSelectWindowViewModelTests {
        [Fact]
        public async void CanLoadPeopleFromRepo() {
            //Add a single person we can compare against
            var repo = new MemoryRepository();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };
            await repo.SavePersonInfoAsync(person);

            var model = new PersonSelectWindowViewModel(repo);
            await model.LoadDataAsync();

            Assert.NotEmpty(model.People);
            Assert.Empty(model.SelectedPeople);
            Assert.Single(model.People);
            Assert.Equal(person, model.People.First().Person);
        }
    }
}
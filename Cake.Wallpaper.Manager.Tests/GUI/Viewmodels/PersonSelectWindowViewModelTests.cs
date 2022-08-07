using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Cake.Wallpaper.Manager.Tests.Core.WallpaperRepositories;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI.Testing;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels {
    public class PersonSelectWindowViewModelTests {
        [Fact]
        public async void AttemptToLoadPeopleFromRepo() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns(new List<Person>() {
                person
            }.ToAsyncEnumerable());

            var model = new PersonSelectWindowViewModel(repoMock.Object);
            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            await model.RefreshDataAsync();

            Assert.NotEmpty(model.People);
            Assert.Empty(model.SelectedPeople);
            Assert.Single(model.People);
            Assert.Equal(person, model.People.First().Person);

            repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
            repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Once);
        }

        [Fact]
        public async void AttemptToLoadNullPeopleFromRepo() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns((IAsyncEnumerable<Person>?) null);

            var model = new PersonSelectWindowViewModel(repoMock.Object);

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            await model.RefreshDataAsync();

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
            repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Once);
        }

        [Fact]
        public async void AttemptToLoadNullPeopleFromRepoWithFilter() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns((IAsyncEnumerable<Person>?) null);

            var model = new PersonSelectWindowViewModel(repoMock.Object);
            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            model.PersonSearchTerm = "Blah";

            await model.RefreshDataAsync();


            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Once);
            repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Never);
        }

        [Fact]
        public void NullSearchFilterDoesNotLoadAnything() {
            new TestScheduler().With((scheduler) => {
                //Add a single person we can compare against
                var repoMock = new Mock<IWallpaperRepository>();

                repoMock.Setup(o => o.RetrievePeopleAsync()).Returns((IAsyncEnumerable<Person>?) null);

                var model = new PersonSelectWindowViewModel(repoMock.Object);
                Assert.Empty(model.People);
                Assert.Empty(model.SelectedPeople);

                model.PersonSearchTerm = null;

                Assert.Empty(model.People);
                Assert.Empty(model.SelectedPeople);

                repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Never);
                repoMock.Verify(o => o.RetrievePeopleAsync(), Times.Never);
            });
        }

        [Fact]
        public async void AttemptToLoadPeopleFromRepoWithPersonFilter() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };

            repoMock.Setup(o => o.RetrievePeopleAsync(It.IsAny<string>())).Returns(new List<Person>() {
                person
            }.ToAsyncEnumerable());

            var model = new PersonSelectWindowViewModel(repoMock.Object);

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            model.PersonSearchTerm = "Blah";
            await model.RefreshDataAsync();

            Assert.NotEmpty(model.People);
            Assert.Empty(model.SelectedPeople);
            Assert.Single(model.People);
            Assert.Equal(person, model.People.First().Person);

            repoMock.Verify(o => o.RetrievePeopleAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SelectPersonToRemoveUpdatesSelectList() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns(new List<Person>() {
                person
            }.ToAsyncEnumerable());

            var model = new PersonSelectWindowViewModel(repoMock.Object);

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            model.People.Add(new PersonViewModel(person, repoMock.Object));

            Assert.NotEmpty(model.People);
            Assert.Empty(model.SelectedPeople);
            Assert.Single(model.People);

            model.SelectedPeople.Add(model.People.First());

            Assert.NotEmpty(model.SelectedPeople);
            Assert.Single(model.SelectedPeople);

            model.SelectedPersonToRemoveFromList = model.People.First();

            Assert.Empty(model.SelectedPeople);
        }

        [Fact]
        public void SelectPersonUpdatesSelectList() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns(new List<Person>() {
                person
            }.ToAsyncEnumerable());

            var model = new PersonSelectWindowViewModel(repoMock.Object);

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            model.People.Add(new PersonViewModel(person, repoMock.Object));

            Assert.NotEmpty(model.People);
            Assert.Empty(model.SelectedPeople);
            Assert.Single(model.People);

            model.SelectedPersonToAddToList = model.People.First();

            Assert.NotEmpty(model.SelectedPeople);
            Assert.Single(model.SelectedPeople);
            Assert.Equal(model.SelectedPersonToAddToList, model.SelectedPeople.First());
        }

        [Fact]
        public async void DoneCommandReturnsInstance() {
            //Add a single person we can compare against
            var repoMock = new Mock<IWallpaperRepository>();
            var person = new Person() {
                ID = 1,
                Name = "Blah",
            };

            repoMock.Setup(o => o.RetrievePeopleAsync()).Returns(new List<Person>() {
                person
            }.ToAsyncEnumerable());

            var model = new PersonSelectWindowViewModel(repoMock.Object);

            Assert.Empty(model.People);
            Assert.Empty(model.SelectedPeople);

            model.SelectedPeople.Add(new PersonViewModel(person, repoMock.Object));

            var result = await model.DoneCommand.Execute().FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.NotEmpty(model.SelectedPeople);
            Assert.Single(model.SelectedPeople);
            Assert.Equal(person, model.SelectedPeople.First().Person);
        }
    }
}
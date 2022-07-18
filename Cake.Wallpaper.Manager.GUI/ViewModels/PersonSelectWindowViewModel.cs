using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.Models;
using DynamicData;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using ReactiveUI;

namespace Cake.Wallpaper.Manager.GUI.ViewModels {
    public class PersonSelectWindowViewModel : ViewModelBase, IActivatableViewModel {
        #region Private readonly variables
        private readonly IWallpaperRepository _wallpaperRepository;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets/sets the list of people to display
        /// </summary>
        public ObservableCollection<PersonViewModel> People { get; set; } = new ObservableCollection<PersonViewModel>();

        /// <summary>
        /// Gets the list of people selected by the user 
        /// </summary>
        public ObservableCollection<PersonViewModel> SelectedPeople { get; private set; } = new ObservableCollection<PersonViewModel>();

        /// <summary>
        /// A command to execute when the user has completed their selection
        /// </summary>
        public ReactiveCommand<Unit, PersonSelectWindowViewModel> DoneCommand { get; }

        public ViewModelActivator Activator { get; }
        #endregion

        #region Public constructor
        public PersonSelectWindowViewModel(IWallpaperRepository wallpaperRepository) {
            this._wallpaperRepository = wallpaperRepository;
            this.Activator = new ViewModelActivator();
            this.DoneCommand = ReactiveCommand.Create(() => this);

            this.WhenActivated((disposable) => { Disposable.Create(() => { }).DisposeWith(disposable); });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Attempts to load people from the configured repository into the <see cref="People"/> list
        /// </summary>
        public async Task LoadDataAsync() {
            var people = this._wallpaperRepository.RetrievePeopleAsync();
            await foreach (var person in people) {
                this.People.Add(new PersonViewModel(person, this._wallpaperRepository));
            }
        }
        #endregion
    }
}
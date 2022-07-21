using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        #region Private variables
        private string? _personSearchTerm;
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

        /// <summary>
        /// An activator that is called when the view is initialised
        /// </summary>
        public ViewModelActivator Activator { get; }

        /// <summary>
        /// Gets/sets the current search term for the person filter
        /// </summary>
        public string? PersonSearchTerm {
            get => this._personSearchTerm;
            set => this.RaiseAndSetIfChanged(ref this._personSearchTerm, value);
        }
        #endregion

        #region Public constructor
        public PersonSelectWindowViewModel(IWallpaperRepository wallpaperRepository) {
            this._wallpaperRepository = wallpaperRepository;
            this.Activator = new ViewModelActivator();
            this.DoneCommand = ReactiveCommand.Create(() => this);

            this.WhenActivated((disposable) => { Disposable.Create(() => { }).DisposeWith(disposable); });

            this.WhenAnyValue(o => o.PersonSearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(HandlePersonSearchAsync);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Attempts to load people from the configured repository into the <see cref="People"/> list
        /// </summary>
        public async Task RefreshDataAsync() {
            this.People.Clear();
            //Fetch the people based on the current search term and then add them to the people list
            var people = this.PersonSearchTerm is null ? this._wallpaperRepository.RetrievePeopleAsync() : this._wallpaperRepository.RetrievePeopleAsync(this.PersonSearchTerm);
            if (people is null) {
                return;
            }

            await foreach (var person in people) {
                this.People.Add(new PersonViewModel(person, this._wallpaperRepository));
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Attempts to handle the given search term value and load any related data
        /// </summary>
        /// <param name="term">The search term to validate</param>
        private async void HandlePersonSearchAsync(string? term) {
            if (term is null) {
                return;
            }

            await this.RefreshDataAsync();
        }
        #endregion
    }
}
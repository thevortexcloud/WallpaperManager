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
        private PersonViewModel? _selectedPersonToRemoveFromList;
        private PersonViewModel? _selectedPersonToAddToList;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets/sets the list of people to display
        /// </summary>
        public ObservableCollection<PersonViewModel> People { get; } = new ObservableCollection<PersonViewModel>();

        /// <summary>
        /// Gets the list of people selected by the user 
        /// </summary>
        public ObservableCollection<PersonViewModel> SelectedPeople { get; } = new ObservableCollection<PersonViewModel>();

        /// <summary>
        /// A command to execute when the user has completed their selection
        /// </summary>
        public ReactiveCommand<Unit, PersonSelectWindowViewModel> DoneCommand { get; }

        /// <summary>
        /// An activator that is called when the view is initialised
        /// </summary>
        public ViewModelActivator Activator { get; }

        /// <summary>
        /// Gets/sets the person to add to the <see cref="SelectedPeople"/> list
        /// </summary>
        public PersonViewModel? SelectedPersonToRemoveFromList {
            get => this._selectedPersonToRemoveFromList;
            set => this.RaiseAndSetIfChanged(ref this._selectedPersonToRemoveFromList, value, nameof(SelectedPersonToRemoveFromList));
        }

        /// <summary>
        /// Gets/sets the person to remove from the <see cref="SelectedPeople"/> list
        /// </summary>
        public PersonViewModel? SelectedPersonToAddToList {
            get => this._selectedPersonToAddToList;
            set => this.RaiseAndSetIfChanged(ref this._selectedPersonToAddToList, value, nameof(SelectedPersonToAddToList));
        }

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

            //When someone clicks done, return this entire instance so the caller can grab what ever values they want
            this.DoneCommand = ReactiveCommand.Create(() => this);

            this.WhenActivated((disposable) => { Disposable.Create(() => { }).DisposeWith(disposable); });

            //Register our search handler
            this.WhenAnyValue(o => o.PersonSearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(HandlePersonSearchAsync);

            //Register our person selection handler
            this.WhenAnyValue(o => o.SelectedPersonToRemoveFromList)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(OnPersonSelected);

            //Register our handler for removing people from the selection list
            this.WhenAnyValue(o => o.SelectedPersonToAddToList)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(OnRemoveSelectedPerson);
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
                this.People.Add(new PersonViewModel(person, false, this._wallpaperRepository));
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Removes the given person model from the <see cref="SelectedPeople"/> list
        /// </summary>
        /// <param name="model"></param>
        private void OnPersonSelected(PersonViewModel? model) {
            if (model is not null) {
                this.SelectedPeople.Remove(model);
            }
        }

        private void OnRemoveSelectedPerson(PersonViewModel? model) {
            if (model is not null && !this.SelectedPeople.Contains(model)) {
                this.SelectedPeople.Add(model);
            }
        }

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
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Splat;

namespace Cake.Wallpaper.Manager.GUI {
    public static class WindowHelper {
        #region Public static methods
        /// <summary>
        /// Attempts to create a window instance of <typeparamref name="T"/> using a located data context instance that returns <typeparamref name="TResult"/> once it has closed
        /// </summary>
        /// <param name="owner">The owner of the window</param>
        /// <typeparam name="T">The dialogue window to display</typeparam>
        /// <typeparam name="TResult">The dialogue result that is returned by <typeparamref name="T"/></typeparam>
        /// <returns>The result of the dialogue, or null if the dialogue returns a null</returns>
        public static Task<TResult?> ShowDialogueAsync<T, TResult>(Window owner) where T : Window {
            var view = ViewLocator.CreateView<T>();
            var viewModel = CreateViewModelForDialogueView<T>();

            view.DataContext = viewModel;

            return view.ShowDialog<TResult?>(owner);
        }

        /// <summary>
        /// Attempts to create a window instance of <typeparamref name="T"/> using a located data context instance
        /// </summary>
        /// <param name="owner">The owner of the window</param>
        /// <typeparam name="T">The type of window to attempt to create</typeparam>
        /// <exception cref="ApplicationException">Returned if a view model is unable to be located for the given window type</exception>
        public static void ShowWindow<T>(Window owner) where T : Window {
            var view = ViewLocator.CreateView<T>();
            var viewModel = CreateViewModelForWindowView<T>();
            if (viewModel is null) {
                throw new ApplicationException($"Unable to find view model for {typeof(T).FullName}, has it been registered as a service?");
            }

            view.DataContext = viewModel;

            view.Show(owner);
        }
        #endregion

        #region Private static methods
        /// <summary>
        /// Attempts to create a view model that matches the naming convention of the window
        /// </summary>
        /// <typeparam name="T">The window to find the view model for</typeparam>
        /// <returns>An instance of the located viewmodel, or null if the view model is not found</returns>
        private static object? CreateViewModelForWindowView<T>() where T : Window {
            var viewmodelName = typeof(T).FullName!.Replace("Window", "ViewModel").Replace("Views", "ViewModels");
            return CreateViewModel(viewmodelName);
        }

        /// <summary>
        /// Attempts to locate the given viewmodel type and resolve it from the dependency container 
        /// </summary>
        /// <param name="viewModelName">The view model to locate</param>
        /// <returns>An instance of the given view model or null</returns>
        /// <remarks>Due to this locating an instance of the view model from the container, the view model MUST be registered for this to work</remarks>
        private static object? CreateViewModel(string viewModelName) {
            var viewmodelType = Type.GetType(viewModelName);
            if (viewmodelType is null) {
                return null;
            }

            var viewModel = Locator.Current.GetService(viewmodelType);
            return viewModel;
        }

        /// <summary>
        /// Attempts to create a view model that matches the naming convention of the dialogue window
        /// </summary>
        /// <typeparam name="T">The window to find the view model for</typeparam>
        /// <returns>An instance of the located viewmodel, or null if the view model is not found</returns>
        private static object? CreateViewModelForDialogueView<T>() where T : Window {
            var viewmodelName = typeof(T).FullName!.Replace("DialogueWindow", "WindowViewModel").Replace("Views", "ViewModels");
            return CreateViewModel(viewmodelName);
        }
        #endregion
    }
}
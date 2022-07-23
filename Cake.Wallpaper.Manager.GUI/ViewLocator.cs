using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI {
    public class ViewLocator : IDataTemplate {
        #region IDataTemplate Implementation
        public IControl Build(object data) {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null) {
                return (Control) Activator.CreateInstance(type)!;
            } else {
                return new TextBlock {Text = "Not Found: " + name};
            }
        }

        public bool Match(object data) {
            return data is ViewModelBase;
        }
        #endregion

        #region Public static methods
        /// <summary>
        /// Attempts to create an instance of the given view
        /// </summary>
        /// <typeparam name="T">The view to create</typeparam>
        /// <returns>A instance of the view</returns>
        /// <remarks>The returned instance does NOT have a datacontext set, it is up to the caller to decide how to get the window into a valid state</remarks>
        public static T CreateView<T>() where T : Window {
            return (T) Activator.CreateInstance(typeof(T))!;
        }
        #endregion
    }
}
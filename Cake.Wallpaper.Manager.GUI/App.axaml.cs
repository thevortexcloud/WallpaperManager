using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Cake.Wallpaper.Manager.GUI.Views;
using Splat;

namespace Cake.Wallpaper.Manager.GUI {
    public partial class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                Locator.CurrentMutable.Register<IWallpaperRepository>(() => { return new DiskRepository(); });
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel(Locator.Current.GetService<IWallpaperRepository>()),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
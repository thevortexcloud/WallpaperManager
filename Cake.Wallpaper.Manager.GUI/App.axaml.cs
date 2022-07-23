using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Cake.Wallpaper.Manager.Core.Interfaces;
using Cake.Wallpaper.Manager.Core.WallpaperRepositories;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Cake.Wallpaper.Manager.GUI.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace Cake.Wallpaper.Manager.GUI {
    public partial class App : Application {
        public override void Initialize() {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.UseMicrosoftDependencyResolver();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                serviceCollection.AddScoped<IWallpaperRepository, SqlAndDiskRepository>();
                serviceCollection.AddScoped<MainWindowViewModel>();

                serviceCollection.AddTransient<PersonSelectWindowViewModel>();
                serviceCollection.AddTransient<FranchiseSelectWindowViewModel>();
                serviceCollection.AddTransient<PersonManagementViewModel>();
                serviceCollection.AddTransient<FranchiseManagementViewModel>();

                var miResolver = new MicrosoftDependencyResolver(serviceCollection);
                miResolver.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
                miResolver.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));

                miResolver.InitializeSplat();
                miResolver.InitializeReactiveUI();

                Locator.SetLocator(miResolver);


                RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;


                var model = miResolver.GetService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow() {
                    DataContext = model,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
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
using Microsoft.Extensions.Configuration;
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
            //Create our DI container and tell Splat to use the Microsoft resolver
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.UseMicrosoftDependencyResolver();

            //Set up our config file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<App>() //This should go last to override any settings in the appsettings file
                .Build();

            //Create an object for us to bind to
            var config = new Models.Configuration();
//Bind the config to the object we just made
            configuration.Bind(config);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                serviceCollection.AddScoped<IWallpaperRepository, SqlAndDiskRepository>(o => new SqlAndDiskRepository(config.WallpaperPath, config.ConnectionString));
                serviceCollection.AddScoped<MainWindowViewModel>();

                //These need to be transient since we do NOT want to preserve state with them, and lifetime of the container is the life time of the application
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
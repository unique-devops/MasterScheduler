using MasterScheduler.Interface;
using MasterScheduler.Service;
using MasterScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace MasterScheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Core
            services.AddSingleton<NavigationHost>();
            services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            //services.AddTransient<JobTypeSelectionViewModel>();
            //services.AddTransient<AddTaskViewModel>();

            var provider = services.BuildServiceProvider();

            var navigationHost = provider.GetRequiredService<NavigationHost>();
            var navigationService = provider.GetRequiredService<INavigationService>();
            navigationService.NavigateTo<DashboardViewModel>(); // Set initial screen

            var mainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
    }

}

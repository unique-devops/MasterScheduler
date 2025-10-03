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
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            // Register viewmodels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();            
            services.AddTransient<TaskTypeSelectionViewModel>();            
            services.AddTransient<SQLBackupScheduleViewModel>();                                                                                

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            }
            ;
            mainWindow.Show();
        }
    }

}

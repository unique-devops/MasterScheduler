using MasterScheduler.Interface;
using MasterScheduler.Service;
using MasterScheduler.Shared.Data;
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
        public static ToastService ToastService { get; } = new ToastService();
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
            services.AddTransient<SchedulerSettingsViewModel>();                                                                                
            services.AddTransient<LogDashboardViewModel>();                                                                                
            services.AddTransient<EditionOverlayViewModel>();                                                                                
            services.AddTransient<LocalPathBackupConfigViewModel>();                                                                                

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DatabaseHelper.Initialize();
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            }
            ;
            mainWindow.Show();
        }
    }

}

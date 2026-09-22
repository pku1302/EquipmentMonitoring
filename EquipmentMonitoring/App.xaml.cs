using EquipmentMonitoring.Parsers;
using EquipmentMonitoring.Services;
using EquipmentMonitoring.ViewModels;
using EquipmentMonitoring.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace EquipmentMonitoring
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(
            IServiceCollection services)
        {
            // =========================
            // Infrastructure
            // =========================

            services.AddSingleton(new HttpClient
            {
                BaseAddress =
                    new Uri("https://localhost:7257/")
            });

            services.AddSingleton<MonitoringSignalRService>();

            services.AddSingleton<SensorApiService>();

            // =========================
            // Application Services
            // =========================

            services.AddSingleton<AlarmApiService>();

            // 로그
            services.AddSingleton<LogService>();

            // =========================
            // ViewModels
            // =========================

            services.AddSingleton<DashboardViewModel>();

            services.AddTransient<EquipmentViewModel>();

            services.AddSingleton<AlarmViewModel>();

            services.AddSingleton<LogViewModel>();

            services.AddSingleton<MainViewModel>();

            services.AddSingleton<MonitoringSignalRService>();

            services.AddSingleton<LiveChartViewModel>();

            // =========================
            // Views
            // =========================

            services.AddSingleton<MainWindow>();
        }

        protected override async void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            var signalRService =
                Services.GetRequiredService<MonitoringSignalRService>();

            try
            {
                await signalRService.ConnectAsync();
            }
            catch (Exception)
            {

            }

            var mainWindow =
                Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }
    }
}

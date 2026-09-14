using EquipmentMonitoring.Parsers;
using EquipmentMonitoring.Repositories;
using EquipmentMonitoring.Services;
using EquipmentMonitoring.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace EquipmentMonitoring
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        private const string ConnectionString =
            @"Server=localhost;
                  Database=EquipmentMonitoringDb;
                  Trusted_Connection=True;
                  TrustServerCertificate=True;";

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

            // TCP
            services.AddSingleton<TcpCommunicationService>();

            // Repository
            services.AddSingleton(
                _ => new SensorRepository(ConnectionString));

            services.AddSingleton(
                _ => new AlarmRepository(ConnectionString));


            // =========================
            // Application Services
            // =========================

            // 공용 상태
            services.AddSingleton<EquipmentStateService>();

            // 패킷 파서
            services.AddSingleton<EquipmentPacketParser>();

            // 로그
            services.AddSingleton<LogService>();

            // 장비
            services.AddSingleton<EquipmentService>();

            // =========================
            // ViewModels
            // =========================

            services.AddSingleton<DashboardViewModel>();

            services.AddSingleton<EquipmentViewModel>();

            services.AddSingleton<AlarmViewModel>();

            services.AddSingleton<LogViewModel>();

            services.AddSingleton<MainViewModel>();

            // =========================
            // Views
            // =========================

            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            Services.GetRequiredService<EquipmentService>();

            var mainWindow =
                Services.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }

    }
}

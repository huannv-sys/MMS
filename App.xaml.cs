using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikroTikMonitor.Services;
using MikroTikMonitor.ViewModels;
using MikroTikMonitor.Windows;
using log4net;
using log4net.Config;

namespace MikroTikMonitor
{
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        private readonly IServiceProvider _serviceProvider;
        public static IConfiguration Configuration { get; private set; }

        public App()
        {
            // Initialize log4net
            var logRepository = LogManager.GetRepository();
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            
            // Load configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            Configuration = builder.Build();
            
            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register configuration
            services.AddSingleton<IConfiguration>(Configuration);
            
            // Register services
            services.AddSingleton<IRouterApiService, RouterApiService>();
            services.AddSingleton<ISnmpService, SnmpService>();
            services.AddSingleton<ISshService, SshService>();
            services.AddSingleton<IWinboxService, WinboxService>();
            services.AddSingleton<ICloudService, CloudService>();
            services.AddSingleton<IWorkerService, WorkerService>();
            
            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<SiteViewModel>();
            services.AddTransient<CloudViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<InterfacesViewModel>();
            services.AddTransient<DhcpViewModel>();
            services.AddTransient<FirewallViewModel>();
            services.AddTransient<VpnViewModel>();
            services.AddTransient<QosViewModel>();
            services.AddTransient<TrafficViewModel>();
            services.AddTransient<LogsViewModel>();
            services.AddTransient<SettingsViewModel>();
            
            // Register Windows
            services.AddTransient<MikroTikMonitor.Windows.MainWindow>();
            services.AddTransient<AddDeviceWindow>();
            services.AddTransient<AddSiteWindow>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<SshTerminalWindow>();
            services.AddTransient<ChartDetailWindow>();
            services.AddTransient<CloudLoginWindow>();
            services.AddTransient<AboutWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            log.Info("Application starting up...");
            
            try
            {
                var mainWindow = _serviceProvider.GetRequiredService<MikroTikMonitor.Windows.MainWindow>();
                mainWindow.Show();
                
                // Start background worker service
                var workerService = _serviceProvider.GetRequiredService<IWorkerService>();
                workerService.Start();
                
                log.Info("Application successfully started");
            }
            catch (Exception ex)
            {
                log.Error("Error during application startup", ex);
                MessageBox.Show($"Error starting application: {ex.Message}", "Startup Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Stop background worker service
                var workerService = _serviceProvider.GetRequiredService<IWorkerService>();
                workerService.Stop();
                
                log.Info("Application shutting down...");
            }
            catch (Exception ex)
            {
                log.Error("Error during application shutdown", ex);
            }
            
            base.OnExit(e);
        }
    }
}
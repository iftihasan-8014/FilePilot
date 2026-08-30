using System.Windows;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FilePilot.ViewModels;
using FilePilot.Views;
using FilePilot.Data;
using FilePilot.Services;

namespace FilePilot;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private bool _startMinimized;

    public IServiceProvider Services => _host.Services;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Database
                services.AddDbContext<FilePilotDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                // Services
                services.AddTransient<IActivityService, ActivityService>();
                services.AddTransient<IFileOrganizerService, FileOrganizerService>();
                services.AddSingleton<IStartupService, StartupService>();
                
                // Register MonitorService both as IMonitorService and as a HostedService
                services.AddSingleton<MonitorService>();
                services.AddSingleton<IMonitorService>(provider => provider.GetRequiredService<MonitorService>());
                services.AddHostedService(provider => provider.GetRequiredService<MonitorService>());

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CategorizerViewModel>();
                services.AddTransient<DuplicateFinderViewModel>();
                services.AddTransient<PlaceholderPageViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            if (e.Args.Contains("--minimized"))
            {
                _startMinimized = true;
            }

            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FilePilotDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            
            if (_startMinimized)
            {
                // Just minimize and hide it. In a real app we'd use a NotifyIcon (System Tray).
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.ShowInTaskbar = false;
            }
            else
            {
                mainWindow.Show();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fatal error during startup: {ex.Message}", "FilePilot Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}

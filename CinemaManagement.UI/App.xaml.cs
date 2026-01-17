// CinemaManagement.UI/App.xaml/App.xaml.cs

using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Infrastructure.Data;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Core.Managers;
using CinemaManagement.Infrastructure.Repositories;
using CinemaManagement.UI.Views;

namespace CinemaManagement.UI
{
    public partial class App : Application
    {
        private IHost _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(context.Configuration);

                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

                    var connectionString = context.Configuration.GetConnectionString("MySqlConnection");
                    services.AddDbContext<CinemaDbContext>(options =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

                    services.AddScoped<IMovieRepository, MovieRepository>();
                    services.AddScoped<IHallRepository, HallRepository>();
                    services.AddScoped<IScreeningRepository, ScreeningRepository>();
                    services.AddScoped<IReservationRepository, ReservationRepository>();
                    services.AddScoped<IUserRepository, UserRepository>();

                    services.AddScoped<MovieManager>();
                    services.AddScoped<HallManager>();
                    services.AddScoped<ScreeningManager>();
                    services.AddScoped<ReservationManager>();
                    services.AddScoped<AuthenticationManager>();

                    services.AddTransient<MainWindow>();  
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<AdminDashboard>();
                    services.AddTransient<SpectatorDashboard>();
                    services.AddTransient<MovieManagementWindow>();
                    services.AddTransient<HallManagementWindow>();
                    services.AddTransient<ScreeningManagementWindow>();
                    services.AddTransient<BookingWindow>();
                    services.AddTransient<MyReservationsWindow>();
                })
                .Build();

            await EnsureDatabaseAsync();

            await CreateDefaultAccountsAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }


        private async Task EnsureDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

            try
            {
                logger.LogInformation("Ensuring database is created...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database is ready");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error ensuring database");
                MessageBox.Show($"Database connection failed: {ex.Message}\n\nPlease ensure XAMPP MySQL is running.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }


        private async Task CreateDefaultAccountsAsync()
        {
            using var scope = _host.Services.CreateScope();
            var authManager = scope.ServiceProvider.GetRequiredService<AuthenticationManager>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();

            try
            {
                await authManager.CreateDefaultAdminAsync();
                logger.LogInformation("Default accounts initialized");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not create default accounts (may already exist)");
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }


        public static T GetService<T>()
        {
            return ((App)Current)._host.Services.GetRequiredService<T>();
        }
    }
}
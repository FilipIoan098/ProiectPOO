// CinemaManagement.UI/Views/MainWindow.xaml.cs

using System.Windows;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI
{
    public partial class MainWindow : Window
    {
        private readonly AuthenticationManager _authManager;
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(AuthenticationManager authManager, ILogger<MainWindow> logger)
        {
            InitializeComponent();
            _authManager = authManager;
            _logger = logger;

            _logger.LogInformation("Main welcome window loaded");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("User clicked Login button");

            try
            {
                var loginWindow = App.GetService<Views.LoginWindow>();
                loginWindow.Owner = this;

                this.Hide();

                loginWindow.Closed += (s, args) =>
                {
                    bool otherWindowsOpen = false;
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != this && window != loginWindow && window.IsVisible)
                        {
                            otherWindowsOpen = true;
                            break;
                        }
                    }

                    if (!otherWindowsOpen)
                    {
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                };

                loginWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening login window");
                MessageBox.Show("Failed to open login window. Please try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("User clicked Register button");

            try
            {
                var registerWindow = new Views.RegisterWindow(_authManager,
                    _logger as ILogger ?? throw new InvalidOperationException("Logger not available"));
                registerWindow.Owner = this;

                this.Hide();

                registerWindow.Closed += async (s, args) =>
                {
                    if (registerWindow.DialogResult == true)
                    {
                        _logger.LogInformation("Registration successful, attempting auto-login");

                        var username = registerWindow.RegisteredUsername;
                        var password = registerWindow.RegisteredPassword;

                        try
                        {
                            var user = await _authManager.LoginAsync(username, password);

                            _logger.LogInformation($"Auto-login successful for user: {username}");

                            MessageBox.Show(
                                $"Welcome aboard, {username}!\n\n" +
                                "Your account has been created successfully.\n" +
                                "You are now logged in.",
                                "Registration Successful",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            if (user.Role == Core.Entities.UserRole.Administrator)
                            {
                                var adminDashboard = App.GetService<Views.AdminDashboard>();
                                adminDashboard.SetCurrentUser(user);
                                adminDashboard.Show();
                            }
                            else
                            {
                                var spectatorDashboard = App.GetService<Views.SpectatorDashboard>();
                                spectatorDashboard.SetCurrentUser(user);
                                spectatorDashboard.Show();
                            }

                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Auto-login failed after registration");
                            MessageBox.Show(
                                "Registration successful but auto-login failed.\n" +
                                "Please login manually with your credentials.",
                                "Login Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            this.Show();
                        }
                    }
                    else
                    {
                        this.Show();
                    }
                };

                registerWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening register window");
                MessageBox.Show("Failed to open registration window. Please try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }
    }
}
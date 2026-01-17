// CinemaManagement.UI/Views/LoginWindow.xaml.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace CinemaManagement.UI.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationManager _authManager;
        private readonly ILogger<LoginWindow> _logger;

        public LoginWindow(AuthenticationManager authManager, ILogger<LoginWindow> logger)
        {
            InitializeComponent();
            _authManager = authManager;
            _logger = logger;

            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter username and password",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            SetLoginState(isLoading: true);

            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", username);

                User user = await _authManager.LoginAsync(username, password);

                _logger.LogInformation("User logged in: {Username} as {Role}", username, user.Role);

                if (user.Role == UserRole.Administrator)
                {
                    var adminDashboard = App.GetService<AdminDashboard>();
                    adminDashboard.SetCurrentUser(user);
                    adminDashboard.Show();
                }
                else
                {
                    var spectatorDashboard = App.GetService<SpectatorDashboard>();
                    spectatorDashboard.SetCurrentUser(user);
                    spectatorDashboard.Show();
                }

                Close();
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning(ex, "Login failed");
                MessageBox.Show(ex.Message, "Login Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                MessageBox.Show(
                    "An unexpected error occurred. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SetLoginState(isLoading: false);
            }
        }

        private void SetLoginState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            LoginButton.Content = isLoading ? "LOGGING IN..." : "LOGIN";
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow(_authManager, _logger)
            {
                Owner = this
            };

            if (registerWindow.ShowDialog() == true)
            {
                MessageBox.Show(
                    "Registration successful! You can now login.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}

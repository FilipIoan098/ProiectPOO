// CinemaManagement.UI/Views/RegisterWindow.xaml.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;

namespace CinemaManagement.UI.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthenticationManager _authManager;
        private readonly ILogger _logger;


        public string RegisteredUsername { get; private set; }
        public string RegisteredPassword { get; private set; }


        public RegisterWindow(AuthenticationManager authManager, ILogger logger)
        {
            InitializeComponent();
            _authManager = authManager;
            _logger = logger;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            button.IsEnabled = false;
            button.Content = "REGISTERING...";

            try
            {
                await _authManager.RegisterAsync(username, password, UserRole.Spectator);
                _logger.LogInformation($"New user registered: {username}");

                RegisteredUsername = username;
                RegisteredPassword = password;

                DialogResult = true;
                Close();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Registration validation failed: {ex.Message}");
                MessageBox.Show(ex.Message, "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                button.IsEnabled = true;
                button.Content = "REGISTER";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                MessageBox.Show("Registration failed. Please try again.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
                button.Content = "REGISTER";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("User cancelled registration");
            DialogResult = false;
            Close();
        }
    }
}
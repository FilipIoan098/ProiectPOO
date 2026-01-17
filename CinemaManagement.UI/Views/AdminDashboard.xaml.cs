// CinemaManagement.UI/Views/AdminDashboard.xaml.cs

using System.Windows;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;


namespace CinemaManagement.UI.Views
{
    public partial class AdminDashboard : Window
    {
        private readonly MovieManager _movieManager;
        private readonly HallManager _hallManager;
        private readonly ScreeningManager _screeningManager;
        private readonly ILogger<AdminDashboard> _logger;
        private User _currentUser;

        public AdminDashboard(
            MovieManager movieManager,
            HallManager hallManager,
            ScreeningManager screeningManager,
            ILogger<AdminDashboard> logger)
        {
            InitializeComponent();
            _movieManager = movieManager;
            _hallManager = hallManager;
            _screeningManager = screeningManager;
            _logger = logger;

            Loaded += AdminDashboard_Loaded;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            WelcomeText.Text = $"Welcome, {user.Username}";
        }

        private async void AdminDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDashboardStats();
        }

        private async Task LoadDashboardStats()
        {
            try
            {
                var activeMovies = await _movieManager.GetActiveMoviesAsync();
                var halls = await _hallManager.GetAllHallsAsync();
                var screenings = await _screeningManager.GetUpcomingScreeningsAsync();

                ActiveMoviesCount.Text = activeMovies.Count.ToString();
                TotalHallsCount.Text = halls.Count.ToString();
                UpcomingScreeningsCount.Text = screenings.Count.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard stats");
            }
        }

        private void ManageMovies_Click(object sender, RoutedEventArgs e)
        {
            var movieWindow = App.GetService<MovieManagementWindow>();
            movieWindow.ShowDialog();
            _ = LoadDashboardStats();
        }

        private void ManageHalls_Click(object sender, RoutedEventArgs e)
        {
            var hallWindow = App.GetService<HallManagementWindow>();
            hallWindow.ShowDialog();
            _ = LoadDashboardStats();
        }

        private void ManageScreenings_Click(object sender, RoutedEventArgs e)
        {
            var screeningWindow = App.GetService<ScreeningManagementWindow>();
            screeningWindow.ShowDialog();
            _ = LoadDashboardStats();
        }

        private void ViewReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reports feature coming soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = App.GetService<LoginWindow>();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}
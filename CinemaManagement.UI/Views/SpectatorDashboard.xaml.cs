// CinemaManagement.UI/Views/SpectatorDashboard.xaml.cs

using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI.Views
{
    public partial class SpectatorDashboard : Window
    {
        private readonly MovieManager _movieManager;
        private readonly ScreeningManager _screeningManager;
        private readonly ReservationManager _reservationManager;
        private readonly ILogger<SpectatorDashboard> _logger;
        private User _currentUser;
        private List<Movie> _allMovies;

        public SpectatorDashboard(
            MovieManager movieManager,
            ScreeningManager screeningManager,
            ReservationManager reservationManager,
            ILogger<SpectatorDashboard> logger)
        {
            InitializeComponent();
            _movieManager = movieManager;
            _screeningManager = screeningManager;
            _reservationManager = reservationManager;
            _logger = logger;

            Loaded += async (s, e) => await LoadMovies();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            WelcomeText.Text = $"Welcome, {user.Username}!";
        }

        private async Task LoadMovies()
        {
            try
            {
                _allMovies = await _movieManager.GetActiveMoviesAsync();
                MoviesItemsControl.ItemsSource = _allMovies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movies");
                MessageBox.Show("Failed to load movies", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseMovies_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadMovies();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MoviesItemsControl.ItemsSource = _allMovies;
            }
            else
            {
                var filtered = _allMovies.Where(m =>
                    m.Title.ToLower().Contains(searchTerm) ||
                    m.Genre.ToLower().Contains(searchTerm)).ToList();
                MoviesItemsControl.ItemsSource = filtered;
            }
        }

        private async void ViewShowtimes_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int movieId = (int)button.Tag;

            try
            {
                var screenings = await _screeningManager.GetScreeningsByMovieAsync(movieId);
                screenings = screenings.Where(s => s.ShowDateTime > DateTime.Now).ToList();

                if (screenings.Count == 0)
                {
                    MessageBox.Show("No upcoming showtimes for this movie", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var screeningDialog = new ScreeningSelectionDialog(screenings);
                if (screeningDialog.ShowDialog() == true)
                {
                    var selectedScreening = screeningDialog.SelectedScreening;

                    var bookingWindow = new SeatSelectionWindow(
                        selectedScreening,
                        _currentUser,
                        _reservationManager,
                        _logger);
                    bookingWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading showtimes");
                MessageBox.Show("Failed to load showtimes", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MyReservations_Click(object sender, RoutedEventArgs e)
        {
            var reservationsWindow = App.GetService<MyReservationsWindow>();
            reservationsWindow.SetCurrentUser(_currentUser);
            reservationsWindow.ShowDialog();
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

    public class ScreeningSelectionDialog : Window
    {
        public Screening SelectedScreening { get; private set; }

        public ScreeningSelectionDialog(List<Screening> screenings)
        {
            Width = 600;
            Height = 400;
            Title = "Select Showtime";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(13, 17, 23));

            var stack = new StackPanel { Margin = new Thickness(20) };

            var title = new TextBlock
            {
                Text = "Available Showtimes",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stack.Children.Add(title);

            var listBox = new ListBox
            {
                Height = 250,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(22, 27, 34)),
                BorderThickness = new Thickness(0)
            };

            foreach (var screening in screenings)
            {
                var item = new ListBoxItem
                {
                    Content = $"{screening.ShowDateTime:ddd, MMM dd yyyy - hh:mm tt} | " +
                             $"{screening.Hall.Name} ({screening.Hall.Type}) | " +
                             $"Price: ${screening.GetFinalPrice():F2}",
                    Tag = screening,
                    Foreground = System.Windows.Media.Brushes.White,
                    Padding = new Thickness(10)
                };
                listBox.Items.Add(item);
            }

            stack.Children.Add(listBox);

            var selectBtn = new Button
            {
                Content = "Select Showtime",
                Margin = new Thickness(0, 15, 0, 0),
                Padding = new Thickness(15, 10, 15, 10),
            };
            selectBtn.Click += (s, e) => {
                if (listBox.SelectedItem != null)
                {
                    SelectedScreening = (Screening)((ListBoxItem)listBox.SelectedItem).Tag;
                    DialogResult = true;
                    Close();
                }
            };
            stack.Children.Add(selectBtn);

            Content = stack;
        }
    }
}
// CinemaManagement.UI/Views/MovieManagementWindow.xaml.cs

using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Core.Managers;
using CinemaManagement.Core.Entities;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI.Views
{
    public partial class MovieManagementWindow : Window
    {
        private readonly MovieManager _movieManager;
        private readonly ILogger<MovieManagementWindow> _logger;

        public MovieManagementWindow(MovieManager movieManager, ILogger<MovieManagementWindow> logger)
        {
            InitializeComponent();
            _movieManager = movieManager;
            _logger = logger;
            Loaded += async (s, e) => await LoadMovies();
        }

        private async Task LoadMovies()
        {
            try
            {
                var movies = await _movieManager.GetAllMoviesAsync();
                MoviesDataGrid.ItemsSource = movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movies");
                MessageBox.Show("Failed to load movies", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMovie_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MovieDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = SaveMovie(dialog.MovieTitle, dialog.MovieGenre, dialog.MovieDuration, dialog.MovieRating);
            }
        }

        private async Task SaveMovie(string title, string genre, int duration, decimal rating)
        {
            try
            {
                await _movieManager.CreateMovieAsync(title, genre, duration, rating);
                MessageBox.Show("Movie added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMovies();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMovie_Click(object sender, RoutedEventArgs e)
        {
            var selected = MoviesDataGrid.SelectedItem as Movie;
            if (selected == null)
            {
                MessageBox.Show("Please select a movie to edit", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new MovieDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateMovie(selected.Id, dialog.MovieTitle, dialog.MovieGenre,
                    dialog.MovieDuration, dialog.MovieRating);
            }
        }

        private async Task UpdateMovie(int id, string title, string genre, int duration, decimal rating)
        {
            try
            {
                await _movieManager.UpdateMovieAsync(id, title, genre, duration, rating);
                MessageBox.Show("Movie updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadMovies();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RetireMovie_Click(object sender, RoutedEventArgs e)
        {
            var selected = MoviesDataGrid.SelectedItem as Movie;
            if (selected == null) return;

            var result = MessageBox.Show($"Retire movie '{selected.Title}'?",
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _movieManager.RetireMovieAsync(selected.Id);
                    await LoadMovies();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ActivateMovie_Click(object sender, RoutedEventArgs e)
        {
            var selected = MoviesDataGrid.SelectedItem as Movie;
            if (selected == null) return;

            try
            {
                await _movieManager.ActivateMovieAsync(selected.Id);
                await LoadMovies();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadMovies();
        }
    }

    public partial class MovieDialog : Window
    {
        public string MovieTitle { get; private set; }
        public string MovieGenre { get; private set; }
        public int MovieDuration { get; private set; }
        public decimal MovieRating { get; private set; }

        public MovieDialog(Movie existing = null)
        {
            Width = 400;
            Height = 400;
            Title = existing == null ? "Add Movie" : "Edit Movie";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(26, 26, 46));

            var stack = new StackPanel { Margin = new Thickness(20) };

            var titleBox = CreateTextBox("Title:", existing?.Title ?? "");
            var genreBox = CreateTextBox("Genre:", existing?.Genre ?? "");
            var durationBox = CreateTextBox("Duration (minutes):", existing?.DurationMinutes.ToString() ?? "120");
            var ratingBox = CreateTextBox("Rating (0-5):", existing?.Rating.ToString() ?? "4.0");

            stack.Children.Add(titleBox.Item1);
            stack.Children.Add(titleBox.Item2);
            stack.Children.Add(genreBox.Item1);
            stack.Children.Add(genreBox.Item2);
            stack.Children.Add(durationBox.Item1);
            stack.Children.Add(durationBox.Item2);
            stack.Children.Add(ratingBox.Item1);
            stack.Children.Add(ratingBox.Item2);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 20, 0, 0) };
            var saveBtn = new Button { Content = "Save", Width = 100, Padding = new Thickness(10), Margin = new Thickness(0, 0, 10, 0) };
            var cancelBtn = new Button { Content = "Cancel", Width = 100, Padding = new Thickness(10) };

            saveBtn.Click += (s, e) =>
            {
                MovieTitle = titleBox.Item2.Text;
                MovieGenre = genreBox.Item2.Text;
                if (!int.TryParse(durationBox.Item2.Text, out int dur) || dur <= 0)
                {
                    MessageBox.Show("Invalid duration");
                    return;
                }
                if (!decimal.TryParse(ratingBox.Item2.Text, out decimal rat) || rat < 0 || rat > 5)
                {
                    MessageBox.Show("Invalid rating (0-5)");
                    return;
                }
                MovieDuration = dur;
                MovieRating = rat;
                DialogResult = true;
                Close();
            };
            cancelBtn.Click += (s, e) => { DialogResult = false; Close(); };

            btnPanel.Children.Add(saveBtn);
            btnPanel.Children.Add(cancelBtn);
            stack.Children.Add(btnPanel);

            Content = stack;
        }

        private (TextBlock, TextBox) CreateTextBox(string label, string value)
        {
            var lbl = new TextBlock
            {
                Text = label,
                Foreground = System.Windows.Media.Brushes.LightGray,
                Margin = new Thickness(0, 10, 0, 5)
            };
            var txt = new TextBox
            {
                Text = value,
                Padding = new Thickness(8),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(22, 27, 34))
            };
            return (lbl, txt);
        }
    }
}
// CinemaManagement.UI/Views/ScreeningManagementWindow.xaml.cs

using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI.Views
{
    public partial class ScreeningManagementWindow : Window
    {
        private readonly ScreeningManager _screeningManager;
        private readonly MovieManager _movieManager;
        private readonly HallManager _hallManager;
        private readonly ILogger<ScreeningManagementWindow> _logger;

        public ScreeningManagementWindow(
            ScreeningManager screeningManager,
            MovieManager movieManager,
            HallManager hallManager,
            ILogger<ScreeningManagementWindow> logger)
        {
            InitializeComponent();
            _screeningManager = screeningManager;
            _movieManager = movieManager;
            _hallManager = hallManager;
            _logger = logger;
            Loaded += async (s, e) => await LoadScreenings();
        }

        private async Task LoadScreenings()
        {
            try
            {
                var screenings = await _screeningManager.GetUpcomingScreeningsAsync();
                ScreeningsDataGrid.ItemsSource = screenings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading screenings");
                MessageBox.Show("Failed to load screenings", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddScreening_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var movies = await _movieManager.GetActiveMoviesAsync();
                var halls = await _hallManager.GetAllHallsAsync();

                if (movies.Count == 0)
                {
                    MessageBox.Show("No active movies available. Add movies first.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (halls.Count == 0)
                {
                    MessageBox.Show("No halls available. Add halls first.", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new ScreeningDialog(movies, halls);
                if (dialog.ShowDialog() == true)
                {
                    await _screeningManager.CreateScreeningAsync(
                        dialog.SelectedMovieId,
                        dialog.SelectedHallId,
                        dialog.ShowDateTime,
                        dialog.BasePrice,
                        dialog.ScreeningType);

                    MessageBox.Show("Screening added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadScreenings();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding screening");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadScreenings();
        }
    }

    public class ScreeningDialog : Window
    {
        public int SelectedMovieId { get; private set; }
        public int SelectedHallId { get; private set; }
        public DateTime ShowDateTime { get; private set; }
        public decimal BasePrice { get; private set; }
        public ScreeningType ScreeningType { get; private set; }

        public ScreeningDialog(List<Movie> movies, List<Hall> halls)
        {
            Width = 450;
            Height = 500;
            Title = "Add Screening";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var stack = new StackPanel { Margin = new Thickness(20) };

            var movieLabel = new TextBlock { Text = "Select Movie:", Margin = new Thickness(0, 10, 0, 5) };
            var movieCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
            foreach (var movie in movies)
                movieCombo.Items.Add(new ComboBoxItem { Content = movie.Title, Tag = movie.Id });
            movieCombo.SelectedIndex = 0;

            var hallLabel = new TextBlock { Text = "Select Hall:", Margin = new Thickness(0, 10, 0, 5) };
            var hallCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 15) };
            foreach (var hall in halls)
                hallCombo.Items.Add(new ComboBoxItem { Content = $"{hall.Name} ({hall.Type})", Tag = hall.Id });
            hallCombo.SelectedIndex = 0;

            var dateLabel = new TextBlock { Text = "Show Date:", Margin = new Thickness(0, 10, 0, 5) };
            var datePicker = new DatePicker { Margin = new Thickness(0, 0, 0, 15), SelectedDate = DateTime.Today.AddDays(1) };

            var timeLabel = new TextBlock { Text = "Show Time (24hr format, e.g., 18:30):", Margin = new Thickness(0, 10, 0, 5) };
            var timeBox = new TextBox { Text = "18:00", Margin = new Thickness(0, 0, 0, 15) };

            var priceLabel = new TextBlock { Text = "Base Price ($):", Margin = new Thickness(0, 10, 0, 5) };
            var priceBox = new TextBox { Text = "12.00", Margin = new Thickness(0, 0, 0, 15) };

            var typeLabel = new TextBlock { Text = "Screening Type:", Margin = new Thickness(0, 10, 0, 5) };
            var typeCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 20) };
            typeCombo.Items.Add(ScreeningType.Matinee);
            typeCombo.Items.Add(ScreeningType.Evening);
            typeCombo.Items.Add(ScreeningType.Weekend);
            typeCombo.SelectedIndex = 1;

            stack.Children.Add(movieLabel);
            stack.Children.Add(movieCombo);
            stack.Children.Add(hallLabel);
            stack.Children.Add(hallCombo);
            stack.Children.Add(dateLabel);
            stack.Children.Add(datePicker);
            stack.Children.Add(timeLabel);
            stack.Children.Add(timeBox);
            stack.Children.Add(priceLabel);
            stack.Children.Add(priceBox);
            stack.Children.Add(typeLabel);
            stack.Children.Add(typeCombo);

            var saveBtn = new Button { Content = "Create Screening", Padding = new Thickness(15, 10, 15, 10) };
            saveBtn.Click += (s, e) => {
                try
                {
                    SelectedMovieId = (int)((ComboBoxItem)movieCombo.SelectedItem).Tag;
                    SelectedHallId = (int)((ComboBoxItem)hallCombo.SelectedItem).Tag;

                    var date = datePicker.SelectedDate.Value;
                    var timeParts = timeBox.Text.Split(':');
                    ShowDateTime = new DateTime(date.Year, date.Month, date.Day,
                        int.Parse(timeParts[0]), int.Parse(timeParts[1]), 0);

                    BasePrice = decimal.Parse(priceBox.Text);
                    ScreeningType = (ScreeningType)typeCombo.SelectedItem;

                    DialogResult = true;
                    Close();
                }
                catch
                {
                    MessageBox.Show("Invalid input. Please check all fields.", "Error");
                }
            };

            stack.Children.Add(saveBtn);
            Content = new ScrollViewer { Content = stack };
        }
    }
}
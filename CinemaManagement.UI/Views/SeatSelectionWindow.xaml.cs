// CinemaManagement.UI/Views/SeatSelectionWindow.xaml.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CinemaManagement.UI.Views
{
    public partial class SeatSelectionWindow : Window
    {
        private readonly Screening _screening;
        private readonly User _currentUser;
        private readonly ReservationManager _reservationManager;
        private readonly ILogger _logger;
        private List<string> _bookedSeats;
        private List<string> _selectedSeats = new();
        private Dictionary<string, Button> _seatButtons = new();

        public SeatSelectionWindow(
            Screening screening,
            User currentUser,
            ReservationManager reservationManager,
            ILogger logger)
        {
            InitializeComponent();

            _screening = screening;
            _currentUser = currentUser;
            _reservationManager = reservationManager;
            _logger = logger;

            InitializeWindow();
        }

        private void InitializeWindow()
        {
            HeaderText.Text = $"{_screening.Movie.Title} - {_screening.ShowDateTime:ddd, MMM dd - hh:mm tt}";
            SubHeaderText.Text = $"{_screening.Hall.Name} ({_screening.Hall.Type}) | Price per seat: ${_screening.GetFinalPrice():F2}";

            SeatGrid.Rows = _screening.Hall.TotalRows;
            SeatGrid.Columns = _screening.Hall.SeatsPerRow;

            for (int row = 0; row < _screening.Hall.TotalRows; row++)
            {
                for (int seat = 0; seat < _screening.Hall.SeatsPerRow; seat++)
                {
                    string seatNumber = $"{(char)('A' + row)}{seat + 1}";
                    var seatButton = CreateSeatButton(seatNumber);
                    _seatButtons[seatNumber] = seatButton;
                    SeatGrid.Children.Add(seatButton);
                }
            }

            Loaded += async (s, e) => await LoadBookedSeats();
        }

        private Button CreateSeatButton(string seatNumber)
        {
            var button = new Button
            {
                Content = seatNumber,
                Width = 50,
                Height = 40,
                Margin = new Thickness(2),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Color.FromRgb(35, 134, 54)), 
                Tag = seatNumber
            };

            button.Click += SeatButton_Click;
            return button;
        }

        private async Task LoadBookedSeats()
        {
            try
            {
                _bookedSeats = await _reservationManager.GetBookedSeatsAsync(_screening.Id);

                foreach (var seat in _bookedSeats)
                {
                    if (_seatButtons.ContainsKey(seat))
                    {
                        _seatButtons[seat].Background = new SolidColorBrush(Color.FromRgb(153, 27, 27));
                        _seatButtons[seat].IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booked seats");
            }
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string seatNumber = button.Tag.ToString();

            if (_selectedSeats.Contains(seatNumber))
            {
                _selectedSeats.Remove(seatNumber);
                button.Background = new SolidColorBrush(Color.FromRgb(35, 134, 54)); 
            }
            else
            {
                _selectedSeats.Add(seatNumber);
                button.Background = new SolidColorBrush(Color.FromRgb(88, 166, 255)); 
            }

            UpdateSelectionDisplay();
        }

        private void UpdateSelectionDisplay()
        {
            if (_selectedSeats.Count == 0)
            {
                SelectedSeatsLabel.Text = "Selected Seats: None";
                TotalLabel.Text = "Total: $0.00";
            }
            else
            {
                SelectedSeatsLabel.Text = $"Selected Seats: {string.Join(", ", _selectedSeats)}";
                decimal total = _screening.GetFinalPrice() * _selectedSeats.Count;
                TotalLabel.Text = $"Total: ${total:F2}";
            }
        }

        private async void BookButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSeats.Count == 0)
            {
                MessageBox.Show("Please select at least one seat", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Confirm booking {_selectedSeats.Count} seat(s) for ${_screening.GetFinalPrice() * _selectedSeats.Count:F2}?",
                "Confirm Booking", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _reservationManager.CreateReservationAsync(
                        _currentUser.Id,
                        _screening.Id,
                        _selectedSeats);

                    MessageBox.Show("Booking successful! Enjoy the movie!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                catch (SeatsUnavailableException ex)
                {
                    MessageBox.Show(ex.Message, "Seats Unavailable",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    await LoadBookedSeats(); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Booking failed");
                    MessageBox.Show("Booking failed. Please try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
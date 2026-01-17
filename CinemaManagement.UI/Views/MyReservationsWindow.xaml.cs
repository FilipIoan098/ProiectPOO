// CinemaManagement.UI/Views/MyReservationsWindow.xaml.cs

using System.Windows;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Managers;
using CinemaManagement.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI.Views
{
    public partial class MyReservationsWindow : Window
    {
        private readonly ReservationManager _reservationManager;
        private readonly ILogger<MyReservationsWindow> _logger;
        private User _currentUser;

        public MyReservationsWindow(
            ReservationManager reservationManager,
            ILogger<MyReservationsWindow> logger)
        {
            InitializeComponent();
            _reservationManager = reservationManager;
            _logger = logger;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            Loaded += async (s, e) => await LoadReservations();
        }

        private async Task LoadReservations()
        {
            try
            {
                var reservations = await _reservationManager.GetUserReservationsAsync(_currentUser.Id);
                ReservationsDataGrid.ItemsSource = reservations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations");
                MessageBox.Show("Failed to load reservations", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            var selected = ReservationsDataGrid.SelectedItem as Reservation;
            if (selected == null)
            {
                MessageBox.Show("Please select a reservation to cancel", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selected.Status == ReservationStatus.Cancelled)
            {
                MessageBox.Show("This reservation is already cancelled", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Cancel reservation for '{selected.Screening.Movie.Title}'?\n" +
                $"Seats: {selected.SeatNumbers}\n" +
                $"Show time: {selected.Screening.ShowDateTime:MMM dd, yyyy hh:mm tt}",
                "Confirm Cancellation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _reservationManager.CancelReservationAsync(selected.Id, _currentUser.Id);

                    MessageBox.Show("Reservation cancelled successfully", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadReservations();
                }
                catch (ReservationCancellationException ex)
                {
                    MessageBox.Show(ex.Message, "Cannot Cancel",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cancelling reservation");
                    MessageBox.Show("Failed to cancel reservation", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadReservations();
        }
    }
}
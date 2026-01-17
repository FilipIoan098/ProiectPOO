// Managers/ReservationManager.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using CinemaManagement.Core.Exceptions;

namespace CinemaManagement.Core.Managers
{
    public class ReservationManager
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IScreeningRepository _screeningRepo;
        private readonly ILogger<ReservationManager> _logger;

        public ReservationManager(
            IReservationRepository reservationRepo,
            IScreeningRepository screeningRepo,
            ILogger<ReservationManager> logger)
        {
            _reservationRepo = reservationRepo;
            _screeningRepo = screeningRepo;
            _logger = logger;
        }

        public async Task<Reservation> CreateReservationAsync(int userId, int screeningId, List<string> seats)
        {
            _logger.LogInformation($"Creating reservation for user {userId}, screening {screeningId}");

            var screening = await _screeningRepo.GetByIdAsync(screeningId);
            if (screening == null)
                throw new EntityNotFoundException("Screening", screeningId);

            if (screening.ShowDateTime < DateTime.Now)
                throw new DataValidationException("Cannot book past screenings");

            var bookedSeats = await _reservationRepo.GetBookedSeatsForScreeningAsync(screeningId);
            var unavailableSeats = seats.Intersect(bookedSeats).ToList();

            if (unavailableSeats.Any())
                throw new SeatsUnavailableException(string.Join(", ", unavailableSeats));

            decimal totalPrice = screening.GetFinalPrice() * seats.Count;

            try
            {
                var reservation = new Reservation(userId, screeningId, string.Join(",", seats), totalPrice);
                var result = await _reservationRepo.AddAsync(reservation);

                _logger.LogInformation($"Reservation created: ID {result.Id}, Seats: {result.SeatNumbers}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                throw new CinemaException("Failed to create reservation", ex);
            }
        }

        public async Task<Reservation> CancelReservationAsync(int reservationId, int userId)
        {
            _logger.LogInformation($"Cancelling reservation {reservationId}");

            var reservation = await _reservationRepo.GetByIdAsync(reservationId);
            if (reservation == null)
                throw new EntityNotFoundException("Reservation", reservationId);

            Console.WriteLine("reservation found : "+reservation.Id);

            if (reservation.UserId != userId)
                throw new UnauthorizedException("You can only cancel your own reservations");

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new ReservationCancellationException("Reservation is already cancelled");

            var screening = await _screeningRepo.GetByIdAsync(reservation.ScreeningId);
            if (screening == null)
                throw new EntityNotFoundException("Screening", reservation.ScreeningId);

            if (screening.ShowDateTime < DateTime.Now)
                throw new ReservationCancellationException("Cannot cancel past screenings");

            try
            {
                var cancelledReservation = reservation.Cancel();
                return await _reservationRepo.UpdateAsync(cancelledReservation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling reservation {reservationId}");
                throw new CinemaException("Failed to cancel reservation", ex);
            }
        }

        public async Task<List<Reservation>> GetUserReservationsAsync(int userId) =>
            await _reservationRepo.GetByUserIdAsync(userId);

        public async Task<List<string>> GetBookedSeatsAsync(int screeningId) =>
            await _reservationRepo.GetBookedSeatsForScreeningAsync(screeningId);
    }
}

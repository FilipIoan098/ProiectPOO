// CinemaManagement.Tests/ReservationManagerTests.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CinemaManagement.Tests
{
    public class ReservationManagerTests
    {
        private readonly Mock<IReservationRepository> _mockReservationRepo;
        private readonly Mock<IScreeningRepository> _mockScreeningRepo;
        private readonly Mock<ILogger<ReservationManager>> _mockLogger;
        private readonly ReservationManager _reservationManager;

        public ReservationManagerTests()
        {
            _mockReservationRepo = new Mock<IReservationRepository>();
            _mockScreeningRepo = new Mock<IScreeningRepository>();
            _mockLogger = new Mock<ILogger<ReservationManager>>();
            _reservationManager = new ReservationManager(
                _mockReservationRepo.Object,
                _mockScreeningRepo.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateReservation_WithAvailableSeats_ShouldSucceed()
        {
            // Arrange
            var userId = 1;
            var screeningId = 1;
            var seats = new List<string> { "A1", "A2" };

            var screening = new Screening(1, 1, DateTime.Now.AddDays(1), 10.0m, ScreeningType.Evening)
            {
                Id = screeningId,
                Movie = new Movie("Test Movie", "Action", 120, 4.5m) { Id = 1 },
                Hall = new Hall("Hall A", HallType.Standard, 10, 10) { Id = 1 }
            };

            _mockScreeningRepo.Setup(r => r.GetByIdAsync(screeningId))
                             .ReturnsAsync(screening);

            _mockReservationRepo.Setup(r => r.GetBookedSeatsForScreeningAsync(screeningId))
                               .ReturnsAsync(new List<string>()); // No seats booked

            _mockReservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
                               .ReturnsAsync((Reservation res) => res);

            // Act
            var result = await _reservationManager.CreateReservationAsync(userId, screeningId, seats);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(screeningId, result.ScreeningId);
            Assert.Equal("A1,A2", result.SeatNumbers);
            Assert.Equal(ReservationStatus.Active, result.Status);
        }

        [Fact]
        public async Task CreateReservation_WithBookedSeats_ShouldThrowSeatsUnavailableException()
        {
            // Arrange
            var userId = 1;
            var screeningId = 1;
            var seats = new List<string> { "A1", "A2" };

            var screening = new Screening(1, 1, DateTime.Now.AddDays(1), 10.0m, ScreeningType.Evening)
            {
                Id = screeningId
            };

            _mockScreeningRepo.Setup(r => r.GetByIdAsync(screeningId))
                             .ReturnsAsync(screening);

            _mockReservationRepo.Setup(r => r.GetBookedSeatsForScreeningAsync(screeningId))
                               .ReturnsAsync(new List<string> { "A1" }); // A1 already booked

            // Act & Assert
            await Assert.ThrowsAsync<SeatsUnavailableException>(
                async () => await _reservationManager.CreateReservationAsync(userId, screeningId, seats)
            );
        }

        [Fact]
        public async Task CancelReservation_ValidReservation_ShouldSucceed()
        {
            // Arrange
            var reservationId = 1;
            var userId = 1;

            var screening = new Screening(1, 1, DateTime.Now.AddDays(1), 10.0m, ScreeningType.Evening)
            {
                Id = 1
            };

            var reservation = new Reservation(userId, 1, "A1,A2", 20.0m)
            {
                Id = reservationId,
                Screening = screening
            };

            _mockReservationRepo.Setup(r => r.GetByIdAsync(reservationId))
                               .ReturnsAsync(reservation);

            _mockScreeningRepo.Setup(r => r.GetByIdAsync(1))
                             .ReturnsAsync(screening);

            _mockReservationRepo.Setup(r => r.UpdateAsync(It.IsAny<Reservation>()))
                               .ReturnsAsync((Reservation res) => res);

            // Act
            var result = await _reservationManager.CancelReservationAsync(reservationId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReservationStatus.Cancelled, result.Status);
            Assert.NotNull(result.CancelledAt);
        }

        [Fact]
        public async Task CancelReservation_DifferentUser_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var reservationId = 1;
            var ownerId = 1;
            var differentUserId = 2;

            var reservation = new Reservation(ownerId, 1, "A1", 10.0m)
            {
                Id = reservationId
            };

            _mockReservationRepo.Setup(r => r.GetByIdAsync(reservationId))
                               .ReturnsAsync(reservation);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _reservationManager.CancelReservationAsync(reservationId, differentUserId)
            );
        }
    }
}
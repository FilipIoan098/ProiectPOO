// CinemaManagement.Tests/EntityTests.cs

using Xunit;
using CinemaManagement.Core.Entities;

namespace CinemaManagement.Tests
{
    public class EntityTests
    {
        [Fact]
        public void Movie_Retire_ShouldCreateNewInstanceWithIsActiveFalse()
        {
            var movie = new Movie("Test Movie", "Action", 120, 4.5m);
            Assert.True(movie.IsActive);

            var retiredMovie = movie.Retire();

            Assert.False(retiredMovie.IsActive);
            Assert.True(movie.IsActive); 
            Assert.NotSame(movie, retiredMovie);
        }

        [Fact]
        public void Movie_Update_ShouldCreateNewInstance()
        {
            var movie = new Movie("Old Title", "Drama", 90, 3.0m);

            var updatedMovie = movie.Update("New Title", "Comedy", 120, 4.5m);

            Assert.Equal("Old Title", movie.Title);
            Assert.Equal("New Title", updatedMovie.Title);
            Assert.NotSame(movie, updatedMovie);
        }

        [Fact]
        public void Screening_GetFinalPrice_ShouldCalculateCorrectly()
        {
            var basePrice = 10.0m;

            var matineeScreening = new Screening(1, 1, DateTime.Now, basePrice, ScreeningType.Matinee);
            var eveningScreening = new Screening(1, 1, DateTime.Now, basePrice, ScreeningType.Evening);
            var weekendScreening = new Screening(1, 1, DateTime.Now, basePrice, ScreeningType.Weekend);

            Assert.Equal(8.0m, matineeScreening.GetFinalPrice()); // 20% discount
            Assert.Equal(10.0m, eveningScreening.GetFinalPrice()); // No change
            Assert.Equal(12.0m, weekendScreening.GetFinalPrice()); // 20% premium
        }

        [Fact]
        public void Reservation_Cancel_ShouldSetStatusToCancelled()
        {
            var reservation = new Reservation(1, 1, "A1,A2", 20.0m);
            Assert.Equal(ReservationStatus.Active, reservation.Status);

            var cancelledReservation = reservation.Cancel();

            Assert.Equal(ReservationStatus.Cancelled, cancelledReservation.Status);
            Assert.NotNull(cancelledReservation.CancelledAt);
            Assert.Equal(ReservationStatus.Active, reservation.Status);
        }

        [Fact]
        public void Reservation_GetSeats_ShouldReturnListOfSeats()
        {
            var reservation = new Reservation(1, 1, "A1,A2,B5", 30.0m);

            var seats = reservation.GetSeats();

            Assert.Equal(3, seats.Count);
            Assert.Contains("A1", seats);
            Assert.Contains("A2", seats);
            Assert.Contains("B5", seats);
        }

        [Fact]
        public void Hall_TotalSeats_ShouldCalculateCorrectly()
        {
            var hall = new Hall("Hall A", HallType.IMAX, 10, 12);

            Assert.Equal(120, hall.TotalSeats); 
        }
    }
}
// Entities/Reservation.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Entities
{
    public record Reservation
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public int ScreeningId { get; init; }
        public string SeatNumbers { get; init; }
        public decimal TotalPrice { get; init; }
        public ReservationStatus Status { get; init; }
        public DateTime BookedAt { get; init; }
        public DateTime? CancelledAt { get; init; }

        public User User { get; init; }
        public Screening Screening { get; init; }

        public Reservation(int userId, int screeningId, string seatNumbers, decimal totalPrice)
        {
            UserId = userId;
            ScreeningId = screeningId;
            SeatNumbers = seatNumbers;
            TotalPrice = totalPrice;
            Status = ReservationStatus.Active;
            BookedAt = DateTime.Now;
        }

        private Reservation() { }

        // Cancel reservation
        public Reservation Cancel()
        {
            return this with
            {
                Status = ReservationStatus.Cancelled,
                CancelledAt = DateTime.Now
            };
        }

        public Reservation MarkAsCompleted()
        {
            return this with { Status = ReservationStatus.Completed };
        }

        public List<string> GetSeats()
        {
            return SeatNumbers.Split(',').ToList();
        }
    }

    public enum ReservationStatus
    {
        Active,
        Cancelled,
        Completed
    }
}

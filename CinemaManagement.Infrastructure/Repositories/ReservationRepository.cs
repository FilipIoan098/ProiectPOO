// CinemaManagemnt.Infrastracture/Repositories/ReservationRepository.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<ReservationRepository> _logger;

        public ReservationRepository(CinemaDbContext context, ILogger<ReservationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Reservation added: ID {reservation.Id}");
            return reservation;
        }

        public async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            var local = _context.Set<Reservation>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(reservation.Id));

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            // update the entity
            _context.Entry(reservation).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reservation updated: ID {reservation.Id}");
            return reservation;
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Hall)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Movie)
                .Include(r => r.Screening)
                    .ThenInclude(s => s.Hall)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.BookedAt)
                .ToListAsync();
        }

        public async Task<List<Reservation>> GetByScreeningIdAsync(int screeningId)
        {
            return await _context.Reservations
                .Where(r => r.ScreeningId == screeningId && r.Status == ReservationStatus.Active)
                .ToListAsync();
        }

        public async Task<List<string>> GetBookedSeatsForScreeningAsync(int screeningId)
        {
            var reservations = await _context.Reservations
                .Where(r => r.ScreeningId == screeningId && r.Status == ReservationStatus.Active)
                .ToListAsync();

            var bookedSeats = new List<string>();
            foreach (var reservation in reservations)
            {
                bookedSeats.AddRange(reservation.GetSeats());
            }

            return bookedSeats;
        }
    }
}

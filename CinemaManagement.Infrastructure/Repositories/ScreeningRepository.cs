// CinemaManagemnt.Infrastracture/Repositories/ScreeningRepository.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Infrastructure.Repositories
{
    public class ScreeningRepository : IScreeningRepository
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<ScreeningRepository> _logger;

        public ScreeningRepository(CinemaDbContext context, ILogger<ScreeningRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Screening> AddAsync(Screening screening)
        {
            _context.Screenings.Add(screening);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Screening added: ID {screening.Id}");
            return screening;
        }

        public async Task<Screening> UpdateAsync(Screening screening)
        {
            var local = _context.Set<Screening>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(screening.Id));

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Screenings.Update(screening);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Screening updated: ID {screening.Id}");
            return screening;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null) return false;

            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Screening?> GetByIdAsync(int id)
        {
            return await _context.Screenings
                .AsNoTracking() 
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Screening>> GetAllAsync()
        {
            return await _context.Screenings
                .AsNoTracking()  
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ToListAsync();
        }

        public async Task<List<Screening>> GetByMovieIdAsync(int movieId)
        {
            return await _context.Screenings
                .AsNoTracking() 
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.MovieId == movieId)
                .ToListAsync();
        }

        public async Task<List<Screening>> GetUpcomingScreeningsAsync()
        {
            return await _context.Screenings
                .AsNoTracking()  
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.ShowDateTime > DateTime.Now)
                .OrderBy(s => s.ShowDateTime)
                .ToListAsync();
        }
    }
}
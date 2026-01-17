// CinemaManagemnt.Infrastracture/Repositories/MovieRepository.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<MovieRepository> _logger;

        public MovieRepository(CinemaDbContext context, ILogger<MovieRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Movie> AddAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Movie added: ID {movie.Id}");
            return movie;
        }

        public async Task<Movie> UpdateAsync(Movie movie)
        {
            var local = _context.Set<Movie>()
                .Local
                .FirstOrDefault(entry => entry.Id == movie.Id);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Movie updated: ID {movie.Id}");
            return movie;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Movie deleted: ID {id}");
            return true;
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .AsNoTracking() 
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Movie>> GetAllAsync()
        {
            return await _context.Movies
                .AsNoTracking() 
                .ToListAsync();
        }

        public async Task<List<Movie>> GetActiveMoviesAsync()
        {
            return await _context.Movies
                .AsNoTracking()
                .Where(m => m.IsActive)
                .ToListAsync();
        }
    }
}

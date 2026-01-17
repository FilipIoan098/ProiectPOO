// Managers/ScreeningManager.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using CinemaManagement.Core.Exceptions;

namespace CinemaManagement.Core.Managers
{
    public class ScreeningManager
    {
        private readonly IScreeningRepository _repository;
        private readonly ILogger<ScreeningManager> _logger;

        public ScreeningManager(IScreeningRepository repository, ILogger<ScreeningManager> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Screening> CreateScreeningAsync(int movieId, int hallId, DateTime showTime, decimal basePrice, ScreeningType type)
        {
            _logger.LogInformation($"Creating screening for movie {movieId} in hall {hallId}");

            if (showTime < DateTime.Now)
                throw new DataValidationException("Show time cannot be in the past");

            if (basePrice <= 0)
                throw new DataValidationException("Base price must be greater than 0");

            try
            {
                var screening = new Screening(movieId, hallId, showTime, basePrice, type);
                var result = await _repository.AddAsync(screening);

                _logger.LogInformation($"Screening created: ID {result.Id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating screening");
                throw new CinemaException("Failed to create screening", ex);
            }
        }

        public async Task<List<Screening>> GetUpcomingScreeningsAsync() => await _repository.GetUpcomingScreeningsAsync();
        public async Task<List<Screening>> GetScreeningsByMovieAsync(int movieId) => await _repository.GetByMovieIdAsync(movieId);
        public async Task<Screening> GetScreeningByIdAsync(int id) => await _repository.GetByIdAsync(id);
    }
}

// Managers/MovieManagers.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.Core.Managers
{
    public class MovieManager
    {
        private readonly IMovieRepository _repository;
        private readonly ILogger<MovieManager> _logger;

        public MovieManager(IMovieRepository repository, ILogger<MovieManager> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Movie> CreateMovieAsync(string title, string genre, int duration, decimal rating)
        {
            _logger.LogInformation($"Creating new movie: {title}");

            if (string.IsNullOrWhiteSpace(title))
                throw new DataValidationException("Movie title cannot be empty");

            if (duration <= 0)
                throw new DataValidationException("Duration must be greater than 0");

            if (rating < 0 || rating > 5)
                throw new DataValidationException("Rating must be between 0 and 5");

            try
            {
                var movie = new Movie(title, genre, duration, rating);
                var result = await _repository.AddAsync(movie);

                _logger.LogInformation($"Movie created successfully: ID {result.Id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating movie: {title}");
                throw new CinemaException("Failed to create movie", ex);
            }
        }

        public async Task<Movie> UpdateMovieAsync(int id, string title, string genre, int duration, decimal rating)
        {
            _logger.LogInformation($"Updating movie ID: {id}");

            var existingMovie = await _repository.GetByIdAsync(id);
            if (existingMovie == null)
                throw new EntityNotFoundException("Movie", id);

            if (string.IsNullOrWhiteSpace(title))
                throw new ValidationException("Movie title cannot be empty");

            try
            {
                var updatedMovie = existingMovie.Update(title, genre, duration, rating);
                var result = await _repository.UpdateAsync(updatedMovie);

                _logger.LogInformation($"Movie updated successfully: ID {id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating movie ID: {id}");
                throw new CinemaException("Failed to update movie", ex);
            }
        }

        public async Task<Movie> RetireMovieAsync(int id)
        {
            _logger.LogInformation($"Retiring movie ID: {id}");

            var movie = await _repository.GetByIdAsync(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            var retiredMovie = movie.Retire();
            return await _repository.UpdateAsync(retiredMovie);
        }
        public async Task<Movie> ActivateMovieAsync(int id)
        {
            _logger.LogInformation($"Activating movie ID: {id}");

            var movie = await _repository.GetByIdAsync(id);
            if (movie == null)
                throw new EntityNotFoundException("Movie", id);

            var activeMovie = movie.Activate();
            return await _repository.UpdateAsync(activeMovie);
        }

        public async Task<List<Movie>> GetAllMoviesAsync() => await _repository.GetAllAsync();
        public async Task<List<Movie>> GetActiveMoviesAsync() => await _repository.GetActiveMoviesAsync();
        public async Task<Movie> GetMovieByIdAsync(int id) => await _repository.GetByIdAsync(id);
    }
}

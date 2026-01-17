// Managers/HallManager.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.Core.Managers
{
    public class HallManager
    {
        private readonly IHallRepository _repository;
        private readonly ILogger<HallManager> _logger;

        public HallManager(IHallRepository repository, ILogger<HallManager> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Hall> CreateHallAsync(string name, HallType type, int rows, int seatsPerRow)
        {
            _logger.LogInformation($"Creating new hall: {name}");

            if (string.IsNullOrWhiteSpace(name))
                throw new DataValidationException("Hall name cannot be empty");

            if (rows <= 0 || seatsPerRow <= 0)
                throw new DataValidationException("Rows and seats per row must be greater than 0");

            try
            {
                var hall = new Hall(name, type, rows, seatsPerRow);
                var result = await _repository.AddAsync(hall);

                _logger.LogInformation($"Hall created: ID {result.Id}, Total seats: {result.TotalSeats}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating hall: {name}");
                throw new CinemaException("Failed to create hall", ex);
            }
        }

        public async Task<Hall> UpdateHallAsync(
        int id,
        string name,
        HallType type,
        int rows,
        int seatsPerRow)
        {
            _logger.LogInformation($"Updating hall ID: {id}");

            var hall = await _repository.GetByIdAsync(id);
            if (hall == null)
                throw new EntityNotFoundException("Hall", id);

            hall.Update(name, type, rows, seatsPerRow);

            await _repository.UpdateAsync(hall);

            return hall;
        }




        public async Task<List<Hall>> GetAllHallsAsync() => await _repository.GetAllAsync();
        public async Task<Hall?> GetHallByIdAsync(int id) => await _repository.GetByIdAsync(id);
    }
}

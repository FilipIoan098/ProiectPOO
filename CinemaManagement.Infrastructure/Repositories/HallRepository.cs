// CinemaManagemnt.Infrastracture/Repositories/HallRepository.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.Infrastructure.Repositories
{
    public class HallRepository : IHallRepository
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<HallRepository> _logger;

        public HallRepository(CinemaDbContext context, ILogger<HallRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Hall> AddAsync(Hall hall)
        {
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Hall added: ID {hall.Id}");
            return hall;
        }

        public async Task<Hall> UpdateAsync(Hall hall)
        {
            var local = _context.Halls
                .Local
                .FirstOrDefault(entry => entry.Id == hall.Id);

            if (local != null)
            {
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Halls.Update(hall);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Hall updated: ID {hall.Id}");

            return hall;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null) return false;

            _context.Halls.Remove(hall);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Hall deleted: ID {id}");
            return true;
        }

        public async Task<Hall?> GetByIdAsync(int id)
        {
            return await _context.Halls
                .AsNoTracking() 
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<List<Hall>> GetAllAsync()
        {
            return await _context.Halls
                .AsNoTracking() 
                .ToListAsync();
        }
    }
}

// Interfaces/IHallRepository.cs
using CinemaManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Interfaces
{
    public interface IHallRepository
    {
        Task<Hall> AddAsync(Hall hall);
        Task<Hall> UpdateAsync(Hall hall);
        Task<bool> DeleteAsync(int id);
        Task<Hall?> GetByIdAsync(int id);
        Task<List<Hall>> GetAllAsync();
    }
}

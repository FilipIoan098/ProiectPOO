//  Interfaces/IScreeningRepository
using CinemaManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Interfaces
{
    public interface IScreeningRepository
    {
        Task<Screening> AddAsync(Screening screening);
        Task<Screening> UpdateAsync(Screening screening);
        Task<bool> DeleteAsync(int id);
        Task<Screening> GetByIdAsync(int id);
        Task<List<Screening>> GetAllAsync();
        Task<List<Screening>> GetByMovieIdAsync(int movieId);
        Task<List<Screening>> GetUpcomingScreeningsAsync();
    }
}

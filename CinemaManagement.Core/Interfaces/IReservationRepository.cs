//  Interfaces/IReservationRepository.cs
using CinemaManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation> AddAsync(Reservation reservation);
        Task<Reservation> UpdateAsync(Reservation reservation);
        Task<Reservation?> GetByIdAsync(int id);
        Task<List<Reservation>> GetByUserIdAsync(int userId);
        Task<List<Reservation>> GetByScreeningIdAsync(int screeningId);
        Task<List<string>> GetBookedSeatsForScreeningAsync(int screeningId);
    }
}

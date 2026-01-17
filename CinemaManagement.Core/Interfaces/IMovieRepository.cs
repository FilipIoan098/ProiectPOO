// Interfaces/IMovieRepository.cs
using CinemaManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie> AddAsync(Movie movie);
        Task<Movie> UpdateAsync(Movie movie);
        Task<bool> DeleteAsync(int id);
        Task<Movie?> GetByIdAsync(int id);
        Task<List<Movie>> GetAllAsync();
        Task<List<Movie>> GetActiveMoviesAsync();
    }
}

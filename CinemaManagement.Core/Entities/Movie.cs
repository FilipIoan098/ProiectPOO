// Entities/Movie.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Entities
{
    public record Movie
    {

        public int Id { get; init; }
        public string Title { get; init; }
        public string Genre { get; init; }
        public int DurationMinutes { get; init; }
        public decimal Rating { get; init; } 
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }

        public Movie(string title, string genre, int durationMinutes, decimal rating)
        {
            Title = title;
            Genre = genre;
            DurationMinutes = durationMinutes;
            Rating = rating;
            IsActive = true;
            CreatedAt = DateTime.Now;
        }

        private Movie() { }

        public Movie Retire()
        {
            return this with { IsActive = false };
        }

        public Movie Activate()
        {
            return this with { IsActive = true };
        }

        public Movie Update(string title, string genre, int durationMinutes, decimal rating)
        {
            return this with
            {
                Title = title,
                Genre = genre,
                DurationMinutes = durationMinutes,
                Rating = rating
            };
        }
    }
}

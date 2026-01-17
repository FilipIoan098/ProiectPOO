// Entities/Screenin.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Entities
{
    public record Screening
    {
        public int Id { get; init; }
        public int MovieId { get; init; }
        public int HallId { get; init; }
        public DateTime ShowDateTime { get; init; }
        public decimal BasePrice { get; init; }
        public ScreeningType Type { get; init; } 
        public DateTime CreatedAt { get; init; }

        public Movie Movie { get; init; }
        public Hall Hall { get; init; }

        public Screening(int movieId, int hallId, DateTime showDateTime, decimal basePrice, ScreeningType type)
        {
            MovieId = movieId;
            HallId = hallId;
            ShowDateTime = showDateTime;
            BasePrice = basePrice;
            Type = type;
            CreatedAt = DateTime.Now;
        }

        private Screening() { }

        public decimal GetFinalPrice()
        {
            return Type switch
            {
                ScreeningType.Matinee => BasePrice * 0.8m, 
                ScreeningType.Weekend => BasePrice * 1.2m, 
                ScreeningType.Evening => BasePrice,
                _ => BasePrice
            };
        }

        public Screening Update(DateTime showDateTime, decimal basePrice, ScreeningType type)
        {
            return this with
            {
                ShowDateTime = showDateTime,
                BasePrice = basePrice,
                Type = type
            };
        }
    }

    public enum ScreeningType
    {
        Matinee,    
        Evening,    
        Weekend     
    }
}

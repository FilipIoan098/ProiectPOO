// Entities/Hall.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Entities
{
    public record Hall
    {
        public int Id { get; init; }
        public string Name { get; init; } 
        public HallType Type { get; init; } 
        public int TotalSeats { get; init; }
        public int TotalRows { get; init; }
        public int SeatsPerRow { get; init; }
        public DateTime CreatedAt { get; init; }

        public Hall(string name, HallType type, int rows, int seatsPerRow)
        {
            Name = name;
            Type = type;
            TotalRows = rows;
            SeatsPerRow = seatsPerRow;
            TotalSeats = rows * seatsPerRow;
            CreatedAt = DateTime.Now;
        }

        private Hall() { }

        public Hall Update(string name, HallType type, int rows, int seatsPerRow)
        {
            return this with
            {
                Name = name,
                Type = type,
                TotalRows = rows,
                SeatsPerRow = seatsPerRow,
                TotalSeats = rows * seatsPerRow
            };
        }
    }

    public enum HallType
    {
        Standard,
        IMAX,
        ThreeD
    }
}

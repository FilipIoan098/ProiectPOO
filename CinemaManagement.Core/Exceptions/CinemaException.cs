// Entities/CinemaException.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Exceptions
{
    public class CinemaException : Exception
    {
        public CinemaException(string message) : base(message) { }
        public CinemaException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class EntityNotFoundException : CinemaException
    {
        public EntityNotFoundException(string entityName, int id)
            : base($"{entityName} with ID {id} was not found.") { }
    }

    public class DataValidationException : CinemaException
    {
        public DataValidationException(string message) : base(message) { }
    }

    public class SeatsUnavailableException : CinemaException
    {
        public SeatsUnavailableException(string seats)
            : base($"The following seats are already booked: {seats}") { }
    }

    public class AuthenticationException : CinemaException
    {
        public AuthenticationException(string message) : base(message) { }
    }

    public class UnauthorizedException : CinemaException
    {
        public UnauthorizedException(string message)
            : base(message) { }
    }

    public class DataFileException : CinemaException
    {
        public DataFileException(string message) : base(message) { }
        public DataFileException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class ReservationCancellationException : CinemaException
    {
        public ReservationCancellationException(string message) : base(message) { }
    }
}

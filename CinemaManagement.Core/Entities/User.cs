//  Entities/User.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Core.Entities
{
    public record User
    {
        public int Id { get; init; }
        public string Username { get; init; }
        public string PasswordHash { get; init; }
        public UserRole Role { get; init; }
        public DateTime CreatedAt { get; init; }

        public User(string username, string passwordHash, UserRole role)
        {
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.Now;
        }

        private User() { }
    }

    public enum UserRole
    {
        Administrator,
        Spectator
    }
}

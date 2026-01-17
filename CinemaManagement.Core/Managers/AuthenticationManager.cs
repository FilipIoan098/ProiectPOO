// Managers/AuthenticationManager.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace CinemaManagement.Core.Managers
{
    public class AuthenticationManager
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<AuthenticationManager> _logger;

        public AuthenticationManager(IUserRepository repository, ILogger<AuthenticationManager> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<User> RegisterAsync(string username, string password, UserRole role)
        {
            _logger.LogInformation($"Registering new user: {username}");

            if (string.IsNullOrWhiteSpace(username))
                throw new DataValidationException("Username cannot be empty");

            if (string.IsNullOrWhiteSpace(password))
                throw new DataValidationException("Password cannot be empty");

            if (password.Length < 4)
                throw new DataValidationException("Password must be at least 4 characters");

            var existingUser = await _repository.GetByUsernameAsync(username);
            if (existingUser != null)
                throw new DataValidationException("Username already exists");

            try
            {
                string passwordHash = HashPassword(password);

                var user = new User(username, passwordHash, role);
                var result = await _repository.AddAsync(user);

                _logger.LogInformation($"User registered successfully: {username} with role {role}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user: {username}");
                throw new CinemaException("Failed to register user", ex);
            }
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            _logger.LogInformation($"Login attempt for user: {username}");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new AuthenticationException("Username and password are required");

            try
            {
                var user = await _repository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning($"Login failed: User not found - {username}");
                    throw new AuthenticationException("Invalid username or password");
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed: Invalid password for user - {username}");
                    throw new AuthenticationException("Invalid username or password");
                }

                _logger.LogInformation($"User logged in successfully: {username}");
                return user;
            }
            catch (AuthenticationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user: {username}");
                throw new CinemaException("Login failed", ex);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            string passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
        public async Task CreateDefaultAdminAsync()
        {
            try
            {
                var existingAdmin = await _repository.GetByUsernameAsync("admin");
                if (existingAdmin == null)
                {
                    await RegisterAsync("admin", "admin123", UserRole.Administrator);
                    _logger.LogInformation("Default admin account created");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default admin");
            }
        }
    }
}

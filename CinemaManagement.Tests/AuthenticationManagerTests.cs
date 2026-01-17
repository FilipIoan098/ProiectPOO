// CinemaManagement.Tests/AuthenticationManagerTests.cs

using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Exceptions;
using CinemaManagement.Core.Interfaces;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CinemaManagement.Tests
{
    public class AuthenticationManagerTests
    {
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<ILogger<AuthenticationManager>> _mockLogger;
        private readonly AuthenticationManager _authManager;

        public AuthenticationManagerTests()
        {
            _mockRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<AuthenticationManager>>();
            _authManager = new AuthenticationManager(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldSucceed()
        {
            var username = "testuser";
            var password = "testpass123";
            var role = UserRole.Spectator;

            _mockRepository.Setup(r => r.GetByUsernameAsync(username))
                          .ReturnsAsync((User)null);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                          .ReturnsAsync((User u) => new User(u.Username, u.PasswordHash, u.Role) { Id = 1 });

            var result = await _authManager.RegisterAsync(username, password, role);

            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(role, result.Role);
            Assert.NotNull(result.PasswordHash);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            var username = "testuser";
            var password = "testpass123";

            _mockRepository.Setup(r => r.GetByUsernameAsync(username))
                          .ReturnsAsync((User)null);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                          .ReturnsAsync((User u) => u);

            var registeredUser = await _authManager.RegisterAsync(username, password, UserRole.Spectator);

            _mockRepository.Setup(r => r.GetByUsernameAsync(username))
                          .ReturnsAsync(registeredUser);

            var result = await _authManager.LoginAsync(username, password);

            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldThrowAuthenticationException()
        {
            var username = "testuser";
            var correctPassword = "correctpass";
            var wrongPassword = "wrongpass";

            var user = new User(username, "somehash", UserRole.Spectator) { Id = 1 };
            _mockRepository.Setup(r => r.GetByUsernameAsync(username))
                          .ReturnsAsync(user);

            await Assert.ThrowsAsync<AuthenticationException>(
                async () => await _authManager.LoginAsync(username, wrongPassword)
            );
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ShouldThrowAuthenticationException()
        {
            var username = "nonexistent";
            var password = "anypass";

            _mockRepository.Setup(r => r.GetByUsernameAsync(username))
                          .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<AuthenticationException>(
                async () => await _authManager.LoginAsync(username, password)
            );
        }
    }
}
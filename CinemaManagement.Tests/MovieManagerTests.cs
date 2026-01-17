// CinemaManagement.Tests/MovieManagerTests.cs

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
    public class MovieManagerTests
    {
        private readonly Mock<IMovieRepository> _mockRepository;
        private readonly Mock<ILogger<MovieManager>> _mockLogger;
        private readonly MovieManager _movieManager;

        public MovieManagerTests()
        {
            _mockRepository = new Mock<IMovieRepository>();
            _mockLogger = new Mock<ILogger<MovieManager>>();
            _movieManager = new MovieManager(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateMovie_WithValidData_ShouldSucceed()
        {
            var title = "Inception";
            var genre = "Sci-Fi";
            var duration = 148;
            var rating = 4.8m;

            var expectedMovie = new Movie(title, genre, duration, rating) { Id = 1 };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Movie>()))
                          .ReturnsAsync(expectedMovie);

            var result = await _movieManager.CreateMovieAsync(title, genre, duration, rating);

            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(genre, result.Genre);
            Assert.Equal(duration, result.DurationMinutes);
            Assert.Equal(rating, result.Rating);
            Assert.True(result.IsActive);
        }


        [Fact]
        public async Task RetireMovie_ExistingMovie_ShouldSetIsActiveToFalse()
        {
            var movieId = 1;
            var activeMovie = new Movie("Test Movie", "Action", 120, 4.5m) { Id = movieId };

            _mockRepository.Setup(r => r.GetByIdAsync(movieId))
                          .ReturnsAsync(activeMovie);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Movie>()))
                          .ReturnsAsync((Movie m) => m);

            var result = await _movieManager.RetireMovieAsync(movieId);

            Assert.NotNull(result);
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task RetireMovie_NonExistentMovie_ShouldThrowEntityNotFoundException()
        {
            var movieId = 999;
            _mockRepository.Setup(r => r.GetByIdAsync(movieId))
                          .ReturnsAsync((Movie)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                async () => await _movieManager.RetireMovieAsync(movieId)
            );
        }

        [Fact]
        public async Task UpdateMovie_WithValidData_ShouldUpdateSuccessfully()
        {
            var movieId = 1;
            var existingMovie = new Movie("Old Title", "Drama", 90, 3.0m) { Id = movieId };
            var newTitle = "New Title";
            var newGenre = "Comedy";
            var newDuration = 120;
            var newRating = 4.5m;

            _mockRepository.Setup(r => r.GetByIdAsync(movieId))
                          .ReturnsAsync(existingMovie);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Movie>()))
                          .ReturnsAsync((Movie m) => m);

            var result = await _movieManager.UpdateMovieAsync(movieId, newTitle, newGenre, newDuration, newRating);

            Assert.NotNull(result);
            Assert.Equal(newTitle, result.Title);
            Assert.Equal(newGenre, result.Genre);
            Assert.Equal(newDuration, result.DurationMinutes);
            Assert.Equal(newRating, result.Rating);
        }
    }
}

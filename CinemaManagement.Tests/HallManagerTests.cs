// CinemaManagement.Tests/HallManagerTests.cs

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
    public class HallManagerTests
    {
        private readonly Mock<IHallRepository> _mockRepository;
        private readonly Mock<ILogger<HallManager>> _mockLogger;
        private readonly HallManager _hallManager;

        public HallManagerTests()
        {
            _mockRepository = new Mock<IHallRepository>();
            _mockLogger = new Mock<ILogger<HallManager>>();
            _hallManager = new HallManager(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateHall_WithValidData_ShouldSucceed()
        {
            var name = "Hall A";
            var type = HallType.IMAX;
            var rows = 10;
            var seatsPerRow = 12;

            var expectedHall = new Hall(name, type, rows, seatsPerRow) { Id = 1 };
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Hall>()))
                          .ReturnsAsync(expectedHall);

            var result = await _hallManager.CreateHallAsync(name, type, rows, seatsPerRow);

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(type, result.Type);
            Assert.Equal(rows, result.TotalRows);
            Assert.Equal(seatsPerRow, result.SeatsPerRow);
            Assert.Equal(120, result.TotalSeats);
        }
    }
}
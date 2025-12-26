using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Infrastructure.Repositories;

namespace ToDoAppTests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedUsers()
        {
            // Arrange
            var logger = new Mock<ILogger<FileUserRepository>>();
            var fileStorage = new Mock<IFileStorage>();

            var filePath = "users.json";

            var expectedDtos = new List<UserDto>
            {
                new() {
                    Id = Guid.Empty,
                    Username = "Grigorii",
                    PasswordHash = "1111",
                }
            };

            fileStorage
                .Setup(s => s.LoadAsync<List<UserDto>>(filePath))
                .ReturnsAsync(expectedDtos);

            var repo = new FileUserRepository(
                logger.Object,
                fileStorage.Object,
                filePath);

            // Act
            var users = await repo.GetAllAsync();

            // Assert
            users.Should().NotBeNull();
            users.Should().HaveCount(1);

            var user = users[0];
            user.Id.Should().Be(Guid.Empty);
            user.Username.Should().Be("Grigorii");

            fileStorage.Verify(x => x.LoadAsync<List<UserDto>>(filePath), Times.Once);
        }
    }
}

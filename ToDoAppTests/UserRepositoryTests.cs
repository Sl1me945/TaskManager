using FluentAssertions;
using Moq;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Enums;
using ToDoApp.Infrastructure.Repositories;

namespace ToDoAppTests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedUsers()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var fileStorage = new Mock<IFileStorage>();

            var filePath = "users.json";

            var expectedDtos = new List<UserDto>
            {
                new() {
                    Username = "Grigorii",
                    PasswordHash = "1111",
                    Tasks =
                    [
                        new() {
                            Id = Guid.NewGuid(),
                            Title = "Test Task",
                            Type = TaskType.Simple,
                            IsCompleted = false,
                            CreatedAt = DateTime.UtcNow
                        }
                    ]
                }
            };

            fileStorage
                .Setup(s => s.LoadAsync<IReadOnlyList<UserDto>>(filePath))
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
            user.Username.Should().Be("Grigorii");
            user.Tasks.Should().HaveCount(1);
            user.Tasks[0].Title.Should().Be("Test Task");

            fileStorage.Verify(x => x.LoadAsync<IReadOnlyList<UserDto>>(filePath), Times.Once);
        }
    }
}

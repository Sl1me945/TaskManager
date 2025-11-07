using FluentAssertions;
using Moq;
using ToDoApp.DTOs;
using ToDoApp.Interfaces;
using ToDoApp.Models.Tasks;
using ToDoApp.Services;

namespace ToDoAppTests
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturnUsersFromStorage()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            var fileStorage = new Mock<IFileStorage>();

            var expectedUsers = new List<UserDto>
        {
            new UserDto
            {
                Username = "Grigorii",
                PasswordHash = "1111",
                Tasks = new List<TaskDto>
                {
                    new TaskDto { Title = "Test Task", IsCompleted = false }
                }
            }
        };

            // When JsonUserRepository loads users, it will call LoadAsync<IReadOnlyList<UserDto>>(...)
            fileStorage.Setup(f => f.LoadAsync<IReadOnlyList<UserDto>>("data/users/users.json"))
                       .ReturnsAsync(expectedUsers);

            IUserRepository userRepository = new JsonUserRepository(logger.Object, fileStorage.Object);

            // Act
            var actualUsers = await userRepository.GetAllAsync();

            // Assert
            actualUsers.Should().NotBeNull();
            actualUsers.Should().HaveCount(1);
            actualUsers[0].Username.Should().Be("Grigorii");

            // Verify that LoadAsync was indeed called
            fileStorage.Verify(f => f.LoadAsync<IReadOnlyList<UserDto>>("data/users/users.json"), Times.Once);
        }
    }
}

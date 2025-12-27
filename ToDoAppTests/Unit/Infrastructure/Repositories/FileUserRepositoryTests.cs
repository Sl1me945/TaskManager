using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;
using ToDoApp.Infrastructure.Repositories;

namespace ToDoAppTests.Unit.Infrastructure.Repositories
{
    public class FileUserRepositoryTests
    {
        private readonly Mock<ILogger<FileUserRepository>> _loggerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private const string FilePath = "test-users.json";

        public FileUserRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<FileUserRepository>>();
            _fileStorageMock = new Mock<IFileStorage>();
        }

        private FileUserRepository CreateRepository()
            => new(_loggerMock.Object, _fileStorageMock.Object, FilePath);


        [Fact]
        public async Task GetAllAsync_WhenUserExist_ReturnsAllUsers()
        {
            // Arrange
            var expectedDtos = new List<UserDto>
            {
                new() { Id = Guid.NewGuid(), Username = "user1", PasswordHash = "hash1" },
                new() { Id = Guid.NewGuid(), Username = "user2", PasswordHash = "hash2" }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            result[0].Username.Should().Be("user1");
            result[1].Username.Should().Be("user2");

            _fileStorageMock.Verify(x => x.LoadAsync<List<UserDto>>(FilePath), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenFileEmpty_ReturnsEmptyList()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(new List<UserDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_WhenFileNotFound_ReturnsEmptyList()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ThrowsAsync(new FileNotFoundException());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userDtos = new List<UserDto>
            {
                new() { Id = userId, Username = "testuser", PasswordHash = "hash" }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(userDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(new List<UserDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            const string testUsername = "john";
            var userDtos = new List<UserDto>
            {
                new() { Id = Guid.NewGuid(), Username = testUsername, PasswordHash = "hash" }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(userDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUsernameAsync(testUsername);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(testUsername);
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(new List<UserDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUsernameAsync("jack");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserAndSave() 
        {
            // Arrange
            var existingUsers = new List<UserDto>();
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(existingUsers);

            var newUser = new User { Id = Guid.NewGuid(), Username = "newuser", PasswordHash = "hash" };
            var repository = CreateRepository();

            // Act
            await repository.AddAsync(newUser);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<UserDto>>(list => list.Count() == 1 && list[0].Username == "newuser")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserExists_ShouldUpdateAndSave()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUsers = new List<UserDto>()
            {
                new() {Id = userId, Username = "oldname", PasswordHash = "oldhash"}
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(existingUsers);

            var updatedUser = new User { Id = userId, Username = "newname", PasswordHash = "newhash" };
            var repository = CreateRepository();

            // Act
            await repository.UpdateAsync(updatedUser);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<UserDto>>(list => list.Count() == 1 && 
                    list[0].Username == "newname" &&
                    list[0].PasswordHash == "newhash")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<UserDto>>(FilePath))
                .ReturnsAsync(new List<UserDto>());

            var user = new User { Id = Guid.NewGuid(), Username = "newname", PasswordHash = "newhash" };
            var repository = CreateRepository();

            // Act
            var act = async () => await repository.UpdateAsync(user);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
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

        #region Helper Methods

        private FileUserRepository CreateRepository()
            => new(_loggerMock.Object, _fileStorageMock.Object, FilePath);

        private FileUserDto CreateUserDto(string username = "testuser", Guid? id = null)
            => new()
            {
                Id = id ?? Guid.NewGuid(),
                Username = username,
                PasswordHash = $"hash_for_{username}"
            };

        private void VerifyLogWarning(string expectedMessage)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenUsersExist_ReturnsAllUsers()
        {
            // Arrange
            var expectedDtos = new List<FileUserDto>
            {
                CreateUserDto("testuser1"),
                CreateUserDto("testuser2")
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Username.Should().Be("testuser1");
            result[1].Username.Should().Be("testuser2");

            _fileStorageMock.Verify(x => x.LoadAsync<List<FileUserDto>>(FilePath), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenFileEmpty_ReturnsEmptyList()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(new List<FileUserDto>());

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
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ThrowsAsync(new FileNotFoundException());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            VerifyLogWarning("not found");
        }

        [Fact]
        public async Task GetAllAsync_WithLargeDataset_ReturnsAllUsersEfficiently()
        {
            // Arrange
            var largeDtoList = Enumerable.Range(0, 10000)
                .Select(i => CreateUserDto($"testuser{i}"))
                .ToList();
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(largeDtoList);

            var repository = CreateRepository();

            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await repository.GetAllAsync();
            stopwatch.Stop();

            // Assert
            result.Should().HaveCount(10000);
            stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task GetAllAsync_WhenDtoCorrupted_ReturnsEmptyList()
        {
            // Arrange
            var corruptedDtos = new List<FileUserDto>
            {
                CreateUserDto(null!)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(corruptedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userDtos = new List<FileUserDto>
            {
                CreateUserDto(id: id)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(userDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(new List<FileUserDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByUsernameAsync Tests

        [Fact]
        public async Task GetByUsernameAsync_WhenUserExists_ReturnsUser()
        {
            // Arrange
            var userDtos = new List<FileUserDto>
            {
                CreateUserDto()
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(userDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUsernameAsync("testuser");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByUsernameAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(new List<FileUserDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUsernameAsync("user");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidUser_AddsUserAndSaves()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(new List<FileUserDto>());

            var newUser = new User("testuser", "hash_for_testuser");
            var repository = CreateRepository();

            // Act
            await repository.AddAsync(newUser);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<FileUserDto>>(list => list.Count == 1 && list[0].Username == "testuser")),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenStorageThrowsException_PropagatesException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.SaveAsync(FilePath, It.IsAny<List<FileUserDto>>()))
                .ThrowsAsync(new IOException("Disk full"));

            var user = new User("testuser", "hash_for_testuser");
            var repository = CreateRepository();

            // Act
            var act = async () => await repository.AddAsync(user);

            // Assert
            await act.Should().ThrowAsync<IOException>();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenUserExists_UpdatesUserAndSaves()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingUsers = new List<FileUserDto>
            {
                CreateUserDto(id: id)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(existingUsers);

            var updatedUser = new User(id, "newuser", "hash_for_newuser");
            var repository = CreateRepository();

            // Act
            await repository.UpdateAsync(updatedUser);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<FileUserDto>>(list => list.Count == 1 &&
                    list[0].Username == "newuser" &&
                    list[0].PasswordHash == "hash_for_newuser")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenUserNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileUserDto>>(FilePath))
                .ReturnsAsync(new List<FileUserDto>());

            var user = new User(Guid.NewGuid(), "testuser", "user");
            var repository = CreateRepository();

            // Act
            var act = async () => await repository.UpdateAsync(user);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        #endregion
    }
}
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;
using ToDoApp.Infrastructure.Repositories;

namespace ToDoAppTests.Unit.Infrastructure.Repositories
{
    public class FileTaskRepositoryTests
    {
        private readonly Mock<ILogger<FileTaskRepository>> _loggerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private const string FilePath = "test-tasks.json";

        public FileTaskRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<FileTaskRepository>>();
            _fileStorageMock = new Mock<IFileStorage>();
        }

        #region Helper Methods

        private FileTaskRepository CreateRepository()
            => new(_loggerMock.Object, _fileStorageMock.Object, FilePath);

        private FileTaskDto CreateSimpleTaskDto(string title = "task1", Guid? id = null, Guid? userId = null)
            => new()
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Title = title,
                Description = $"description for {title}",
                CreatedAt = _fixedDateTime,
                DueDate = _fixedDateTime.AddHours(1),
                IsCompleted = false,
                Priority = Priority.Low,
                Type = TaskType.Simple
            };

        private FileTaskDto CreateWorkTaskDto(string title, string projectName)
        {
            var dto = CreateSimpleTaskDto(title);
            dto.Type = TaskType.Work;
            dto.ProjectName = projectName;
            return dto;
        }

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
        public async Task GetAllAsync_WhenTasksExist_ReturnsAllTasks()
        {
            // Arrange
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto("task1"),
                CreateWorkTaskDto("task2", "project name for task2")
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            result[0].Should().BeOfType<SimpleTask>();
            result[0].Title.Should().Be("task1");
            result[0].Description.Should().Be("description for task1");

            result[1].Should().BeOfType<WorkTask>()
                .Which.ProjectName.Should().Be("project name for task2");


            _fileStorageMock.Verify(x => x.LoadAsync<List<FileTaskDto>>(FilePath), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenFileEmpty_ReturnsEmptyList()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(new List<FileTaskDto>());

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
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ThrowsAsync(new FileNotFoundException());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
            VerifyLogWarning("not found");
        }

        [Fact]
        public async Task GetAllAsync_WithLargeDataset_ReturnsAllTasksEfficiently()
        {
            // Arrange
            var largeDtoList = Enumerable.Range(0, 10000)
                .Select(i => CreateSimpleTaskDto($"task{i}"))
                .ToList();
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
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
            var corruptedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto(null!)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(corruptedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByUserIdAsync Tests

        [Fact]
        public async Task GetByUserIdAsync_WhenTaskExists_ReturnsTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto("task1", userId: userId)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(1);
            result[0].UserId.Should().Be(userId);
            result[0].Title.Should().Be("task1");
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenTaskNotFound_ReturnsEmptyList()
        {
            // Arrange
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto()
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTaskExists_ReturnsTask()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto(id: id)
            };
            _fileStorageMock
                .Setup(x => x.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTaskNotFound_ReturnsNull()
        {
            // Arrange
            _fileStorageMock
                .Setup(x => x.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(new List<FileTaskDto>());

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_WithValidTask_AddsTaskAndSaves()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(new List<FileTaskDto>());

            var newTask = new SimpleTask("title", "description", _fixedDateTime.AddHours(1), Priority.Low);
            var repository = CreateRepository();

            // Act
            await repository.AddAsync(newTask);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<FileTaskDto>>(list => list.Count == 1 && list[0].Title == "title")),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenStorageThrowsException_PropagatesException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.SaveAsync(FilePath, It.IsAny<List<FileTaskDto>>()))
                .ThrowsAsync(new IOException("Disk full"));

            var task = new SimpleTask("title", "description", _fixedDateTime.AddHours(1), Priority.Low);
            var repository = CreateRepository();

            // Act
            var act = async () => await repository.AddAsync(task);

            // Assert
            await act.Should().ThrowAsync<IOException>();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenTaskExists_UpdatesTaskAndSaves()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto("oldtitle", id, userId)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var updatedTask = new SimpleTask(
                id,
                _fixedDateTime,
                "newtitle",
                "description for newtitle",
                _fixedDateTime.AddHours(1),
                Priority.Medium,
                false,
                userId
            );

            var repository = CreateRepository();

            // Act
            await repository.UpdateAsync(updatedTask);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<FileTaskDto>>(list => list.Count == 1 &&
                    list[0].Title == "newtitle" &&
                    list[0].Description == "description for newtitle" &&
                    list[0].Priority == Priority.Medium)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(new List<FileTaskDto>());

            var task = new SimpleTask("title", "description", _fixedDateTime.AddHours(1), Priority.Low);
            var repository = CreateRepository();

            // Act
            var act = async () => await repository.UpdateAsync(task);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTaskExists_DeletesTaskAndSaves()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expectedDtos = new List<FileTaskDto>
            {
                CreateSimpleTaskDto(id: id)
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            await repository.DeleteAsync(id);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<FileTaskDto>>(list => list.Count == 0)),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<FileTaskDto>>(FilePath))
                .ReturnsAsync(new List<FileTaskDto>());

            var repository = CreateRepository();

            // Act
            var act = async () => await repository.DeleteAsync(Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        #endregion
    }
}
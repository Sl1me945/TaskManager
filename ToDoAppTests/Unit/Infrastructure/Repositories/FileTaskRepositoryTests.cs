using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Application.DTOs;
using ToDoApp.Application.Interfaces;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;
using ToDoApp.Infrastructure.Repositories;

namespace ToDoAppTests.Unit.Infrastructure.Repositories
{
    public class FileTaskRepositoryTests
    {
        private readonly Mock<ILogger<FileTaskRepository>> _loggerMock;
        private readonly Mock<IFileStorage> _fileStorageMock;
        private const string FilePath = "test-tasks.json";

        public FileTaskRepositoryTests()
        {
            _loggerMock = new Mock<ILogger<FileTaskRepository>>();
            _fileStorageMock = new Mock<IFileStorage>();
        }

        private FileTaskRepository CreateRepository()
            => new(_loggerMock.Object, _fileStorageMock.Object, FilePath);


        [Fact]
        public async Task GetAllAsync_WhenTaskExist_ReturnsAllTasks()
        {
            // Arrange
            var expectedDtos = new List<TaskDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Title = "task1",
                    Description = "description1",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Low,
                    Type = TaskType.Simple
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Title = "task2",
                    Description = "description2",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Medium,
                    Type = TaskType.Work,
                    ProjectName = "projectname"
                },
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            result[0].Title.Should().Be("task1");
            result[0].Description.Should().Be("description1");
            result[0].Should().BeOfType<SimpleTask>();

            result[1].Title.Should().Be("task2");
            result[1].Should().BeOfType<WorkTask>(); 
            var workTask = result[1] as WorkTask;
            workTask.Should().NotBeNull();
            workTask!.ProjectName.Should().Be("projectname");

            _fileStorageMock.Verify(x => x.LoadAsync<List<TaskDto>>(FilePath), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenFileEmpty_ReturnsEmptyList()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(new List<TaskDto>());

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
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
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
        public async Task GetByUserIdAsync_WhenTaskExists_ReturnsTask()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedDtos = new List<TaskDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "task1",
                    Description = "description1",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Low,
                    Type = TaskType.Simple
                }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
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
            var expectedDtos = new List<TaskDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Title = "task1",
                    Description = "description1",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Low,
                    Type = TaskType.Simple
                }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(expectedDtos);

            var repository = CreateRepository();

            // Act
            var result = await repository.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_ShouldAddTaskAndSave()
        {
            // Arrange
            var existingTasks = new List<TaskDto>();
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(existingTasks);

            var newTask = new SimpleTask(Guid.NewGuid(), DateTime.UtcNow);
            newTask.UserId = Guid.NewGuid();
            newTask.Title = "title";
            newTask.Description = "description";
            newTask.DueDate = DateTime.UtcNow.AddHours(1);
            newTask.Priority = Priority.Low;

            var repository = CreateRepository();

            // Act
            await repository.AddAsync(newTask);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<TaskDto>>(list => list.Count() == 1 && list[0].Title == "title")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskExists_ShouldUpdateAndSave()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var existingTasks = new List<TaskDto>
            {
                new()
                {
                    Id = id,
                    UserId = userId,
                    Title = "oldtitle",
                    Description = "olddescription",
                    CreatedAt = createdAt,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Low,
                    Type = TaskType.Simple
                }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(existingTasks);

            var updatedTask = new SimpleTask(id, createdAt);
            updatedTask.UserId = userId;
            updatedTask.Title = "newtitle";
            updatedTask.Description = "newdescription";
            updatedTask.DueDate = DateTime.UtcNow.AddHours(1);
            updatedTask.Priority = Priority.Medium;

            var repository = CreateRepository();

            // Act
            await repository.UpdateAsync(updatedTask);

            // Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<TaskDto>>(list => list.Count() == 1 &&
                    list[0].Title == "newtitle" &&
                    list[0].Description == "newdescription" &&
                    list[0].Priority == Priority.Medium)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskNotFound_ThrowsException()
        {
            // Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(new List<TaskDto>());

            var task = new SimpleTask(Guid.NewGuid(), DateTime.UtcNow);
            task.UserId = Guid.NewGuid();
            task.Title = "title";
            task.Description = "description";
            task.DueDate = DateTime.UtcNow.AddHours(1);
            task.Priority = Priority.Medium;

            var repository = CreateRepository();

            // Act
            var act = async () => await repository.UpdateAsync(task);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskExist_ShouldDeleteAndSave()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingTasks = new List<TaskDto>
            {
                new()
                {
                    Id = id,
                    UserId = Guid.NewGuid(),
                    Title = "oldtitle",
                    Description = "olddescription",
                    CreatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddHours(1),
                    IsCompleted = false,
                    Priority = Priority.Low,
                    Type = TaskType.Simple
                }
            };
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(existingTasks);

            var repository = CreateRepository();

            //Act
            await repository.DeleteAsync(id);

            //Assert
            _fileStorageMock.Verify(
                x => x.SaveAsync(
                    FilePath,
                    It.Is<List<TaskDto>>(list => list.Count() == 0)
                ));
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotFound_ThrowsException()
        {
            //Arrange
            _fileStorageMock
                .Setup(s => s.LoadAsync<List<TaskDto>>(FilePath))
                .ReturnsAsync(new List<TaskDto>());

            var repository = CreateRepository();

            //Act
            var act = async () => await repository.DeleteAsync(Guid.NewGuid());

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*not found*");
        }
    }
}

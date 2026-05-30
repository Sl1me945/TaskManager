using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.Services;
using ToDoApp.Domain.Entities;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;
using ToDoApp.Application.Interfaces;

namespace ToDoAppTests.Unit.Application.Services
{
    public class TaskManagerTests
    {
        private readonly Mock<ILogger<TaskManager>> _loggerMock;
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly User _testUser;

        public TaskManagerTests()
        {
            _loggerMock = new Mock<ILogger<TaskManager>>();
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _testUser = new User("testuser", "hash");
        }

        #region Helper Methods

        private TaskManager CreateService()
        {
            return new TaskManager(
                _loggerMock.Object,
                _taskRepositoryMock.Object,
                _userRepositoryMock.Object
            );
        }

        private SimpleTask CreateSimpleTask(string title = "Test Task", Guid? userId = null)
        {
            var task = new SimpleTask(title, "Description", _fixedDateTime.AddDays(1), Priority.Low);
            if (userId.HasValue)
            {
                task.AssignToUser(userId.Value);
            }
            return task;
        }

        private void VerifyLogInformation(string expectedMessage)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion

        #region AddTaskAsync Tests

        [Fact]
        public async Task AddTaskAsync_WithValidTask_AddsTaskSuccessfully()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync(_testUser);

            var service = CreateService();

            // Act
            await service.AddTaskAsync(_userId, task);

            // Assert
            _taskRepositoryMock.Verify(x => x.AddAsync(task), Times.Once);
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddTaskAsync_WithValidTask_AssignsTaskToUser()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync(_testUser);

            var service = CreateService();

            // Act
            await service.AddTaskAsync(_userId, task);

            // Assert
            task.UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task AddTaskAsync_WithNonExistentUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            var act = async () => await service.AddTaskAsync(_userId, task);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*User not found*");
        }

        [Fact]
        public async Task AddTaskAsync_WithNonExistentUser_DoesNotAddTask()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            var act = async () => await service.AddTaskAsync(_userId, task);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<BaseTask>()), Times.Never);
        }

        [Fact]
        public async Task AddTaskAsync_WithValidTask_LogsTaskAddition()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync(_testUser);

            var service = CreateService();

            // Act
            await service.AddTaskAsync(_userId, task);

            // Assert
            VerifyLogInformation("Task");
            VerifyLogInformation("added");
        }

        [Fact]
        public async Task AddTaskAsync_WithValidTask_SavesChanges()
        {
            // Arrange
            var task = CreateSimpleTask();

            _userRepositoryMock
                .Setup(x => x.GetByIdAsync(_userId))
                .ReturnsAsync(_testUser);

            var service = CreateService();

            // Act
            await service.AddTaskAsync(_userId, task);

            // Assert
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region UpdateTaskAsync Tests

        [Fact]
        public async Task UpdateTaskAsync_WithOwnTask_UpdatesTaskSuccessfully()
        {
            // Arrange
            var task = CreateSimpleTask(userId: _userId);

            var service = CreateService();

            // Act
            await service.UpdateTaskAsync(_userId, task);

            // Assert
            _taskRepositoryMock.Verify(x => x.UpdateAsync(task), Times.Once);
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithOtherUsersTask_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            var service = CreateService();

            // Act
            var act = async () => await service.UpdateTaskAsync(_userId, task);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*You can only update your own tasks*");
        }

        [Fact]
        public async Task UpdateTaskAsync_WithOtherUsersTask_DoesNotUpdateTask()
        {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            var service = CreateService();

            // Act
            var act = async () => await service.UpdateTaskAsync(_userId, task);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<BaseTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_WithOwnTask_LogsTaskUpdate()
        {
            // Arrange
            var task = CreateSimpleTask(userId: _userId);

            var service = CreateService();

            // Act
            await service.UpdateTaskAsync(_userId, task);

            // Assert
            VerifyLogInformation("Task");
            VerifyLogInformation("updated");
        }

        [Fact]
        public async Task UpdateTaskAsync_WithOwnTask_SavesChanges()
        {
            // Arrange
            var task = CreateSimpleTask(userId: _userId);

            var service = CreateService();

            // Act
            await service.UpdateTaskAsync(_userId, task);

            // Assert
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region RemoveTaskAsync Tests

        [Fact]
        public async Task RemoveTaskAsync_WithOwnTask_RemovesTaskSuccessfully()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            _taskRepositoryMock.Verify(x => x.DeleteAsync(taskId), Times.Once);
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveTaskAsync_WithNonExistentTask_ThrowsInvalidOperationException()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync((BaseTask?)null);

            var service = CreateService();

            // Act
            var act = async () => await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Task {taskId} not found*");
        }

        [Fact]
        public async Task RemoveTaskAsync_WithOtherUsersTask_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            var act = async () => await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*You can only delete your own tasks*");
        }

        [Fact]
        public async Task RemoveTaskAsync_WithOtherUsersTask_DoesNotRemoveTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            var act = async () => await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _taskRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RemoveTaskAsync_WithOwnTask_LogsTaskRemoval()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            VerifyLogInformation("Task");
            VerifyLogInformation("removed");
        }

        [Fact]
        public async Task RemoveTaskAsync_WithOwnTask_SavesChanges()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.RemoveTaskAsync(_userId, taskId);

            // Assert
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region MarkAsCompletedAsync Tests

        [Fact]
        public async Task MarkAsCompletedAsync_WithOwnTask_MarksTaskAsCompleted()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            task.IsCompleted.Should().BeTrue();
            _taskRepositoryMock.Verify(x => x.UpdateAsync(task), Times.Once);
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkAsCompletedAsync_WithNonExistentTask_ThrowsInvalidOperationException()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync((BaseTask?)null);

            var service = CreateService();

            // Act
            var act = async () => await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"*Task {taskId} not found*");
        }

        [Fact]
        public async Task MarkAsCompletedAsync_WithOtherUsersTask_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            var act = async () => await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*You can only modify your own tasks*");
        }

        [Fact]
        public async Task MarkAsCompletedAsync_WithOtherUsersTask_DoesNotUpdateTask()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: otherUserId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            var act = async () => await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            task.IsCompleted.Should().BeFalse();
            _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<BaseTask>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsCompletedAsync_WithOwnTask_LogsCompletion()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            VerifyLogInformation("Task");
            VerifyLogInformation("marked as completed");
        }

        [Fact]
        public async Task MarkAsCompletedAsync_WithOwnTask_SavesChanges()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = CreateSimpleTask(userId: _userId);

            _taskRepositoryMock
                .Setup(x => x.GetByIdAsync(taskId))
                .ReturnsAsync(task);

            var service = CreateService();

            // Act
            await service.MarkAsCompletedAsync(_userId, taskId);

            // Assert
            _taskRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        #endregion

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithMatchingKeyword_ReturnsMatchingTasks()
        {
            // Arrange
            var tasks = new List<BaseTask>
            {
                CreateSimpleTask("Buy groceries", _userId),
                CreateSimpleTask("Buy tickets", _userId),
                CreateSimpleTask("Clean house", _userId)
            };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "Buy");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Title == "Buy groceries");
            result.Should().Contain(t => t.Title == "Buy tickets");
        }

        [Fact]
        public async Task SearchAsync_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var tasks = new List<BaseTask>
            {
                CreateSimpleTask("Buy groceries", _userId),
                CreateSimpleTask("Clean house", _userId)
            };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "Nonexistent");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchAsync_IsCaseInsensitive()
        {
            // Arrange
            var tasks = new List<BaseTask>
            {
                CreateSimpleTask("Buy Groceries", _userId)
            };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "buy groceries");

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SearchAsync_SearchesInDescription()
        {
            // Arrange
            var task1 = new SimpleTask("Task 1", "Important description", _fixedDateTime.AddDays(1), Priority.Low);
            task1.AssignToUser(_userId);

            var task2 = new SimpleTask("Task 2", "Regular task", _fixedDateTime.AddDays(1), Priority.Low);
            task2.AssignToUser(_userId);

            var tasks = new List<BaseTask> { task1, task2 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "Important");

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Task 1");
        }

        [Fact]
        public async Task SearchAsync_WithEmptyTaskList_ReturnsEmptyList()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(new List<BaseTask>());

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "anything");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchAsync_WithPartialMatch_ReturnsMatchingTasks()
        {
            // Arrange
            var tasks = new List<BaseTask>
            {
                CreateSimpleTask("Shopping", _userId),
                CreateSimpleTask("Shop", _userId),
                CreateSimpleTask("Work", _userId)
            };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SearchAsync(_userId, "Shop");

            // Assert
            result.Should().HaveCount(2);
        }

        #endregion

        #region SortByDateAsync Tests

        [Fact]
        public async Task SortByDateAsync_Ascending_ReturnsSortedTasksAscending()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            var task2 = new SimpleTask("Task 2", "Description", _fixedDateTime.AddDays(2), Priority.Low);
            task2.AssignToUser(_userId);
            var task3 = new SimpleTask("Task 3", "Description", _fixedDateTime.AddDays(3), Priority.Low);
            task3.AssignToUser(_userId);

            var tasks = new List<BaseTask> { task3, task1, task2 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SortByDateAsync(_userId, ascending: true);

            // Assert
            var resultList = result.ToList();
            resultList[0].Title.Should().Be("Task 1");
            resultList[1].Title.Should().Be("Task 2");
            resultList[2].Title.Should().Be("Task 3");
        }

        [Fact]
        public async Task SortByDateAsync_Descending_ReturnsSortedTasksDescending()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            var task2 = new SimpleTask("Task 2", "Description", _fixedDateTime.AddDays(2), Priority.Low);
            task2.AssignToUser(_userId);
            var task3 = new SimpleTask("Task 3", "Description", _fixedDateTime.AddDays(3), Priority.Low);
            task3.AssignToUser(_userId);

            var tasks = new List<BaseTask> { task1, task2, task3 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SortByDateAsync(_userId, ascending: false);

            // Assert
            var resultList = result.ToList();
            resultList[0].Title.Should().Be("Task 3");
            resultList[1].Title.Should().Be("Task 2");
            resultList[2].Title.Should().Be("Task 1");
        }

        [Fact]
        public async Task SortByDateAsync_WithDefaultParameter_SortsAscending()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            var task2 = new SimpleTask("Task 2", "Description", _fixedDateTime.AddDays(2), Priority.Low);
            task2.AssignToUser(_userId);

            var tasks = new List<BaseTask> { task2, task1 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SortByDateAsync(_userId);

            // Assert
            var resultList = result.ToList();
            resultList[0].Title.Should().Be("Task 1");
            resultList[1].Title.Should().Be("Task 2");
        }

        [Fact]
        public async Task SortByDateAsync_WithEmptyTaskList_ReturnsEmptyList()
        {
            // Arrange
            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(new List<BaseTask>());

            var service = CreateService();

            // Act
            var result = await service.SortByDateAsync(_userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SortByDateAsync_WithSingleTask_ReturnsSingleTask()
        {
            // Arrange
            var task = CreateSimpleTask("Task 1", _userId);
            var tasks = new List<BaseTask> { task };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.SortByDateAsync(_userId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Task 1");
        }

        #endregion

        #region FilterByCompletionAsync Tests

        [Fact]
        public async Task FilterByCompletionAsync_WithCompletedTasks_ReturnsOnlyCompletedTasks()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            task1.MarkAsCompleted();

            var task2 = CreateSimpleTask("Task 2", _userId);

            var task3 = CreateSimpleTask("Task 3", _userId);
            task3.MarkAsCompleted();

            var tasks = new List<BaseTask> { task1, task2, task3 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.FilterByCompletionAsync(_userId, isCompleted: true);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Title == "Task 1");
            result.Should().Contain(t => t.Title == "Task 3");
            result.Should().OnlyContain(t => t.IsCompleted);
        }

        [Fact]
        public async Task FilterByCompletionAsync_WithIncompleteTasks_ReturnsOnlyIncompleteTasks()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            task1.MarkAsCompleted();

            var task2 = CreateSimpleTask("Task 2", _userId);

            var task3 = CreateSimpleTask("Task 3", _userId);

            var tasks = new List<BaseTask> { task1, task2, task3 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.FilterByCompletionAsync(_userId, isCompleted: false);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(t => t.Title == "Task 2");
            result.Should().Contain(t => t.Title == "Task 3");
            result.Should().OnlyContain(t => !t.IsCompleted);
        }

        [Fact]
        public async Task FilterByCompletionAsync_WithNoCompletedTasks_ReturnsEmptyList()
        {
            // Arrange
            var tasks = new List<BaseTask>
            {
                CreateSimpleTask("Task 1", _userId),
                CreateSimpleTask("Task 2", _userId)
            };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.FilterByCompletionAsync(_userId, isCompleted: true);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FilterByCompletionAsync_WithAllCompletedTasks_ReturnsEmptyListForIncomplete()
        {
            // Arrange
            var task1 = CreateSimpleTask("Task 1", _userId);
            task1.MarkAsCompleted();

            var task2 = CreateSimpleTask("Task 2", _userId);
            task2.MarkAsCompleted();

            var tasks = new List<BaseTask> { task1, task2 };

            _taskRepositoryMock
                .Setup(x => x.GetByUserIdAsync(_userId))
                .ReturnsAsync(tasks);

            var service = CreateService();

            // Act
            var result = await service.FilterByCompletionAsync(_userId, isCompleted: false);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
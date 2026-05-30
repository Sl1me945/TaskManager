using FluentAssertions;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoAppTests.Unit.Domain.Entities.Tasks
{
    public class SimpleTaskTests
    {
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private readonly DateTime _validDueDate;

        public SimpleTaskTests()
        {
            _validDueDate = _fixedDateTime.AddDays(1);
        }

        #region Constructor Tests - Simple Constructor

        [Fact]
        public void Constructor_WhenValid_ShouldCreateSimpleTask()
        {
            // Act
            var task = new SimpleTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High);

            // Assert
            task.Id.Should().NotBeEmpty();
            task.Title.Should().Be("Title");
            task.Priority.Should().Be(Priority.High);
        }

        #endregion

        #region Constructor Tests - Full Constructor

        [Fact]
        public void FullConstructor_WithValidParameters_ShouldCreateSimpleTask()
        {
            // Arrange
            var id = Guid.NewGuid();
            var createdAt = _fixedDateTime;
            var title = "Test Task";
            var description = "Test Description";
            var dueDate = _validDueDate;
            var priority = Priority.High;
            var isCompleted = true;
            var userId = Guid.NewGuid();

            // Act
            var task = new TestTask(id, createdAt, title, description, dueDate, priority, isCompleted, userId);

            // Assert
            task.Id.Should().Be(id);
            task.CreatedAt.Should().Be(createdAt);
            task.Title.Should().Be(title);
            task.Description.Should().Be(description);
            task.DueDate.Should().Be(dueDate);
            task.Priority.Should().Be(priority);
            task.IsCompleted.Should().Be(isCompleted);
            task.UserId.Should().Be(userId);
        }

        #endregion
    }
}

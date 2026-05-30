using FluentAssertions;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoAppTests.Unit.Domain.Entities.Tasks
{
    // Test implementation of abstract BaseTask
    public class TestTask : BaseTask
    {
        public TestTask(string title, string description, DateTime dueDate, Priority priority)
            : base(title, description, dueDate, priority)
        {
        }

        public TestTask(
            Guid id,
            DateTime createdAt,
            string title,
            string description,
            DateTime dueDate,
            Priority priority,
            bool isCompleted,
            Guid userId)
            : base(id, createdAt, title, description, dueDate, priority, isCompleted, userId)
        {
        }
    }

    public class BaseTaskTests
    {
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private readonly DateTime _validDueDate;

        public BaseTaskTests()
        {
            _validDueDate = _fixedDateTime.AddDays(1);
        }

        #region Constructor Tests - Simple Constructor

        [Fact]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var title = "Test Task";
            var description = "Test Description";
            var dueDate = _validDueDate;
            var priority = Priority.High;

            // Act
            var task = new TestTask(title, description, dueDate, priority);

            // Assert
            task.Title.Should().Be(title);
            task.Description.Should().Be(description);
            task.Priority.Should().Be(priority);
            task.IsCompleted.Should().BeFalse();
            task.Id.Should().NotBe(Guid.Empty);
            task.UserId.Should().Be(Guid.Empty);
        }

        [Fact]
        public void Constructor_WithEmptyTitle_ThrowsArgumentException()
        {
            // Act
            var act = () => new TestTask("", "Description", _validDueDate, Priority.Low);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot be empty*")
                .WithParameterName("value");
        }

        [Fact]
        public void Constructor_WithWhitespaceTitle_ThrowsArgumentException()
        {
            // Act
            var act = () => new TestTask("   ", "Description", _validDueDate, Priority.Low);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot be empty*");
        }

        [Fact]
        public void Constructor_WithNullTitle_ThrowsArgumentException()
        {
            // Act
            var act = () => new TestTask(null!, "Description", _validDueDate, Priority.Low);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot be empty*");
        }

        [Fact]
        public void Constructor_WithTitleExceeding200Characters_ThrowsArgumentException()
        {
            // Arrange
            var longTitle = new string('a', 201);

            // Act
            var act = () => new TestTask(longTitle, "Description", _validDueDate, Priority.Low);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot exceed 200 characters*");
        }

        [Fact]
        public void Constructor_WithTitleExactly200Characters_Succeeds()
        {
            // Arrange
            var title = new string('a', 200);

            // Act
            var task = new TestTask(title, "Description", _validDueDate, Priority.Low);

            // Assert
            task.Title.Should().Be(title);
        }

        [Fact]
        public void Constructor_WithNullDescription_SetsEmptyString()
        {
            // Act
            var task = new TestTask("Title", null!, _validDueDate, Priority.Low);

            // Assert
            task.Description.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithPastDueDate_ThrowsArgumentException()
        {
            // Arrange
            var pastDate = _fixedDateTime.AddDays(-1);

            // Act
            var act = () => new TestTask("Title", "Description", pastDate, Priority.Low);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Due date cannot be in the past*")
                .WithParameterName("dueDate");
        }

        [Theory]
        [InlineData(Priority.Low)]
        [InlineData(Priority.Medium)]
        [InlineData(Priority.High)]
        public void Constructor_WithDifferentPriorities_SetsPriorityCorrectly(Priority priority)
        {
            // Act
            var task = new TestTask("Title", "Description", _validDueDate, priority);

            // Assert
            task.Priority.Should().Be(priority);
        }

        #endregion

        #region Constructor Tests - Full Constructor

        [Fact]
        public void FullConstructor_WithValidParameters_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var createdAt = DateTime.UtcNow.AddDays(-5);
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

        [Fact]
        public void FullConstructor_WithInvalidTitle_ThrowsArgumentException()
        {
            // Act
            var act = () => new TestTask(Guid.NewGuid(), _fixedDateTime, "", "Description",
                _validDueDate, Priority.Low, false, Guid.NewGuid());

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region UpdateTitle Tests

        [Fact]
        public void UpdateTitle_WithValidTitle_UpdatesTitleSuccessfully()
        {
            // Arrange
            var task = new TestTask("Original", "Description", _validDueDate, Priority.Low);
            var newTitle = "Updated Title";

            // Act
            task.UpdateTitle(newTitle);

            // Assert
            task.Title.Should().Be(newTitle);
        }

        [Fact]
        public void UpdateTitle_WithEmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var task = new TestTask("Original", "Description", _validDueDate, Priority.Low);

            // Act
            var act = () => task.UpdateTitle("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot be empty*");
        }

        [Fact]
        public void UpdateTitle_WithTitleTooLong_ThrowsArgumentException()
        {
            // Arrange
            var task = new TestTask("Original", "Description", _validDueDate, Priority.Low);
            var longTitle = new string('a', 201);

            // Act
            var act = () => task.UpdateTitle(longTitle);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Title cannot exceed 200 characters*");
        }

        #endregion

        #region UpdateDescription Tests

        [Fact]
        public void UpdateDescription_WithValidDescription_UpdatesDescriptionSuccessfully()
        {
            // Arrange
            var task = new TestTask("Title", "Original", _validDueDate, Priority.Low);
            var newDescription = "Updated Description";

            // Act
            task.UpdateDescription(newDescription);

            // Assert
            task.Description.Should().Be(newDescription);
        }

        [Fact]
        public void UpdateDescription_WithNull_SetsEmptyString()
        {
            // Arrange
            var task = new TestTask("Title", "Original", _validDueDate, Priority.Low);

            // Act
            task.UpdateDescription(null!);

            // Assert
            task.Description.Should().BeEmpty();
        }

        [Fact]
        public void UpdateDescription_WithEmptyString_SetsEmptyString()
        {
            // Arrange
            var task = new TestTask("Title", "Original", _validDueDate, Priority.Low);

            // Act
            task.UpdateDescription("");

            // Assert
            task.Description.Should().BeEmpty();
        }

        #endregion

        #region SetDueDate Tests

        [Fact]
        public void SetDueDate_WithFutureDate_UpdatesDueDateSuccessfully()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            var newDueDate = _fixedDateTime.AddDays(5);

            // Act
            task.SetDueDate(newDueDate);

            // Assert
            task.DueDate.Should().Be(newDueDate);
        }

        [Fact]
        public void SetDueDate_WithPastDate_ThrowsArgumentException()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            var pastDate = _fixedDateTime.AddDays(-1);

            // Act
            var act = () => task.SetDueDate(pastDate);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Due date cannot be in the past*")
                .WithParameterName("dueDate");
        }

        [Fact]
        public void SetDueDate_WithCurrentTimePlusBuffer_Succeeds()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            var nearFuture = _fixedDateTime.AddSeconds(1);

            // Act
            task.SetDueDate(nearFuture);

            // Assert
            task.DueDate.Should().Be(nearFuture);
        }

        #endregion

        #region SetPriority Tests

        [Theory]
        [InlineData(Priority.Low)]
        [InlineData(Priority.Medium)]
        [InlineData(Priority.High)]
        public void SetPriority_WithValidPriority_UpdatesPrioritySuccessfully(Priority newPriority)
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Act
            task.SetPriority(newPriority);

            // Assert
            task.Priority.Should().Be(newPriority);
        }

        #endregion

        #region AssignToUser Tests

        [Fact]
        public void AssignToUser_WithValidUserId_AssignsUserSuccessfully()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            var userId = Guid.NewGuid();

            // Act
            task.AssignToUser(userId);

            // Assert
            task.UserId.Should().Be(userId);
        }

        [Fact]
        public void AssignToUser_WithEmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Act
            var act = () => task.AssignToUser(Guid.Empty);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*User ID cannot be empty*")
                .WithParameterName("userId");
        }

        [Fact]
        public void AssignToUser_MultipleTimes_UpdatesUserId()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            var firstUser = Guid.NewGuid();
            var secondUser = Guid.NewGuid();

            // Act
            task.AssignToUser(firstUser);
            task.AssignToUser(secondUser);

            // Assert
            task.UserId.Should().Be(secondUser);
        }

        #endregion

        #region Completion Status Tests

        [Fact]
        public void MarkAsCompleted_SetsIsCompletedToTrue()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Act
            task.MarkAsCompleted();

            // Assert
            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MarkAsNotCompleted_SetsIsCompletedToFalse()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);
            task.MarkAsCompleted();

            // Act
            task.MarkAsNotCompleted();

            // Assert
            task.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void MarkAsCompleted_CalledMultipleTimes_RemainsCompleted()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Act
            task.MarkAsCompleted();
            task.MarkAsCompleted();

            // Assert
            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void MarkAsNotCompleted_CalledMultipleTimes_RemainsNotCompleted()
        {
            // Arrange
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Act
            task.MarkAsNotCompleted();
            task.MarkAsNotCompleted();

            // Assert
            task.IsCompleted.Should().BeFalse();
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void Id_IsGeneratedAndUnique()
        {
            // Arrange & Act
            var task1 = new TestTask("Title 1", "Description", _validDueDate, Priority.Low);
            var task2 = new TestTask("Title 2", "Description", _validDueDate, Priority.Low);

            // Assert
            task1.Id.Should().NotBe(Guid.Empty);
            task2.Id.Should().NotBe(Guid.Empty);
            task1.Id.Should().NotBe(task2.Id);
        }

        [Fact]
        public void CreatedAt_IsSetToCurrentUtcTime()
        {
            // Act
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Assert
            task.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void IsCompleted_DefaultsToFalse()
        {
            // Act
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Assert
            task.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void UserId_DefaultsToEmptyGuid()
        {
            // Act
            var task = new TestTask("Title", "Description", _validDueDate, Priority.Low);

            // Assert
            task.UserId.Should().Be(Guid.Empty);
        }

        #endregion
    }
}
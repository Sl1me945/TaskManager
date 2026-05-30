using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoAppTests.Unit.Domain.Entities.Tasks
{
    public class RecurringTaskTests
    {
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private readonly DateTime _validDueDate;

        public RecurringTaskTests()
        {
            _validDueDate = _fixedDateTime.AddDays(1);
        }

        #region Constructor Tests - RepeatInterval 

        [Fact]
        public void Constructor_WhenValidInterval_ShouldCreate()
        {
            // Act
            var task = new RecurringTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.Low,
                TimeSpan.FromDays(1));

            // Assert
            task.RepeatInterval.Should().Be(TimeSpan.FromDays(1));
        }

        [Fact]
        public void Constructor_WhenIntervalZero_Throws()
        {
            // Act
            Action act = () => new RecurringTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.Low,
                TimeSpan.Zero);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WhenIntervalNegative_Throws()
        {
            // Act
            Action act = () => new RecurringTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.Low,
                TimeSpan.FromDays(-1));

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WhenIntervalTooLarge_Throws()
        {
            // Act
            Action act = () => new RecurringTask(
                "T",
                "D",
                _validDueDate,
                Priority.Low,
                TimeSpan.FromDays(366));

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region UpdateRepeatInterval Tests

        [Fact]
        public void UpdateRepeatInterval_WhenValid_Updates()
        {
            // Arrange
            var task = new RecurringTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.Low,
                TimeSpan.FromDays(1));

            // Act
            task.UpdateRepeatInterval(TimeSpan.FromDays(7));

            // Assert
            task.RepeatInterval.Should().Be(TimeSpan.FromDays(7));
        }

        [Fact]
        public void UpdateRepeatInterval_WhenInvalid_Throws()
        {
            // Arrange
            var task = new RecurringTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.Low,
                TimeSpan.FromDays(1));

            // Act
            Action act = () => task.UpdateRepeatInterval(TimeSpan.Zero);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion
    }
}

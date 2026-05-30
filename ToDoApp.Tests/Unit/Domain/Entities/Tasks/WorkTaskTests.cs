using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using ToDoApp.Domain.Entities.Tasks;
using ToDoApp.Domain.Enums;

namespace ToDoAppTests.Unit.Domain.Entities.Tasks
{
    public class WorkTaskTests
    {
        private readonly DateTime _fixedDateTime = DateTime.UtcNow;
        private readonly DateTime _validDueDate;

        public WorkTaskTests()
        {
            _validDueDate = _fixedDateTime.AddDays(1);
        }

        #region Constructor Tests - ProjectName

        [Fact]
        public void Constructor_WhenValidProjectName_ShouldCreate()
        {
            // Act
            var task = new WorkTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High,
                "Project X");

            // Assert
            task.ProjectName.Should().Be("Project X");
        }

        [Fact]
        public void Constructor_WhenProjectEmpty_Throws()
        {
            // Act
            Action act = () => new WorkTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High,
                "");

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Constructor_WhenProjectTooLong_Throws()
        {
            // Arrange
            var big = new string('x', 201);

            // Act
            Action act = () => new WorkTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High,
                big);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region UpdateProjectName Tests

        [Fact]
        public void UpdateProjectName_WhenValid_Updates()
        {
            // Arrange
            var task = new WorkTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High,
                "Proj");

            // Act
            task.UpdateProjectName("NewProj");

            // Assert
            task.ProjectName.Should().Be("NewProj");
        }

        [Fact]
        public void UpdateProjectName_WhenEmpty_Throws()
        {
            // Arrange
            var task = new WorkTask(
                "Title",
                "Desc",
                _validDueDate,
                Priority.High,
                "Proj");

            // Act
            Action act = () => task.UpdateProjectName("");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        #endregion
    }
}

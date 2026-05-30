using FluentAssertions;
using ToDoApp.Domain.Entities;

namespace ToDoAppTests.Unit.Domain
{
    public class UserTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidArguments_CreatesUser()
        {
            // Act
            var user = new User("Test username", "Hash_for_test_user");

            // Assert
            user.Id.Should().NotBeEmpty();
            user.Username.Should().Be("Test username");
            user.PasswordHash.Should().Be("Hash_for_test_user");
        }

        [Fact]
        public void Constructor_WithEmptyUsername_ThrowsArgumentException()
        {
            // Act
            var act = () => new User("", "Hash_for_test_user");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void Constructor_WithWhitespaceUsername_ThrowsArgumentException()
        {
            // Act
            var act = () => new User("            ", "Hash_for_test_user");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void Constructor_WithUsernameTooShort_ThrowsArgumentException()
        {
            // Act
            var act = () => new User("short", "Hash_for_test_user");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void Constructor_WithUsernameTooLong_ThrowsArgumentException()
        {
            // Act
            var act = () => new User("too long username 12345 67890", "Hash_for_test_user");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void Constructor_WithEmptyPasswordHash_ThrowsArgumentException()
        {
            // Act
            var act = () => new User("Username", "");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*password hash*");
        }

        #endregion

        #region UpdateUsername Tests

        [Fact]
        public void UpdateUsername_WithValidUsername_UpdatesSuccessfully()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            user.UpdateUsername("newValidName");

            // Assert
            user.Username.Should().Be("newValidName");
        }

        [Fact]
        public void UpdateUsername_WithEmptyUsername_ThrowsArgumentException()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            var act = () => user.UpdateUsername("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void UpdateUsername_WithUsernameTooShort_ThrowsArgumentException()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            var act = () => user.UpdateUsername("12345");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void UpdateUsername_WithUsernameTooLong_ThrowsArgumentException()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            var act = () => user.UpdateUsername("too long username 12345 67890");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*username*");
        }

        [Fact]
        public void UpdateUsername_DoesNotChangeUserIdentity()
        {
            // Arrange
            var user = new User("validName", "hash");
            var originalId = user.Id;

            // Act
            user.UpdateUsername("newvalidname");

            // Assert
            user.Id.Should().Be(originalId);
        }

        #endregion

        #region UpdatePasswordHash Tests

        [Fact]
        public void UpdatePasswordHash_WithValidHash_UpdatesSuccessfully()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            user.UpdatePasswordHash("new_hash");

            // Assert
            user.PasswordHash.Should().Be("new_hash");
        }

        [Fact]
        public void UpdatePasswordHash_WithEmptyHash_ThrowsArgumentException()
        {
            // Arrange
            var user = new User("validName", "hash");

            // Act
            var act = () => user.UpdatePasswordHash("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*password hash*");
        }

        [Fact]
        public void UpdatePasswordHash_DoesNotChangeUserIdentity()
        {
            // Arrange
            var user = new User("validName", "hash");
            var originalId = user.Id;

            // Act
            user.UpdatePasswordHash("newhash");

            // Assert
            user.Id.Should().Be(originalId);
        }

        #endregion
    }
}
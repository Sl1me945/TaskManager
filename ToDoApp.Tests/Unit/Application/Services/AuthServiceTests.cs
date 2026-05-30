using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoApp.Application.Interfaces;
using ToDoApp.Application.Services;
using ToDoApp.Domain.Entities;

namespace ToDoAppTests.Unit.Application.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        public AuthServiceTests()
        {
            _loggerMock = new Mock<ILogger<AuthService>>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenServiceMock = new Mock<ITokenService>();
        }

        #region Helper Methods

        private AuthService CreateService()
        {
            return new AuthService(
                _loggerMock.Object,
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _tokenServiceMock.Object
            );
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

        #region SignUpAsync Tests

        [Fact]
        public async Task SignUpAsync_WithNewUser_CreatesUserSuccessfully()
        {
            // Arrange
            var username = "newuser";
            var password = "SecurePassword123!";
            var hashedPassword = "hashed_password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns(hashedPassword);

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password);

            // Assert
            _userRepositoryMock.Verify(
                x => x.AddAsync(It.Is<User>(u => u.Username == username && u.PasswordHash == hashedPassword)),
                Times.Once);
        }

        [Fact]
        public async Task SignUpAsync_WithNewUser_HashesPassword()
        {
            // Arrange
            var username = "newuser";
            var password = "SecurePassword123!";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns("hashed_password");

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password);

            // Assert
            _passwordHasherMock.Verify(x => x.Hash(password), Times.Once);
        }

        [Fact]
        public async Task SignUpAsync_WithExistingUsername_ThrowsInvalidOperationException()
        {
            // Arrange
            var username = "existinguser";
            var password = "password";
            var existingUser = new User(username, "existing_hash");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(existingUser);

            var service = CreateService();

            // Act
            var act = async () => await service.SignUpAsync(username, password);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*username is already taken*");
        }

        [Fact]
        public async Task SignUpAsync_WithExistingUsername_DoesNotHashPassword()
        {
            // Arrange
            var username = "existinguser";
            var password = "password";
            var existingUser = new User(username, "existing_hash");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(existingUser);

            var service = CreateService();

            // Act
            var act = async () => await service.SignUpAsync(username, password);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignUpAsync_WithExistingUsername_DoesNotAddUser()
        {
            // Arrange
            var username = "existinguser";
            var password = "password";
            var existingUser = new User(username, "existing_hash");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(existingUser);

            var service = CreateService();

            // Act
            var act = async () => await service.SignUpAsync(username, password);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SignUpAsync_WithNewUser_LogsSignUpAttempt()
        {
            // Arrange
            var username = "newuser";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns("hashed_password");

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password);

            // Assert
            VerifyLogInformation("Sign up attempt");
        }

        [Fact]
        public async Task SignUpAsync_WithNewUser_LogsSuccessfulSignUp()
        {
            // Arrange
            var username = "newuser";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns("hashed_password");

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password);

            // Assert
            VerifyLogInformation("Successfully sign up");
        }

        [Fact]
        public async Task SignUpAsync_WithValidCredentials_ChecksUsernameExists()
        {
            // Arrange
            var username = "newuser";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns("hashed_password");

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
        }

        [Fact]
        public async Task SignUpAsync_WithDifferentPasswords_CreatesDifferentHashes()
        {
            // Arrange
            var username = "newuser";
            var password1 = "Password1";
            var password2 = "Password2";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password1))
                .Returns("hash1");

            _passwordHasherMock
                .Setup(x => x.Hash(password2))
                .Returns("hash2");

            var service = CreateService();

            // Act
            await service.SignUpAsync(username, password1);
            await service.SignUpAsync(username, password2);

            // Assert
            _passwordHasherMock.Verify(x => x.Hash(password1), Times.Once);
            _passwordHasherMock.Verify(x => x.Hash(password2), Times.Once);
        }

        #endregion

        #region SignInAsync Tests

        [Fact]
        public async Task SignInAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var username = "testuser";
            var password = "password";
            var user = new User(username, "hashed_password");
            var expectedToken = "generated_token";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns(expectedToken);

            var service = CreateService();

            // Act
            var result = await service.SignInAsync(username, password);

            // Assert
            result.Should().Be(expectedToken);
        }

        [Fact]
        public async Task SignInAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            var result = await service.SignInAsync(username, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SignInAsync_WithIncorrectPassword_ReturnsNull()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var user = new User(username, "hashed_password");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(false);

            var service = CreateService();

            // Act
            var result = await service.SignInAsync(username, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SignInAsync_WithIncorrectPassword_DoesNotGenerateToken()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var user = new User(username, "hashed_password");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(false);

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task SignInAsync_WithNonExistentUser_DoesNotVerifyPassword()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignInAsync_WithValidCredentials_VerifiesPassword()
        {
            // Arrange
            var username = "testuser";
            var password = "password";
            var user = new User(username, "hashed_password");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns("token");

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            _passwordHasherMock.Verify(x => x.Verify(user.PasswordHash, password), Times.Once);
        }

        [Fact]
        public async Task SignInAsync_WithValidCredentials_GeneratesToken()
        {
            // Arrange
            var username = "testuser";
            var password = "password";
            var user = new User(username, "hashed_password");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns("token");

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            _tokenServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
        }

        [Fact]
        public async Task SignInAsync_WithValidCredentials_LogsSignInAttempt()
        {
            // Arrange
            var username = "testuser";
            var password = "password";
            var user = new User(username, "hashed_password");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(x => x.Verify(user.PasswordHash, password))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user))
                .Returns("token");

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            VerifyLogInformation("Sign in attempt");
        }

        [Fact]
        public async Task SignInAsync_WithNonExistentUser_LogsSignInAttempt()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password";

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            await service.SignInAsync(username, password);

            // Assert
            VerifyLogInformation("Sign in attempt");
        }

        [Fact]
        public async Task SignInAsync_WithDifferentUsers_GeneratesDifferentTokens()
        {
            // Arrange
            var user1 = new User("username1", "hash1");
            var user2 = new User("username2", "hash2");

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync("user1"))
                .ReturnsAsync(user1);

            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync("user2"))
                .ReturnsAsync(user2);

            _passwordHasherMock
                .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user1))
                .Returns("token1");

            _tokenServiceMock
                .Setup(x => x.GenerateToken(user2))
                .Returns("token2");

            var service = CreateService();

            // Act
            var result1 = await service.SignInAsync("user1", "password");
            var result2 = await service.SignInAsync("user2", "password");

            // Assert
            result1.Should().Be("token1");
            result2.Should().Be("token2");
            result1.Should().NotBe(result2);
        }

        #endregion

        #region SignOutAsync Tests

        [Fact]
        public async Task SignOutAsync_WithValidToken_RevokesToken()
        {
            // Arrange
            var token = "valid_token";
            var service = CreateService();

            // Act
            await service.SignOutAsync(token);

            // Assert
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task SignOutAsync_WithNullToken_DoesNotRevokeToken()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SignOutAsync(null);

            // Assert
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignOutAsync_WithEmptyToken_DoesNotRevokeToken()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SignOutAsync("");

            // Assert
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignOutAsync_WithWhitespaceToken_DoesNotRevokeToken()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SignOutAsync("   ");

            // Assert
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SignOutAsync_WithValidToken_LogsSignOut()
        {
            // Arrange
            var token = "valid_token";
            var service = CreateService();

            // Act
            await service.SignOutAsync(token);

            // Assert
            VerifyLogInformation("Sign out");
        }

        [Fact]
        public async Task SignOutAsync_WithNullToken_LogsSignOut()
        {
            // Arrange
            var service = CreateService();

            // Act
            await service.SignOutAsync(null);

            // Assert
            VerifyLogInformation("Sign out");
        }

        [Fact]
        public async Task SignOutAsync_WithAnyToken_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = async () => await service.SignOutAsync("any_token");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SignOutAsync_CalledMultipleTimes_RevokesMultipleTimes()
        {
            // Arrange
            var token = "valid_token";
            var service = CreateService();

            // Act
            await service.SignOutAsync(token);
            await service.SignOutAsync(token);

            // Assert
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(token), Times.Exactly(2));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteAuthFlow_SignUpSignInSignOut_WorksCorrectly()
        {
            // Arrange
            var username = "testuser";
            var password = "password";
            var hashedPassword = "hashed_password";
            var token = "generated_token";

            // Setup for SignUp
            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            _passwordHasherMock
                .Setup(x => x.Hash(password))
                .Returns(hashedPassword);

            var service = CreateService();

            // Act - Sign Up
            await service.SignUpAsync(username, password);

            // Setup for SignIn - user now exists
            var createdUser = new User(username, hashedPassword);
            _userRepositoryMock
                .Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(createdUser);

            _passwordHasherMock
                .Setup(x => x.Verify(hashedPassword, password))
                .Returns(true);

            _tokenServiceMock
                .Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns(token);

            // Act - Sign In
            var signInResult = await service.SignInAsync(username, password);

            // Act - Sign Out
            await service.SignOutAsync(signInResult);

            // Assert
            signInResult.Should().Be(token);
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
            _tokenServiceMock.Verify(x => x.RevokeTokenAsync(token), Times.Once);
        }

        #endregion
    }
}
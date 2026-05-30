using FluentAssertions;
using ToDoApp.Infrastructure.Services;

namespace ToDoAppTests.Unit.Infrastructure.Services
{
    public class Pbkdf2PasswordHasherTests
    {
        private readonly Pbkdf2PasswordHasher _hasher;

        public Pbkdf2PasswordHasherTests()
        {
            _hasher = new Pbkdf2PasswordHasher();
        }

        #region Hash Tests

        [Fact]
        public void Hash_WithValidPassword_ReturnsHashedString()
        {
            // Arrange
            var password = "SecurePassword123!";

            // Act
            var hashed = _hasher.Hash(password);

            // Assert
            hashed.Should().NotBeNullOrWhiteSpace();
            hashed.Split('.').Should().HaveCount(3);
        }

        [Fact]
        public void Hash_WithValidPassword_ReturnsCorrectFormat()
        {
            // Arrange
            var password = "MyPassword";

            // Act
            var hashed = _hasher.Hash(password);

            // Assert
            var parts = hashed.Split('.');
            parts[0].Should().MatchRegex(@"^\d+$"); // iterations
            parts[1].Should().NotBeNullOrEmpty(); // salt (base64)
            parts[2].Should().NotBeNullOrEmpty(); // hash (base64)
        }

        [Fact]
        public void Hash_WithEmptyPassword_ThrowsArgumentException()
        {
            // Act
            var act = () => _hasher.Hash("");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Password must not be empty*")
                .WithParameterName("password");
        }

        [Fact]
        public void Hash_WithWhitespacePassword_ThrowsArgumentException()
        {
            // Act
            var act = () => _hasher.Hash("   ");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Password must not be empty*")
                .WithParameterName("password");
        }

        [Fact]
        public void Hash_WithNullPassword_ThrowsArgumentException()
        {
            // Act
            var act = () => _hasher.Hash(null!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("password");
        }

        [Fact]
        public void Hash_SamePasswordTwice_GeneratesDifferentHashes()
        {
            // Arrange
            var password = "SamePassword";

            // Act
            var hash1 = _hasher.Hash(password);
            var hash2 = _hasher.Hash(password);

            // Assert
            hash1.Should().NotBe(hash2, "different salts should produce different hashes");
        }

        [Fact]
        public void Hash_WithLongPassword_HandlesCorrectly()
        {
            // Arrange
            var password = new string('a', 1000);

            // Act
            var hashed = _hasher.Hash(password);

            // Assert
            hashed.Should().NotBeNullOrWhiteSpace();
            hashed.Split('.').Should().HaveCount(3);
        }

        [Fact]
        public void Hash_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "P@ssw0rd!#$%^&*()";

            // Act
            var hashed = _hasher.Hash(password);

            // Assert
            hashed.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Hash_WithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "пароль密码🔐";

            // Act
            var hashed = _hasher.Hash(password);

            // Assert
            hashed.Should().NotBeNullOrWhiteSpace();
        }

        #endregion

        #region Verify Tests

        [Fact]
        public void Verify_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "CorrectPassword";
            var hashed = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(hashed, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "CorrectPassword";
            var hashed = _hasher.Hash(password);
            var wrongPassword = "WrongPassword";

            // Act
            var result = _hasher.Verify(hashed, wrongPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            var password = "Password";
            var hashed = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(hashed, "");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithEmptyHash_ReturnsFalse()
        {
            // Act
            var result = _hasher.Verify("", "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithNullHash_ReturnsFalse()
        {
            // Act
            var result = _hasher.Verify(null!, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithNullPassword_ReturnsFalse()
        {
            // Arrange
            var hashed = _hasher.Hash("Password");

            // Act
            var result = _hasher.Verify(hashed, null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithInvalidHashFormat_ReturnsFalse()
        {
            // Arrange
            var invalidHashes = new[]
            {
                "invalid",
                "one.two",
                "100000.salt",
                "not.a.valid.hash.format"
            };

            // Act & Assert
            foreach (var invalidHash in invalidHashes)
            {
                var result = _hasher.Verify(invalidHash, "Password");
                result.Should().BeFalse($"hash '{invalidHash}' should be invalid");
            }
        }

        [Fact]
        public void Verify_WithInvalidIterations_ReturnsFalse()
        {
            // Arrange
            var invalidHash = "notanumber.c2FsdA==.aGFzaA==";

            // Act
            var result = _hasher.Verify(invalidHash, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithZeroIterations_ReturnsFalse()
        {
            // Arrange
            var invalidHash = "0.c2FsdA==.aGFzaA==";

            // Act
            var result = _hasher.Verify(invalidHash, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithNegativeIterations_ReturnsFalse()
        {
            // Arrange
            var invalidHash = "-100.c2FsdA==.aGFzaA==";

            // Act
            var result = _hasher.Verify(invalidHash, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithInvalidBase64Salt_ReturnsFalse()
        {
            // Arrange
            var invalidHash = "100000.not-base64!.aGFzaA==";

            // Act
            var result = _hasher.Verify(invalidHash, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithInvalidBase64Hash_ReturnsFalse()
        {
            // Arrange
            var invalidHash = "100000.c2FsdA==.not-base64!";

            // Act
            var result = _hasher.Verify(invalidHash, "Password");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithModifiedHash_ReturnsFalse()
        {
            // Arrange
            var password = "Password";
            var hashed = _hasher.Hash(password);
            var parts = hashed.Split('.');
            var modifiedHash = $"{parts[0]}.{parts[1]}.modified{parts[2]}";

            // Act
            var result = _hasher.Verify(modifiedHash, password);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_WithDifferentCasing_ReturnsFalse()
        {
            // Arrange
            var password = "Password";
            var hashed = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(hashed, "password");

            // Assert
            result.Should().BeFalse("passwords should be case-sensitive");
        }

        [Fact]
        public void Verify_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "P@ssw0rd!#$%";
            var hashed = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(hashed, password);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Verify_WithUnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var password = "пароль密码🔐";
            var hashed = _hasher.Hash(password);

            // Act
            var result = _hasher.Verify(hashed, password);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Security Tests

        [Fact]
        public void HashAndVerify_MultipleRounds_WorksConsistently()
        {
            // Arrange
            var password = "ConsistentPassword";

            // Act & Assert
            for (int i = 0; i < 10; i++)
            {
                var hashed = _hasher.Hash(password);
                var verified = _hasher.Verify(hashed, password);
                verified.Should().BeTrue($"iteration {i} should verify correctly");
            }
        }

        [Fact]
        public void Hash_UsesConstantTimeComparison()
        {
            // This test verifies that Verify is implemented (we can't easily test timing)
            // but we verify the method works correctly with similar but different passwords
            // Arrange
            var password1 = "password1";
            var password2 = "password2";
            var hash1 = _hasher.Hash(password1);

            // Act
            var result = _hasher.Verify(hash1, password2);

            // Assert
            result.Should().BeFalse("different passwords should not match");
        }

        #endregion
    }
}
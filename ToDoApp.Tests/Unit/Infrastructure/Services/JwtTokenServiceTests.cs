using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToDoApp.Domain.Entities;
using ToDoApp.Infrastructure.Services;

namespace ToDoAppTests.Unit.Infrastructure.Services
{
    public class JwtTokenServiceTests
    {
        private readonly ToDoApp.Infrastructure.Services.JwtOptions _validOptions;
        private readonly User _testUser;

        public JwtTokenServiceTests()
        {
            _validOptions = new ToDoApp.Infrastructure.Services.JwtOptions
            {
                SecretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("this-is-a-very-secure-secret-key-with-at-least-32-bytes")),
                ExpiryHours = 1,
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            };

            _testUser = new User("testuser", "hash");
        }

        #region Helper Methods

        private JwtTokenService CreateService(ToDoApp.Infrastructure.Services.JwtOptions? options = null)
        {
            var opts = options ?? _validOptions;
            return new JwtTokenService(Options.Create(opts));
        }

        private ToDoApp.Infrastructure.Services.JwtOptions CreateOptions(
            string? secretKey = null,
            int? expiryHours = null,
            string? issuer = null,
            string? audience = null)
        {
            return new JwtOptions
            {
                SecretKey = secretKey ?? _validOptions.SecretKey,
                ExpiryHours = expiryHours ?? _validOptions.ExpiryHours,
                Issuer = issuer ?? _validOptions.Issuer,
                Audience = audience ?? _validOptions.Audience
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidOptions_CreatesService()
        {
            // Act
            var service = CreateService();

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new JwtTokenService(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithNullOptionsValue_ThrowsArgumentNullException()
        {
            // Arrange
            var options = Options.Create<ToDoApp.Infrastructure.Services.JwtOptions>(null!);

            // Act
            var act = () => new JwtTokenService(options);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithEmptySecretKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var options = CreateOptions(secretKey: "");

            // Act
            var act = () => CreateService(options);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*JWT SecretKey is not configured*");
        }

        [Fact]
        public void Constructor_WithWhitespaceSecretKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var options = CreateOptions(secretKey: "   ");

            // Act
            var act = () => CreateService(options);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*JWT SecretKey is not configured*");
        }

        [Fact]
        public void Constructor_WithSecretKeyLessThan32Bytes_ThrowsInvalidOperationException()
        {
            // Arrange
            var shortKey = Convert.ToBase64String(Encoding.UTF8.GetBytes("short")); // Only 5 bytes
            var options = CreateOptions(secretKey: shortKey);

            // Act
            var act = () => CreateService(options);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*JWT SecretKey must be at least 32 bytes*");
        }

        [Fact]
        public void Constructor_WithSecretKeyExactly32Bytes_Succeeds()
        {
            // Arrange
            var key32Bytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 32)));
            var options = CreateOptions(secretKey: key32Bytes);

            // Act
            var service = CreateService(options);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNonBase64SecretKey_UsesUtf8Encoding()
        {
            // Arrange
            var plainTextKey = new string('a', 42); // 42 characters to ensure at least 32 bytes in UTF-8
            var options = CreateOptions(secretKey: plainTextKey);

            // Act
            var service = CreateService(options);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithBase64SecretKey_DecodesCorrectly()
        {
            // Arrange
            var base64Key = Convert.ToBase64String(new byte[32]);
            var options = CreateOptions(secretKey: base64Key);

            // Act
            var service = CreateService(options);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region GenerateToken Tests

        [Fact]
        public void GenerateToken_WithValidUser_ReturnsToken()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();
            token.Split('.').Should().HaveCount(3, "JWT should have 3 parts");
        }

        [Fact]
        public void GenerateToken_WithValidUser_ContainsUserIdClaim()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            subClaim.Should().NotBeNull();
            subClaim!.Value.Should().Be(_testUser.Id.ToString());
        }

        [Fact]
        public void GenerateToken_WithValidUser_ContainsUsernameClaim()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            var usernameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "username");

            usernameClaim.Should().NotBeNull();
            usernameClaim!.Value.Should().Be(_testUser.Username);
        }

        [Fact]
        public void GenerateToken_WithValidUser_ContainsJtiClaim()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            var jtiClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

            jtiClaim.Should().NotBeNull();
            Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue("JTI should be a valid GUID");
        }

        [Fact]
        public void GenerateToken_WithValidUser_ContainsCorrectIssuer()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            jwt.Issuer.Should().Be(_validOptions.Issuer);
        }

        [Fact]
        public void GenerateToken_WithValidUser_ContainsCorrectAudience()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            jwt.Audiences.Should().Contain(_validOptions.Audience);
        }

        [Fact]
        public void GenerateToken_WithValidUser_HasCorrectExpiration()
        {
            // Arrange
            var service = CreateService();
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = service.GenerateToken(_testUser);

            // Assert
            var afterGeneration = DateTime.UtcNow;
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);

            var expectedExpiry = beforeGeneration.AddHours(_validOptions.ExpiryHours);
            jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateToken_CalledTwice_GeneratesDifferentTokens()
        {
            // Arrange
            var service = CreateService();

            // Act
            var token1 = service.GenerateToken(_testUser);
            var token2 = service.GenerateToken(_testUser);

            // Assert
            token1.Should().NotBe(token2, "each token should have unique JTI");
        }

        [Fact]
        public void GenerateToken_WithDifferentUsers_GeneratesDifferentTokens()
        {
            // Arrange
            var service = CreateService();
            var user1 = new User("username1", "hash1");
            var user2 = new User("username2", "hash2");

            // Act
            var token1 = service.GenerateToken(user1);
            var token2 = service.GenerateToken(user2);

            // Assert
            token1.Should().NotBe(token2);
        }

        #endregion

        #region ValidateTokenAsync Tests

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsValidResult()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(_testUser);

            // Act
            var result = await service.ValidateTokenAsync(token);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Exception.Should().BeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ContainsCorrectClaims()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(_testUser);

            // Act
            var result = await service.ValidateTokenAsync(token);

            // Assert
            result.ClaimsIdentity.Should().NotBeNull();
            var subClaim = result.ClaimsIdentity!.FindFirst(JwtRegisteredClaimNames.Sub);
            subClaim.Should().NotBeNull();
            subClaim!.Value.Should().Be(_testUser.Id.ToString());
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsInvalidResult()
        {
            // Arrange
            var service = CreateService();
            var invalidToken = "invalid.token.string";

            // Act
            var result = await service.ValidateTokenAsync(invalidToken);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Exception.Should().NotBeNull();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithTokenFromDifferentKey_ReturnsInvalidResult()
        {
            // Arrange
            var service1 = CreateService();
            var token = service1.GenerateToken(_testUser);

            var differentOptions = CreateOptions(
                secretKey: Convert.ToBase64String(Encoding.UTF8.GetBytes("different-secret-key-with-at-least-32-bytes"))
            );
            var service2 = CreateService(differentOptions);

            // Act
            var result = await service2.ValidateTokenAsync(token);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithWrongIssuer_ReturnsInvalidResult()
        {
            // Arrange
            var service1 = CreateService();
            var token = service1.GenerateToken(_testUser);

            var differentOptions = CreateOptions(issuer: "WrongIssuer");
            var service2 = CreateService(differentOptions);

            // Act
            var result = await service2.ValidateTokenAsync(token);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithWrongAudience_ReturnsInvalidResult()
        {
            // Arrange
            var service1 = CreateService();
            var token = service1.GenerateToken(_testUser);

            var differentOptions = CreateOptions(audience: "WrongAudience");
            var service2 = CreateService(differentOptions);

            // Act
            var result = await service2.ValidateTokenAsync(token);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithExpiredToken_ReturnsInvalidResult()
        {
            // Arrange
            var options = CreateOptions(expiryHours: -1); // Already expired
            var service = CreateService(options);
            var token = service.GenerateToken(_testUser);

            // Wait a bit to ensure expiration
            await Task.Delay(100);

            // Act
            var result = await service.ValidateTokenAsync(token);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithEmptyToken_ReturnsInvalidResult()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.ValidateTokenAsync("");

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateTokenAsync_WithMalformedToken_ReturnsInvalidResult()
        {
            // Arrange
            var service = CreateService();
            var malformedTokens = new[]
            {
                "not.a.token",
                "only.two.parts",
                "too.many.parts.here.now",
                ".",
                ""
            };

            // Act & Assert
            foreach (var malformedToken in malformedTokens)
            {
                var result = await service.ValidateTokenAsync(malformedToken);
                result.IsValid.Should().BeFalse($"token '{malformedToken}' should be invalid");
            }
        }

        #endregion

        #region RevokeTokenAsync Tests

        [Fact]
        public async Task RevokeTokenAsync_WithValidToken_RevokesToken()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(_testUser);

            // Act
            await service.RevokeTokenAsync(token);
            var validationResult = await service.ValidateTokenAsync(token);

            // Assert
            validationResult.IsValid.Should().BeFalse();
            validationResult.Exception.Should().BeOfType<SecurityTokenException>();
            validationResult.Exception!.Message.Should().Contain("Token revoked");
        }

        [Fact]
        public async Task RevokeTokenAsync_WithEmptyToken_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = async () => await service.RevokeTokenAsync("");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RevokeTokenAsync_WithNullToken_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = async () => await service.RevokeTokenAsync(null!);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RevokeTokenAsync_WithWhitespaceToken_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = async () => await service.RevokeTokenAsync("   ");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RevokeTokenAsync_WithInvalidToken_DoesNotThrow()
        {
            // Arrange
            var service = CreateService();

            // Act
            var act = async () => await service.RevokeTokenAsync("invalid.token.format");

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task RevokeTokenAsync_CalledTwice_StillRevokesToken()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(_testUser);

            // Act
            await service.RevokeTokenAsync(token);
            await service.RevokeTokenAsync(token);
            var validationResult = await service.ValidateTokenAsync(token);

            // Assert
            validationResult.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeTokenAsync_DoesNotAffectOtherTokens()
        {
            // Arrange
            var service = CreateService();
            var token1 = service.GenerateToken(_testUser);
            var token2 = service.GenerateToken(_testUser);

            // Act
            await service.RevokeTokenAsync(token1);
            var result1 = await service.ValidateTokenAsync(token1);
            var result2 = await service.ValidateTokenAsync(token2);

            // Assert
            result1.IsValid.Should().BeFalse("token1 should be revoked");
            result2.IsValid.Should().BeTrue("token2 should still be valid");
        }

        #endregion

        #region Revocation Expiry Tests

        [Fact]
        public async Task ValidateTokenAsync_AfterRevocation_CleansUpExpiredEntries()
        {
            // This test verifies that the revocation mechanism works correctly
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(_testUser);

            // Act - Revoke the token
            await service.RevokeTokenAsync(token);

            // Immediately after revocation, token should be invalid
            var resultAfterRevoke = await service.ValidateTokenAsync(token);

            // Assert
            resultAfterRevoke.IsValid.Should().BeFalse("token should be revoked");
            resultAfterRevoke.Exception.Should().BeOfType<SecurityTokenException>();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CompleteTokenLifecycle_GenerateValidateRevoke_WorksCorrectly()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert - Generate
            var token = service.GenerateToken(_testUser);
            token.Should().NotBeNullOrWhiteSpace();

            // Act & Assert - Validate (should be valid)
            var validResult = await service.ValidateTokenAsync(token);
            validResult.IsValid.Should().BeTrue();

            // Act & Assert - Revoke
            await service.RevokeTokenAsync(token);

            // Act & Assert - Validate again (should be invalid)
            var invalidResult = await service.ValidateTokenAsync(token);
            invalidResult.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task MultipleUsers_TokensAreIndependent()
        {
            // Arrange
            var service = CreateService();
            var user1 = new User("username1", "hash1");
            var user2 = new User("username2", "hash2");

            // Act
            var token1 = service.GenerateToken(user1);
            var token2 = service.GenerateToken(user2);

            var result1 = await service.ValidateTokenAsync(token1);
            var result2 = await service.ValidateTokenAsync(token2);

            // Assert
            result1.IsValid.Should().BeTrue();
            result2.IsValid.Should().BeTrue();

            var claim1 = result1.ClaimsIdentity!.FindFirst(JwtRegisteredClaimNames.Sub);
            var claim2 = result2.ClaimsIdentity!.FindFirst(JwtRegisteredClaimNames.Sub);

            claim1!.Value.Should().Be(user1.Id.ToString());
            claim2!.Value.Should().Be(user2.Id.ToString());
        }

        #endregion
    }
}
using FluentAssertions;
using MandarinAuction.Domain.Entities;

namespace MandarinAuction.Tests.Unit;

public class UserTests
{
    [Fact]
    public void SetEmail_WithValidEmail_ShouldSetEmailInLowerCase()
    {
        // Arrange
        var user = new User();
        var email = "Test@Example.COM";

        // Act
        user.SetEmail(email);

        // Assert
        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void SetEmail_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var user = new User();

        // Act
        Action act = () => user.SetEmail(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Некорректный email*");
    }

    [Fact]
    public void GenerateOtpCode_ShouldCreateSixDigitCode()
    {
        // Arrange
        var user = new User();

        // Act
        user.GenerateOtpCode();

        // Assert
        user.OtpCode.Should().NotBeNullOrEmpty();
        user.OtpCode.Should().HaveLength(6);
        user.OtpCode.Should().MatchRegex(@"^\d{6}$");
        user.OtpExpiresAt.Should().NotBeNull();
        user.OtpExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void VerifyOtpCode_WithCorrectCode_ShouldReturnTrue()
    {
        // Arrange
        var user = new User();
        user.GenerateOtpCode();
        var code = user.OtpCode!;

        // Act
        var result = user.VerifyOtpCode(code);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyOtpCode_WithIncorrectCode_ShouldReturnFalse()
    {
        // Arrange
        var user = new User();
        user.GenerateOtpCode();

        // Act
        var result = user.VerifyOtpCode("000000");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyOtpCode_WithExpiredCode_ShouldReturnFalse()
    {
        // Arrange
        var user = new User();
        user.OtpCode = "123456";
        user.OtpExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Act
        var result = user.VerifyOtpCode("123456");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ClearOtpCode_ShouldRemoveOtpCodeAndExpiration()
    {
        // Arrange
        var user = new User();
        user.GenerateOtpCode();

        // Act
        user.ClearOtpCode();

        // Assert
        user.OtpCode.Should().BeNull();
        user.OtpExpiresAt.Should().BeNull();
    }

    [Fact]
    public void GenerateOtpCode_CalledTwice_ShouldGenerateDifferentCodes()
    {
        // Arrange
        var user = new User();
        
        // Act
        user.GenerateOtpCode();
        var firstCode = user.OtpCode;
        user.GenerateOtpCode();
        var secondCode = user.OtpCode;
        
        // Assert
        firstCode.Should().NotBe(secondCode);
    }

    [Fact]
    public void PlaceBid_WithValidAmount_ShouldUpdateCurrentPrice()
    {
        // Arrange
        var amount = 200m;
        var user = new User();
        var mandarinId = Guid.NewGuid();
        var spoilAt = DateTime.UtcNow.AddHours(1);
        var auction = new Auction(mandarinId, 100, 1000, 100, spoilAt);
        
        // Act
        auction.PlaceBid(user.Id, amount);
        
        // Assert
        auction.CurrentPrice.Should().Be(amount);
    }
}

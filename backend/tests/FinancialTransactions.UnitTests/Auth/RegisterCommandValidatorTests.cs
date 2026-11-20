using FinancialTransactions.Application.Auth.Commands.Register;

namespace FinancialTransactions.UnitTests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator validator = new();

    [Fact]
    public void ValidCommand_Passes()
    {
        Assert.True(validator.Validate(ValidCommand()).IsValid);
    }

    [Theory]
    [InlineData(nameof(RegisterCommand.FirstName), "A")]
    [InlineData(nameof(RegisterCommand.LastName), "")]
    [InlineData(nameof(RegisterCommand.NationalId), "12345678951")]
    [InlineData(nameof(RegisterCommand.Email), "not-an-email")]
    [InlineData(nameof(RegisterCommand.Phone), "05321234567")]
    [InlineData(nameof(RegisterCommand.Password), "short1")]
    [InlineData(nameof(RegisterCommand.Password), "onlyletters")]
    [InlineData(nameof(RegisterCommand.Password), "12345678")]
    public void InvalidField_Fails(string field, string value)
    {
        var command = field switch
        {
            nameof(RegisterCommand.FirstName) => ValidCommand() with { FirstName = value },
            nameof(RegisterCommand.LastName) => ValidCommand() with { LastName = value },
            nameof(RegisterCommand.NationalId) => ValidCommand() with { NationalId = value },
            nameof(RegisterCommand.Email) => ValidCommand() with { Email = value },
            nameof(RegisterCommand.Phone) => ValidCommand() with { Phone = value },
            _ => ValidCommand() with { Password = value },
        };

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == field);
    }

    [Fact]
    public void PasswordUpTo72Bytes_Passes()
    {
        Assert.True(validator.Validate(ValidCommand() with { Password = new string('a', 71) + "1" }).IsValid);
        Assert.True(validator.Validate(ValidCommand() with { Password = new string('ş', 34) + "Ab1" }).IsValid);
    }

    [Theory]
    [InlineData(73, 'a')]
    [InlineData(40, 'ş')]
    public void PasswordOver72Bytes_FailsWithClearMessage(int repeat, char letter)
    {
        var result = validator.Validate(ValidCommand() with { Password = new string(letter, repeat) + "Gizli1" });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RegisterCommand.Password), error.PropertyName);
        Assert.StartsWith("Şifre çok uzun", error.ErrorMessage);
    }

    private static RegisterCommand ValidCommand() =>
        new("Ayşe", "Yılmaz", "12345678950", "ayse@example.com", "5321234567", "Sifre1234");
}

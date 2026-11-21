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
    [InlineData(nameof(RegisterCommand.Password), "ab1")]
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

    [Theory]
    [InlineData("abc123")]
    [InlineData("Sifre12345678901")]
    [InlineData("şifre1")]
    public void PasswordBetween6And16Characters_Passes(string password)
    {
        Assert.True(validator.Validate(ValidCommand() with { Password = password }).IsValid);
    }

    [Theory]
    [InlineData("ab12")]
    [InlineData("Sifre123456789012")]
    public void PasswordOutside6To16Characters_FailsWithClearMessage(string password)
    {
        var result = validator.Validate(ValidCommand() with { Password = password });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RegisterCommand.Password), error.PropertyName);
        Assert.Equal("Şifre 6 ile 16 karakter arasında olmalı", error.ErrorMessage);
    }

    private static RegisterCommand ValidCommand() =>
        new("Ayşe", "Yılmaz", "12345678950", "ayse@example.com", "5321234567", "Sifre1234");
}

namespace Iccu.Application.Common.Validation;

using FluentValidation;
using Iccu.Domain.Users;
using System.Text.RegularExpressions;
using Iccu.Application.Common.Extensions;

internal static partial class UserValidationRules
{
    private const int PasswordMinLength = 8;
    private const int PasswordMaxLength = 128;
    private const int FullNameMaxLength = 150;

    public static IRuleBuilderOptions<T, string> ValidUsername<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(username => UsernamePattern().IsMatch(username.Trim()))
            .WithError(UserErrors.InvalidUsername);

    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(FullNameMaxLength);

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MaximumLength(PasswordMaxLength)
            .Must(password => password.Length >= PasswordMinLength && password.Any(char.IsLetter) && password.Any(char.IsDigit))
            .WithError(UserErrors.WeakPassword);

    [GeneratedRegex("^[A-Za-z0-9._-]{3,50}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex UsernamePattern();
}

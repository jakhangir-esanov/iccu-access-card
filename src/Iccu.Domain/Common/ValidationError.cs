namespace Iccu.Domain.Common;

public sealed record ValidationError : Error
{
    private static readonly LocalizedMessage ValidationMessages = new(
        "One or more validation errors occurred",
        "Bir yoki bir nechta maydon noto'g'ri to'ldirilgan",
        "Одно или несколько полей заполнены неверно");

    public ValidationError(Error[] errors)
        : base("General.Validation", ValidationMessages, ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }
}

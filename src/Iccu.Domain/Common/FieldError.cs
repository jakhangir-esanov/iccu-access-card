namespace Iccu.Domain.Common;

public sealed record FieldError : Error
{
    public FieldError(string code, LocalizedMessage messages, string propertyName)
        : base(code, messages, ErrorType.Problem)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}

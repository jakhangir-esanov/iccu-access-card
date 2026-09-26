namespace Iccu.Application.Common.Validation;

using Iccu.Domain.Common;

public static class PersonDetailsNormalizer
{
    public static PersonDetails Normalized(this PersonDetails details) => details with
    {
        LastName = details.LastName.Trim(),
        FirstName = details.FirstName.Trim(),
        MiddleName = string.IsNullOrWhiteSpace(details.MiddleName) ? null : details.MiddleName.Trim(),
        Phone = PhoneNumber.Normalize(details.Phone) ?? details.Phone.Trim(),
        DocumentNumber = DocumentNumber.Normalize(details.DocumentNumber)
    };
}

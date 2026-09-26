namespace Iccu.Domain.Common;

public record Error
{
    public static readonly Error None = new(string.Empty, LocalizedMessage.Empty, ErrorType.Failure);

    public static readonly Error NullValue = Failure(
        "General.Null",
        en: "Null value was provided",
        uz: "Bo'sh qiymat berildi",
        ru: "Передано пустое значение");

    public static readonly Error Unexpected = Failure(
        "General.Unexpected",
        en: "An unexpected error occurred",
        uz: "Kutilmagan xatolik yuz berdi",
        ru: "Произошла непредвиденная ошибка");

    public static readonly Error DuplicateKey = Conflict(
        "Conflict.DuplicateKey",
        en: "The record was written by another request at the same time. Try again.",
        uz: "Yozuv shu vaqtning o'zida boshqa so'rov tomonidan yozildi. Qaytadan urinib ko'ring.",
        ru: "Запись была одновременно изменена другим запросом. Попробуйте ещё раз.");

    public Error(string code, LocalizedMessage messages, ErrorType type)
    {
        Code = code;
        Messages = messages;
        Type = type;
    }

    public string Code { get; }

    public string Message => Messages.En;

    public LocalizedMessage Messages { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.Failure);

    public static Error NotFound(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.NotFound);

    public static Error Problem(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.Problem);

    public static Error Conflict(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.Conflict);

    public static Error Unauthorized(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.Unauthorized);

    public static Error Forbidden(string code, string en, string uz, string ru) =>
        new(code, new LocalizedMessage(en, uz, ru), ErrorType.Forbidden);
}

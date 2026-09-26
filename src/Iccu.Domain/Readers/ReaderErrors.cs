namespace Iccu.Domain.Readers;

using Iccu.Domain.Common;

public static class ReaderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Reader.NotFound",
        en: "The reader was not found.",
        uz: "Kitobxon topilmadi.",
        ru: "Читатель не найден.");

    public static readonly Error DocumentAlreadyRegistered = Error.Conflict(
        "Reader.DocumentAlreadyRegistered",
        en: "A reader with this document is already registered. Reprint the existing card instead of creating a new one.",
        uz: "Bu hujjat bilan kitobxon allaqachon ro'yxatdan o'tgan. Yangi karta ochish o'rniga mavjud kartani qayta chop eting.",
        ru: "Читатель с этим документом уже зарегистрирован. Перепечатайте существующую карту вместо создания новой.");

    public static readonly Error InvalidPhone = Error.Problem(
        "Reader.InvalidPhone",
        en: "The phone number must be an Uzbek number: +998 XX XXX XX XX.",
        uz: "Telefon raqami O'zbekiston raqami bo'lishi kerak: +998 XX XXX XX XX.",
        ru: "Номер телефона должен быть узбекским: +998 XX XXX XX XX.");

    public static readonly Error InvalidPassport = Error.Problem(
        "Reader.InvalidPassport",
        en: "The passport or ID card number must be two letters followed by seven digits, for example AA1234567.",
        uz: "Pasport yoki ID karta raqami ikki harf va yetti raqamdan iborat bo'lishi kerak, masalan AA1234567.",
        ru: "Номер паспорта или ID-карты должен состоять из двух букв и семи цифр, например AA1234567.");

    public static readonly Error InvalidBirthCertificate = Error.Problem(
        "Reader.InvalidBirthCertificate",
        en: "The birth certificate number must contain 6 to 20 letters and digits.",
        uz: "Tug'ilganlik haqidagi guvohnoma raqami 6 tadan 20 tagacha harf va raqamdan iborat bo'lishi kerak.",
        ru: "Номер свидетельства о рождении должен содержать от 6 до 20 букв и цифр.");

    public static readonly Error InvalidBirthDate = Error.Problem(
        "Reader.InvalidBirthDate",
        en: "The birth date must be in the past and not earlier than 1900.",
        uz: "Tug'ilgan sana o'tgan kunlardan biri bo'lishi va 1900-yildan oldin bo'lmasligi kerak.",
        ru: "Дата рождения должна быть в прошлом и не ранее 1900 года.");
}

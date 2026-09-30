namespace Iccu.Domain.Readers;

using Iccu.Domain.Common;

public static class ReaderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Reader.NotFound",
        en: "The reader was not found.",
        uz: "Kitobxon topilmadi.",
        ru: "Читатель не найден.");

    public static readonly Error PhoneAlreadyRegistered = Error.Conflict(
        "Reader.PhoneAlreadyRegistered",
        en: "A reader with this phone number is already registered. Reprint the existing card instead of creating a new one.",
        uz: "Bu telefon raqami bilan kitobxon allaqachon ro'yxatdan o'tgan. Yangi karta ochish o'rniga mavjud kartani qayta chop eting.",
        ru: "Читатель с этим номером телефона уже зарегистрирован. Перепечатайте существующую карту вместо создания новой.");

    public static readonly Error InvalidPhone = Error.Problem(
        "Reader.InvalidPhone",
        en: "The phone number must be an Uzbek number: +998 XX XXX XX XX.",
        uz: "Telefon raqami O'zbekiston raqami bo'lishi kerak: +998 XX XXX XX XX.",
        ru: "Номер телефона должен быть узбекским: +998 XX XXX XX XX.");

    public static readonly Error InvalidInternationalPhone = Error.Problem(
        "Reader.InvalidInternationalPhone",
        en: "The phone number must be in international format: the country code and number, 8 to 15 digits in total.",
        uz: "Telefon raqami xalqaro formatda bo'lishi kerak: davlat kodi va raqam, jami 8 tadan 15 tagacha raqam.",
        ru: "Номер телефона должен быть в международном формате: код страны и номер, всего от 8 до 15 цифр.");

    public static readonly Error InvalidBirthDate = Error.Problem(
        "Reader.InvalidBirthDate",
        en: "The birth date must be in the past and not earlier than 1900.",
        uz: "Tug'ilgan sana o'tgan kunlardan biri bo'lishi va 1900-yildan oldin bo'lmasligi kerak.",
        ru: "Дата рождения должна быть в прошлом и не ранее 1900 года.");
}

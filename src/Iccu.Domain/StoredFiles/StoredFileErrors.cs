namespace Iccu.Domain.StoredFiles;

using Iccu.Domain.Common;

public static class StoredFileErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StoredFile.NotFound",
        en: "The specified file was not found.",
        uz: "Ko'rsatilgan fayl topilmadi.",
        ru: "Указанный файл не найден.");

    public static readonly Error InUse = Error.Conflict(
        "StoredFile.InUse",
        en: "The file is still used by a reader or a registration request and cannot be deleted.",
        uz: "Fayl hali kitobxon yoki arizada ishlatilmoqda, uni o'chirib bo'lmaydi.",
        ru: "Файл всё ещё используется читателем или заявкой и не может быть удалён.");

    public static readonly Error Empty = Error.Problem(
        "StoredFile.Empty",
        en: "The uploaded file is empty.",
        uz: "Yuklangan fayl bo'sh.",
        ru: "Загруженный файл пуст.");

    public static readonly Error UnsupportedContent = Error.Problem(
        "StoredFile.UnsupportedContent",
        en: "Only JPEG, PNG or WebP photos are accepted.",
        uz: "Faqat JPEG, PNG yoki WebP rasm qabul qilinadi.",
        ru: "Принимаются только фотографии JPEG, PNG или WebP.");

    public static readonly Error TooLarge = Error.Problem(
        "StoredFile.TooLarge",
        en: "The photo must not exceed 8 MB.",
        uz: "Rasm hajmi 8 MB dan oshmasligi kerak.",
        ru: "Размер фотографии не должен превышать 8 МБ.");

    public static readonly Error ContentMismatch = Error.Problem(
        "StoredFile.ContentMismatch",
        en: "The file contents do not match its extension.",
        uz: "Fayl mazmuni uning kengaytmasiga mos kelmaydi.",
        ru: "Содержимое файла не соответствует его расширению.");
}

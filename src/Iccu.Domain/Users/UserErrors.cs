namespace Iccu.Domain.Users;

using Iccu.Domain.Common;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        en: "The user was not found.",
        uz: "Foydalanuvchi topilmadi.",
        ru: "Пользователь не найден.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "User.InvalidCredentials",
        en: "The username or password is incorrect.",
        uz: "Login yoki parol noto'g'ri.",
        ru: "Неверный логин или пароль.");

    public static readonly Error LockedOut = Error.Forbidden(
        "User.LockedOut",
        en: "Too many failed attempts. The account is locked for 15 minutes.",
        uz: "Juda ko'p noto'g'ri urinish. Hisob 15 daqiqaga bloklandi.",
        ru: "Слишком много неудачных попыток. Учётная запись заблокирована на 15 минут.");

    public static readonly Error Inactive = Error.Forbidden(
        "User.Inactive",
        en: "The account is deactivated. Contact the administrator.",
        uz: "Hisob o'chirilgan. Administratorga murojaat qiling.",
        ru: "Учётная запись отключена. Обратитесь к администратору.");

    public static readonly Error UsernameTaken = Error.Conflict(
        "User.UsernameTaken",
        en: "This username is already taken.",
        uz: "Bu login band.",
        ru: "Этот логин уже занят.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "User.InvalidRefreshToken",
        en: "The session has expired. Sign in again.",
        uz: "Sessiya muddati tugadi. Qaytadan kiring.",
        ru: "Сессия истекла. Войдите снова.");

    public static readonly Error WrongCurrentPassword = Error.Problem(
        "User.WrongCurrentPassword",
        en: "The current password is incorrect.",
        uz: "Joriy parol noto'g'ri.",
        ru: "Текущий пароль неверен.");

    public static readonly Error WeakPassword = Error.Problem(
        "User.WeakPassword",
        en: "The password must be at least 8 characters long and contain both letters and digits.",
        uz: "Parol kamida 8 belgidan iborat bo'lishi hamda harf va raqamlarni o'z ichiga olishi kerak.",
        ru: "Пароль должен содержать не менее 8 символов, включая буквы и цифры.");

    public static readonly Error InvalidUsername = Error.Problem(
        "User.InvalidUsername",
        en: "The username may contain only Latin letters, digits, dots, hyphens and underscores (3 to 50 characters).",
        uz: "Login faqat lotin harflari, raqamlar, nuqta, chiziqcha va pastki chiziqdan iborat bo'lishi mumkin (3-50 belgi).",
        ru: "Логин может содержать только латинские буквы, цифры, точки, дефисы и подчёркивания (от 3 до 50 символов).");

    public static readonly Error CannotDemoteSelf = Error.Conflict(
        "User.CannotDemoteSelf",
        en: "You cannot deactivate your own account or remove your own administrator role.",
        uz: "O'z hisobingizni o'chira olmaysiz yoki o'zingizdan administrator rolini ololmaysiz.",
        ru: "Нельзя отключить собственную учётную запись или снять с себя роль администратора.");
}

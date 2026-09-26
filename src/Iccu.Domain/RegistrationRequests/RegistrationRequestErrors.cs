namespace Iccu.Domain.RegistrationRequests;

using Iccu.Domain.Common;

public static class RegistrationRequestErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "RegistrationRequest.NotFound",
        en: "The registration request was not found.",
        uz: "Ro'yxatdan o'tish arizasi topilmadi.",
        ru: "Заявка на регистрацию не найдена.");

    public static readonly Error NotPending = Error.Conflict(
        "RegistrationRequest.NotPending",
        en: "The registration request has already been reviewed or has expired.",
        uz: "Ariza allaqachon ko'rib chiqilgan yoki muddati o'tgan.",
        ru: "Заявка уже рассмотрена или срок её действия истёк.");

    public static readonly Error ConsentRequired = Error.Problem(
        "RegistrationRequest.ConsentRequired",
        en: "Consent to the processing of personal data is required.",
        uz: "Shaxsga doir ma'lumotlarni qayta ishlashga rozilik berish majburiy.",
        ru: "Необходимо согласие на обработку персональных данных.");

    public static readonly Error Expired = Error.Conflict(
        "RegistrationRequest.Expired",
        en: "The registration request has expired. Ask the visitor to fill in the form again.",
        uz: "Arizaning muddati o'tgan. Tashrif buyuruvchidan anketani qayta to'ldirishni so'rang.",
        ru: "Срок действия заявки истёк. Попросите посетителя заполнить анкету заново.");
}

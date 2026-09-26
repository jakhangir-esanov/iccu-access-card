# ICCU — Arxitektura

Oddiy (modulsiz) Clean Architecture, CQRS va MediatR. Uslub `tofan`dan olingan, farqlari quyida yozilgan.

## Qatlamlar

```text
Iccu.Api                 Program.cs, Swagger (Swashbuckle), CORS, middleware — tofan'ning Api loyihasi bilan bir xil
  └─ Iccu.Infrastructure   EF Core, Dapper, JWT, ObjectStorage, Hangfire, SignalR, rate limiting va barcha DI
        ├─ Iccu.Presentation   faqat endpoint'lar (IEndpoint), Requests/, ApiResults, SignalR hub
        └─ Iccu.Application   command/query handler'lar (biznes mantiq shu yerda), validator'lar, behavior'lar, Dapper SQL
              └─ Iccu.Domain      Common + har bir entity uchun papka: entity, Errors, IRepository
```

Bog'lanish yo'nalishi `test/Iccu.ArchitectureTests` bilan majburlanadi. Qoida buzilsa, build testlardan o'tmaydi.

## API va Presentation

`tofan`ning Api va Presentation qatlamlari bilan bir xil:

- **Yo'llar `/api` prefiksisiz** (`auth/login`, `readers`, `files/{id}/content`, `hubs/registrations`). `/api`ni nginx olib tashlaydi, dev'da frontend proxy (`pathRewrite`) qiladi.
- **Muvaffaqiyatli javob `Result`ga o'ralgan**: `{ "isSuccess": true, "data": ... }`. Ma'lumotsiz command ham `200` va `Result` qaytaradi. Sahifalangan ro'yxatlar (`PagedList`: `{ "data": [...], "totalCount": n }`) o'ralmaydi. Sahifalash parametrlari `tofan`dagidek: `first`, `rows`, `sortField` (snake_case), `sortOrder` (1 yoki -1). Xatolar ProblemDetails + `messages` (uz/ru/en), validatsiyada `errors`.
- **Enum'lar raqam** sifatida keladi va ketadi. Swagger'da nomlari `x-enumNames`da ko'rinadi.
- **Endpoint** — `IEndpoint` klassi, so'rov modeli `Requests/` papkasida alohida `internal sealed record`. `.Produces<Result<T>>`, `.WithTags("...")`, `CancellationToken`siz.
- **DI**: `tofan`dagi `StorageModule` kabi Infrastructure endpoint'larni (`AddEndpoints(Presentation.AssemblyReference.Assembly)`), rate limiting, SignalR, forwarded headers va so'rov hajmi limitini ro'yxatga oladi. `Program.cs` faqat `AddInfrastructure` va `MapEndpoints` chaqiradi.

ICCU'ga xos, `tofan`da yo'q qismlar: refresh token HttpOnly cookie'da, login va ochiq endpoint'larda rate limiting, SignalR, forwarded headers. 500 xatoda `exception.Message` javobga chiqarilmaydi (`tofan`dan farqli).

Har bir qatlamdagi umumiy qism `Common` papkasida turadi, masalan `Domain/Common`, `Application/Common`, `Presentation/Common`. Domain'dagi barcha enum'lar `Domain/Common/Enums` ichida.

## Domain

Har bir entity o'z papkasida, yonida faqat o'ziga tegishli ikki fayl turadi:

```text
Iccu.Domain/
  Common/                 Result, Error, ValidationError, PersonDetails, Enums/
  Readers/                Reader, ReaderErrors, IReaderRepository
  RegistrationRequests/   RegistrationRequest, RegistrationRequestErrors, IRegistrationRequestRepository
  Users/                  User, UserErrors, IUserRepository
  RefreshTokens/          RefreshToken, IRefreshTokenRepository
```

Entity'lar `sealed`, public konstruktori yo'q, faqat factory va holatni o'zgartiruvchi metodlari bor. Ular `Result` qaytarmaydi va qaror qabul qilmaydi. Bularni `DomainTests` tekshiradi.

## Biznes mantiq

Barcha qoidalar tegishli command yoki query handler'ining ichida yoziladi: karta muddati (2 yil), karta raqami formati (`D7`), login bloklash (5 urinish, 15 daqiqa), refresh token rotatsiyasi, ariza muddati (24 soat) va hokazo. Qiymatlar handler ichidagi `private const` sifatida turadi. `CardNumber`, `LoginPolicy`, `ReaderFilter` kabi alohida yordamchi klasslar ataylab yo'q.

Yagona istisno — bir nechta handler aynan bir xil ishlatadigan kod. Masalan `ExportReaders` filtr SQL'ini `GetReadersQueryHandler.ReadersCte` va `BuildFilter`dan oladi.

Foydalanuvchi tushunchasi bitta: `User` (`Admin` yoki `Receptionist`). Alohida "staff" tushunchasi yo'q.

## Infrastructure

`tofan`ning Common.Infrastructure va modul Infrastructure qatlamlari bilan bir xil tuzilgan:

- `InfrastructureConfiguration.AddInfrastructure` avval umumiy qismni (`AddAuthenticationInternal`, `DateTimeProvider`, `NpgsqlDataSource`, health check, `DbConnectionFactory`, Dapper type handler'lar), keyin modul qismini (DbContext, repository'lar, options, Hangfire, SignalR, rate limiting) va endpoint'larni ro'yxatga oladi.
- **Authentication**: `AuthenticationExtensions`, `JwtBearerConfigureOptions` (`IConfigureNamedOptions<JwtBearerOptions>`), `CustomClaims`, `ClaimsPrincipalExtensions.GetUserId()`, `CurrentUser`. `JwtOptions` ishga tushishda tekshiriladi (`ValidateOnStart`, kalit kamida 32 bayt).
- **Database**: `ApplicationDbContext`da `DbSet { get; set; }` va har bir konfiguratsiya `ApplyConfiguration(new ...)` bilan ulanadi. Konfiguratsiyalar `x =>` bilan, `ToTable`siz yoziladi: jadval nomi `DbSet` nomidan snake_case bo'lib chiqadi.
- **ObjectStorage**: `LocalDiskFileStore`, `StorageOptions` (`Storage:RootPath`).
- **BackgroundJobs**: Hangfire (PostgreSQL storage, `hangfire` sxemasi). Har bir vazifa `XJob` + `XJobScheduler` (`IHostedService`, cron) juftligi: `ExpireRegistrationRequestsJob` har 15 daqiqada, `DeleteUnusedFilesJob` har soatda. Job faqat MediatR command'ini yuboradi, biznes mantiq command ichida qoladi. Dashboard `/hangfire`, faqat Development'da yoqiladi.
- **Configuration**: `GetConnectionStringOrThrow`. Serverda connection string va JWT kaliti `tofan`dagidek environment variable orqali keladi (`deploy/stacks/api.yml`).

## Yozish va o'qish

| | Texnologiya | Qayerda |
|---|---|---|
| Yozish | EF Core, repository va `IUnitOfWork` | Command handler'lar |
| O'qish | Dapper, `IDbConnectionFactory` | Query handler'lar. SQL handler faylining o'zida turadi |

Query javoblari `tofan`dagidek positional record'lar (`sealed record UserResponse(Guid Id, ...)`). Dapper ularni konstruktor orqali to'ldiradi: SELECT'dagi ustunlar tartibi va turi record parametrlariga aynan mos bo'lishi kerak (`COUNT(*)::int`, `DateOnly` uchun type handler).

## Application

`tofan`dagidek ikkiga bo'lingan:

- `Common/` — `tofan`ning Common.Application'i: Behaviors, Clock, Data (`IDbConnectionFactory`), Export, Extensions (`RuleBuilderExtensions`, `SortColumnExtensions`), Messaging (`ICommand`, `IQuery`, `IPagedListQuery`), Paging (`PagingRequest<T>`, `PagedList<T>`). Bundan tashqari ICCU'ga xos `Validation/`.
- `Abstractions/` — modulga xos interfeyslar: Authentication (`ICurrentUser.UserId`, `IPasswordHasher`, `ITokenService`, `Policies`), Data (`IUnitOfWork`), Storage (`IFileStore`), Notifications.

Handler parametri har doim `request`, validator'lar `x =>` bilan yoziladi. Ro'yxat query'lari `tofan`dagi `GetSoldiersQuery` shaklida: CTE, `List<string> conditions`, `dataSql` + `countSql`, `ORDER BY {paging.SortField} {paging.SortDirection}`, `OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY`. Filtr maydonlari alohida `XFilter` record'siz, query record'ining o'zida turadi. `ExportReaders` filtrni `GetReadersQueryHandler.BuildFilter`dan oladi. DI (MediatR, behavior'lar, validator'lar) Infrastructure'da qoladi.

## SQL va Sonar S2077

WHERE shartlari va tartiblash `tofan`dagidek matn birlashtirish bilan yig'iladi, shuning uchun `.editorconfig`da S2077 o'chirilgan. Qidiruv so'zlarga bo'lish SQL'ning o'zida (`regexp_split_to_table`, `LIKE ALL`). Bu xavfsiz, chunki ikki qoida doim bajariladi:

1. **SQL'ga faqat koddagi o'zgarmas qismlar qo'shiladi.** Foydalanuvchi yuborgan matn SQL'ning o'ziga hech qachon tushmaydi.
2. **Barcha qiymatlar parametr orqali beriladi.** Tartiblash ustuni `PagingRequest<T>` ichida javob record'ining property nomlaridan (snake_case) tuzilgan oq ro'yxatdan tanlanadi. Noma'lum ustun kelsa, `id` ishlatiladi. Bu `PagingRequestTests`da SQL injection urinishi bilan tekshirilgan.

Yangi dinamik so'rov yozishda ham shu ikki qoidaga amal qilinadi.

## Pipeline

MediatR (12.5, Apache 2.0). `ICommand`, `IQuery`, `IPagedListQuery` va ularning handler'lari MediatR'ning `IRequest`/`IRequestHandler` interfeyslari ustiga qurilgan. Endpoint'lar `ISender` orqali yuboradi. Ikkita behavior `AddOpenBehavior` bilan shu tartibda ro'yxatga olingan:

1. `RequestLoggingPipelineBehavior` — faqat `Result` qaytaradigan so'rovlar uchun; xato bilan tugagan so'rov `Error` darajasida yoziladi (`tofan` kabi).
2. `ValidationPipelineBehavior` — FluentValidation natijasini `ValidationError`ga aylantiradi. `tofan`dan farqli ravishda query'lar ham tekshiriladi (hisobot davri validator'i uchun).

Kutilmagan xatolar behavior'da ushlanmaydi: ularni Api'dagi `GlobalExceptionHandler` bir marta log qiladi va 500 (unique index buzilsa 409) qaytaradi.

Application qatlamida DI konfiguratsiyasi yo'q. MediatR, behavior'lar va validator'lar `InfrastructureConfiguration.AddMessaging()`da `AssemblyReference.Assembly` orqali ro'yxatga olinadi. Handler va validator'lar `internal`.

## Karta raqami

- `iccu.card_number_seq` sequence'i, 1 dan 9 999 999 gacha, hech qachon reset qilinmaydi.
- Raqam `Reader` insert qilinganda bazaning o'zida beriladi (`DEFAULT nextval`). QR arizalar faqat tasdiqlanganda raqam oladi.
- Kitobxonlar yumshoq o'chiriladi (`deleted_at`), shuning uchun raqam boshqa odamga qayta berilmaydi.
- Bitta hujjatga faqat bitta faol kitobxon mumkin: `(document_type, document_number) WHERE deleted_at IS NULL` ustida unique index. Ikki so'rov bir vaqtda kelsa, 409 `Conflict.DuplicateKey` qaytadi.

## Qidiruv

`readers.search_text` — stored generated ustun: familiya, ism va otasining ismi kichik harflarda, apostrof variantlari (`‘ ’ ʻ ʼ`) oddiy `'` ga keltirilgan. Ustiga `pg_trgm` GIN index qo'yilgan. Qidiruv matnidagi har bir so'z `LIKE %so'z%` shartiga aylanadi.

Qidiruv matni son yoki hujjat ko'rinishida bo'lsa, qo'shimcha ravishda karta raqami, telefon va hujjat bo'yicha ham izlanadi.

## Fayllar (rasmlar)

`tofan`dagi Storage moduli bilan bir xil ishlaydi:

1. **Yuklash.** Frontend rasmni avval alohida yuklaydi va `Guid` oladi: xodim `POST files` (avtorizatsiya bilan), QR anketa to'ldiruvchi `POST public/files` (anonim, rate limit bilan). `UploadFileCommand` kengaytmani (`.jpg`, `.jpeg`, `.png`, `.webp`), hajmni (8 MB) va magic-byte imzosini tekshiradi. Keyin faylni o'zgartirmasdan `IFileStore`ga yozadi va `stored_files` jadvaliga yozuv qo'shadi.
2. **Bog'lash.** `POST readers`, `PUT readers/{id}` va `POST public/registrations` JSON qabul qiladi, rasm `PhotoFileId` orqali beriladi. Handler fayl mavjudligini tekshiradi (`StoredFile.NotFound`).
3. **O'qish.** `GET files/{id}/content` faylni oqim sifatida beradi. Pasport rasmlari bo'lgani uchun faqat avtorizatsiya bilan ochiladi (`tofan`da bu endpoint ochiq).
4. **O'chirish.** Admin `DELETE files/{id}` bilan o'chira oladi, lekin fayl ishlatilayotgan bo'lsa `StoredFile.InUse` qaytadi.
5. **Tozalash.** "Ishlatilmoqda" degani: o'chirilmagan kitobxonning rasmi yoki Pending arizaning rasmi. Hangfire `DeleteUnusedFilesJob` har soatda `DeleteUnusedFilesCommand`ni yuboradi. U 24 soatdan eski va hech qayerda ishlatilmayotgan fayllarni o'chiradi: almashtirilgan, rad etilgan, muddati o'tgan va hech qachon biriktirilmagan rasmlar.

Rasm qayta ishlanmaydi (SkiaSharp yo'q). Kesish va o'lchamni frontend qiladi, EXIF metadata fayl ichida qoladi.

## Autentifikatsiya

- **Access token.** JWT (HS256), 15 daqiqa amal qiladi. Claim'lar: `sub`, `name`, `role`, `full_name`.
- **Refresh token.** Kuchli tasodifiy qiymat, `iccu_refresh` nomli HttpOnly + Secure + SameSite=Strict cookie'da, `Path=/api/auth` (brauzer ko'radigan manzil; server ichida yo'l `auth/...`). Bazada faqat uning SHA-256 hash'i saqlanadi. Har yangilanishda rotatsiya qilinadi. Allaqachon almashtirilgan token qayta ishlatilsa, foydalanuvchining barcha sessiyalari bekor qilinadi.
- **Parol.** ASP.NET `PasswordHasher` (PBKDF2). 5 ta xato urinishdan keyin hisob 15 daqiqaga bloklanadi. Login mavjud bo'lmasa ham parol tekshiruviga xuddi shuncha vaqt sarflanadi, shunda javob vaqtidan login borligini bilib bo'lmaydi.
- **Rollar.** `Admin` va `Receptionist`. Policy'lar: `User` (ikkala rol) va `Admin`. Token `JwtTokenService` (`ITokenService`) tomonidan beriladi.

## Real-time

`hubs/registrations` (SignalR, tashqaridan `/api/hubs/registrations`). Yangi ariza kelganda barcha foydalanuvchilarga `RegistrationSubmitted` xabari yuboriladi. Access token query string orqali uzatiladi (`access_token`). Xabar yuborilmay qolsa, faqat log yoziladi, ariza baribir saqlanadi.

## Kim ro'yxatga olgani

Faqat kim ro'yxatga olgani saqlanadi: `readers.created_by` (resepshn) va `registration_requests.reviewed_by` (QR arizani ko'rib chiqqan foydalanuvchi). Ular "foydalanuvchilar kesimida" hisobot uchun kerak. Audit log hamda "kim tahrirlagan" yoki "kim o'chirgan" ma'lumoti ataylab yo'q: ular bu loyiha uchun ortiqcha deb topildi.

## Vaqt

Barcha vaqtlar UTC'da (`timestamptz`) saqlanadi. Bugungi sana va kun chegaralari `Clock:TimeZone` (standart `Asia/Tashkent`) bo'yicha hisoblanadi. SQL'da mahalliy kunlar bo'yicha guruhlash `AT TIME ZONE @TimeZone` orqali qilinadi.

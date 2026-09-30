# ICCU — Frontend uchun backend shartnomasi

Bu hujjat ICCU frontend'i (React) uchun backend API'ning to'liq shartnomasi. U `main` dagi koddan va 2026-09-26 da ishlab turgan API'dan olingan haqiqiy javoblar asosida yozilgan. Frontend repo'sida `docs/backend-contract.md` sifatida saqlanadi. Backend o'zgarsa, shu hujjat va Swagger (`/swagger/v1/swagger.json`, faqat Development'da) bilan solishtiriladi.

DTO'lar qo'lda yoziladi. Swagger'dagi generic sxema nomlari (`PagedList`1[[Iccu.Application...`) codegen uchun yaroqsiz, API esa kichik (28 operatsiya).

---

## 1. Asos

| | |
|---|---|
| Brauzer ko'radigan manzil | `/api/...` (nginx `/api` ni olib tashlab `iccu-api:8080` ga uzatadi) |
| Backend ichidagi yo'l | `/...` (`/api` prefiksisiz: `GET /readers`) |
| Dev'da backend | `http://localhost:5080` |
| JSON | camelCase |
| Enum'lar | butun son (3-bo'lim) |
| Sana (`DateOnly`) | `"2026-09-26"` |
| Vaqt (`DateTime`) | UTC, ISO 8601: `"2026-09-26T14:17:20.417635Z"`. Ekranda `Asia/Tashkent` da ko'rsatiladi |
| Id | `Guid` satr |
| Karta raqami | 7 xonali satr: `"0000001"` |
| Ariza kodi | 4 xonali satr: `"0001"` |

### Dev proxy (majburiy)

Refresh cookie `Path=/api/auth` va `SameSite=Strict` bilan beriladi. Shuning uchun frontend dev'da ham API'ga **o'z origin'i orqali, `/api` bilan** murojaat qilishi kerak. Vite proxy:

```ts
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5080',
      changeOrigin: true,
      ws: true,
      rewrite: (path) => path.replace(/^\/api/, ''),
    },
  },
}
```

`ws: true` SignalR uchun kerak. Backend CORS'da `http://localhost:5173` ruxsat etilgan, lekin proxy bilan CORS ishlatilmaydi. `Secure` cookie `localhost` da ishlaydi.

Lokal backend'ni ishga tushirish: `D:\Projects\iccu-access-card` da `dotnet run --project src/Iccu.Api`. API bazani o'zi yaratadi. Foydalanuvchilarni SQL skript bilan qo'shish kerak (9-bo'lim).

---

## 2. Javob formatlari

### Muvaffaqiyat: `Result` konverti

Ma'lumot qaytaradigan so'rov:

```json
{
  "data": { "code": "0001", "expiresAt": "2026-09-27T14:17:19.2137765Z" },
  "isSuccess": true,
  "isFailure": false,
  "error": { "code": "", "message": "", "messages": { "en": "", "uz": "", "ru": "" }, "type": 0 }
}
```

Ma'lumotsiz command (`approve` dan boshqa POST/PUT/DELETE'larning ko'pi) ham `200` va `data`siz konvert qaytaradi. Frontend faqat `data` ni o'qiydi, `error` bo'sh bo'ladi.

### Sahifalangan ro'yxat: `PagedList`, konvertsiz

```json
{ "data": [ { "...": "..." } ], "totalCount": 1 }
```

### Xato: ProblemDetails

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "RegistrationRequest.NotPending",
  "status": 409,
  "detail": "The registration request has already been reviewed or has expired.",
  "messages": {
    "en": "The registration request has already been reviewed or has expired.",
    "uz": "Ariza allaqachon ko'rib chiqilgan yoki muddati o'tgan.",
    "ru": "Заявка уже рассмотрена или срок её действия истёк."
  },
  "traceId": "00-f14c0cd9c8eb2b84b00f5ac7f6ba5579-3d0b71a1c32fb150-00"
}
```

- `title` — xato **kodi**. Frontend xatoni matni bo'yicha emas, `title` (kod) bo'yicha taniydi.
- `messages` — foydalanuvchiga ko'rsatiladigan matn, uch tilda.
- `traceId` — log'dan qidirish uchun.

Validatsiya xatosi (`400`, `title: "General.Validation"`) qo'shimcha `errors` massivi bilan keladi:

```json
{
  "title": "General.Validation",
  "status": 400,
  "messages": { "uz": "Bir yoki bir nechta maydon noto'g'ri to'ldirilgan", "...": "..." },
  "errors": [
    {
      "propertyName": "details.phone",
      "code": "Reader.InvalidPhone",
      "message": "The phone number must be an Uzbek number: +998 XX XXX XX XX.",
      "messages": { "en": "...", "uz": "Telefon raqami O'zbekiston raqami bo'lishi kerak: +998 XX XXX XX XX.", "ru": "..." },
      "type": 2
    },
    {
      "propertyName": "details.lastName",
      "code": "NotEmptyValidator",
      "messages": { "uz": "'Last Name' bo'sh bo'lishi mumkin emas.", "...": "..." },
      "type": 2
    }
  ]
}
```

Frontend uchun ikki qoida:

1. **`propertyName` dagi `details.` prefiksini olib tashlash kerak.** Kitobxon va ariza so'rovlarining body'si tekis (`lastName`, `phone`, ...), lekin backend ularni ichida `PersonDetails` record'iga yig'adi. Shuning uchun xato `details.phone` bo'lib qaytadi. Boshqa maydonlar (`consentGiven`, `photoFileId`, `username`, ...) prefikssiz.
2. **Kod `Validator` bilan tugasa** (`NotEmptyValidator`, `MaximumLengthValidator`, ...), bu FluentValidation'ning umumiy xabari. Unda maydon nomi inglizcha (`'Last Name'`). Bunday xatolar uchun frontend o'z tarjimasini ko'rsatadi. Katalog kodlari (`Reader.InvalidPhone` kabi) uchun esa backend'ning `messages` matni ishlatiladi.

### Body'siz javoblar

| Status | Qachon | Body |
|---|---|---|
| `401` | token yo'q yoki eskirgan | **bo'sh** |
| `403` | rol yetmaydi | **bo'sh** |
| `429` | rate limit | **bo'sh** |
| `403` | serverda kutubxona tarmog'idan tashqarida `/api/...` (nginx) | nginx HTML sahifasi |
| `500` | kutilmagan xato | ProblemDetails, `title: "General.Unexpected"` |
| `409` | bir vaqtdagi ikki so'rov unique index'ga urildi | ProblemDetails, `title: "Conflict.DuplicateKey"` |

HTTP klient body bo'sh yoki JSON bo'lmagan javobni ham xatoga aylantira olishi kerak.

---

## 3. Enum'lar (butun son)

| Enum | Qiymatlar |
|---|---|
| `UserRole` | 0 Receptionist, 1 Admin |
| `ReaderCategory` | 0 Pupil (O'quvchi), 1 Student (Talaba), 2 Master (Magistr), 3 PhD, 4 DSc, 5 Professor, 6 Employee (Xizmatchi) |
| `Gender` | 0 Male (Erkak), 1 Female (Ayol) |
| `Citizenship` | 0 Uzbekistan (O'zbekiston fuqarosi), 1 Foreign (Chet el fuqarosi) |
| `RegistrationSource` | 0 Reception (Resepshn), 1 SelfService (QR anketa) |
| `RegistrationRequestStatus` | 0 Pending, 1 Approved, 2 Rejected, 3 Expired |
| `CardStatus` (faqat filtr) | 0 Active, 1 ExpiringSoon (30 kun ichida tugaydi), 2 Expired |
| `ReportGrouping` | 0 Day, 1 Month |
| `ErrorType` (`errors[].type`) | 0 Failure, 1 Validation, 2 Problem, 3 NotFound, 4 Conflict, 5 Unauthorized, 6 Forbidden |

Qavs ichidagi o'zbekcha nomlar Excel eksportda ishlatilgan nomlar bilan bir xil. UI'da ham shularni ishlating.

---

## 4. Autentifikatsiya

- **Access token**: JWT (HS256), 15 daqiqa. Login va refresh javobining body'sida keladi. **Faqat xotirada saqlanadi** (`localStorage` emas).
- **Refresh token**: `iccu_refresh` cookie, `HttpOnly; Secure; SameSite=Strict; Path=/api/auth`, 7 kun. JavaScript uni ko'rmaydi, brauzer `/api/auth/*` ga o'zi yuboradi. Har refresh'da yangisiga almashadi.
- Eski (almashtirilgan) refresh token qayta ishlatilsa, foydalanuvchining **barcha** sessiyalari bekor qilinadi (`401`).
- Access token claim'lari: `sub` (user id), `name` (username), `role` (`"Admin"` / `"Receptionist"`, matn!), `full_name`. Frontend profilni claim'dan emas, javobdagi `user` obyektidan oladi.

### Oqim

1. Ilova ochilganda: `POST /api/auth/refresh`. `200` bo'lsa, sessiya tiklanadi. `401` bo'lsa, login sahifasiga o'tiladi.
2. Login: `POST /api/auth/login`.
3. Har bir so'rov: `Authorization: Bearer <accessToken>`.
4. So'rov `401` qaytarsa: bir marta `POST /api/auth/refresh` qilinadi va so'rov qayta yuboriladi. Refresh ham `401` bo'lsa, sessiya tugagan, login sahifasiga o'tiladi. Bir vaqtda kelgan bir nechta `401` uchun refresh **bitta** bo'lishi kerak (umumiy promise).
5. Token tugashidan oldin (`expiresAt`) refresh qilish ham mumkin, lekin 4-band yetarli.
6. Chiqish: `POST /api/auth/logout`, keyin xotiradagi token o'chiriladi.
7. Parol almashtirilgandan keyin (`change-password`) backend cookie'ni o'chiradi va barcha sessiyalarni bekor qiladi. Frontend foydalanuvchini qayta login sahifasiga yuboradi.

### Login javobi

```json
{
  "data": {
    "accessToken": "eyJhbGciOi...",
    "expiresAt": "2026-09-26T14:32:15.8763086Z",
    "user": { "id": "1cd3a7b8-...", "username": "admin", "fullName": "Administrator", "role": 1 }
  },
  "isSuccess": true
}
```

### Login xatolari

| Status | `title` | Ma'nosi |
|---|---|---|
| 400 | `General.Validation` | bo'sh maydon |
| 401 | `User.InvalidCredentials` | login yoki parol noto'g'ri |
| 403 | `User.LockedOut` | 5 ta xato urinish, 15 daqiqaga bloklangan |
| 403 | `User.Inactive` | hisob o'chirilgan |
| 429 | — | daqiqasiga 10 tadan ko'p (IP bo'yicha, login/refresh/logout uchun umumiy) |

---

## 5. Sahifalash

So'rov parametrlari (query):

```
first=<offset, default 0>&rows=<1..1000, default 10>&sortField=<snake_case>&sortOrder=1|-1
```

- Offset asosida: 3-sahifa, 25 qatordan: `first=50&rows=25`.
- `sortField` — javob obyekti property'sining **snake_case** nomi (`last_name`, `expires_on`, `created_at`). Noma'lum qiymat jimgina `id` ga almashadi.
- Har bir ro'yxatning standart tartibi endpoint jadvalida yozilgan.

---

## 6. Endpoint'lar

Ruxsat: **Anonim**, **User** (Receptionist yoki Admin), **Admin**. Yo'llar brauzerdagi ko'rinishda (`/api` bilan).

### 6.1 Auth

| Metod va yo'l | Ruxsat | Body | Javob `data` |
|---|---|---|---|
| `POST /api/auth/login` | Anonim, rate limit | `{ username, password }` | `{ accessToken, expiresAt, user }` + cookie |
| `POST /api/auth/refresh` | Anonim (cookie), rate limit | — | login bilan bir xil |
| `POST /api/auth/logout` | Anonim (cookie), rate limit | — | — |
| `GET /api/auth/me` | User | — | `{ id, username, fullName, role }` |
| `POST /api/auth/change-password` | User | `{ currentPassword, newPassword }` | — (xato: 400 `User.WrongCurrentPassword`) |

### 6.2 Foydalanuvchilar (faqat Admin)

| Metod va yo'l | Body / query | Javob `data` |
|---|---|---|
| `GET /api/users` | paging + `search`, `role`, `isActive`. Standart: `full_name` asc | `PagedList<UserResponse>` |
| `POST /api/users` | `{ username, fullName, role, password }` | yangi `id` (409 `User.UsernameTaken`) |
| `PUT /api/users/{id}` | `{ fullName, role, isActive }` | — (409 `User.CannotDemoteSelf`: o'zini o'chirish yoki adminlikdan tushirish) |
| `POST /api/users/{id}/reset-password` | `{ newPassword }` | — (foydalanuvchining sessiyalari bekor bo'ladi) |

```ts
UserResponse { id, username, fullName, role: UserRole, isActive: boolean, isLockedOut: boolean, lastLoginAt: string | null, createdAt: string }
```

Username saqlashda kichik harfga o'tkaziladi. Faolsizlantirilgan foydalanuvchining sessiyalari darhol bekor qilinadi.

### 6.3 Fayllar (rasmlar)

| Metod va yo'l | Ruxsat | Body | Javob `data` |
|---|---|---|---|
| `POST /api/files` | User | `multipart/form-data`, maydon nomi **`file`** | fayl `id` (Guid) |
| `POST /api/public/files` | Anonim, 10 daqiqada 120 ta (IP) | xuddi shunday | fayl `id` |
| `GET /api/files/{id}/content` | User | — | rasm baytlari (`image/jpeg` / `png` / `webp`) |
| `DELETE /api/files/{id}` | Admin | — | — (409 `StoredFile.InUse`) |

- Faqat `.jpg`, `.jpeg`, `.png`, `.webp`, 8 MB gacha. Kengaytma va faylning birinchi baytlari mos kelishi kerak.
- Xatolar: `StoredFile.Empty`, `StoredFile.UnsupportedContent`, `StoredFile.TooLarge`, `StoredFile.ContentMismatch`.
- Server rasmni o'zgartirmaydi. **3:4 nisbatga kesish frontend'da** qilinadi (`react-easy-crop`), keyin kesilgan rasm JPEG qilib yuklanadi.
- **`GET .../content` token talab qiladi**, shuning uchun `<img src="/api/files/...">` ishlamaydi. Rasm `fetch` bilan (Bearer bilan) blob sifatida olinib, `URL.createObjectURL` bilan ko'rsatiladi va keyin `revokeObjectURL` qilinadi. Buni bitta umumiy komponent qiladi.
- Oqim: avval rasm yuklanadi va `id` olinadi, keyin shu `id` kitobxon yoki ariza so'rovida `photoFileId` sifatida yuboriladi. Hech qayerga bog'lanmagan rasm 24 soatdan keyin avtomatik o'chiriladi.

### 6.4 Ochiq anketa (QR, anonim)

| Metod va yo'l | Body | Javob `data` |
|---|---|---|
| `POST /api/public/registrations` | `PersonDetails` maydonlari + `photoFileId`, `consentGiven` | `{ code: "0001", expiresAt }` |

```ts
SubmitRegistrationRequest {
  category: ReaderCategory; lastName: string; firstName: string; middleName?: string | null;
  birthDate: string; gender: Gender; citizenship: Citizenship; phone: string;
  photoFileId: string; consentGiven: boolean;
}
```

- Rate limit: 10 daqiqada 60 ta (IP).
- `consentGiven` `true` bo'lishi shart (`RegistrationRequest.ConsentRequired`).
- `photoFileId` topilmasa `404 StoredFile.NotFound`.
- Javobdagi `code` tashrif buyuruvchiga katta qilib ko'rsatiladi. U resepshnga shu kodni aytadi. Ariza 24 soat amal qiladi.

### 6.5 Arizalar navbati (User)

| Metod va yo'l | Body / query | Javob `data` |
|---|---|---|
| `GET /api/registration-requests` | paging + `status`, `search` (kod aniq, familiya/ism/telefon qismi). Standart: `submitted_at` desc | `PagedList<RegistrationRequestListItem>` |
| `GET /api/registration-requests/{id}` | — | `RegistrationRequestResponse` |
| `PUT /api/registration-requests/{id}` | `PersonDetails` maydonlari (rasmsiz) | — |
| `POST /api/registration-requests/{id}/approve` | — | `{ readerId, cardNumber }` |
| `POST /api/registration-requests/{id}/reject` | `{ reason }` (1–500 belgi) | — |

```ts
RegistrationRequestListItem {
  id; photoFileId; code; status: RegistrationRequestStatus; category: ReaderCategory;
  lastName; firstName; middleName: string | null; phone;
  submittedAt; expiresAt; reviewedAt: string | null; reviewedByName: string | null;
  hasRegisteredPhone: boolean;
}

RegistrationRequestResponse {
  id; photoFileId; code; status; category; lastName; firstName; middleName: string | null;
  birthDate; gender: Gender | null; citizenship: Citizenship | null; phone;
  submittedAt; expiresAt; reviewedAt: string | null; reviewedByName: string | null;
  rejectionReason: string | null;
  readerId: string | null;
  registeredReaderId: string | null;
  registeredReaderCardNumber: string | null;
}
```

- `hasRegisteredPhone` / `registeredReaderId`: shu telefon raqami bilan kitobxon **allaqachon bor** (bir nechta bo'lsa, karta raqami eng kichigi). UI buni ogohlantirish qilib ko'rsatadi va mavjud kitobxonga havola beradi. Bunday arizani tasdiqlab bo'lmaydi (`409 Reader.PhoneAlreadyRegistered`). To'g'ri yo'l — rad etib, eski kartani qayta chop etish.
- `readerId`: tasdiqlangan arizadan yaratilgan kitobxon.
- Tahrirlash, tasdiqlash va rad etish faqat `Pending` va muddati o'tmagan arizada ishlaydi: `409 RegistrationRequest.NotPending` / `RegistrationRequest.Expired`.
- Tasdiqlangandan keyin kitobxon kartochkasiga o'tib, kartani chop etish kerak.

### 6.6 Kitobxonlar

| Metod va yo'l | Ruxsat | Body / query | Javob `data` |
|---|---|---|---|
| `GET /api/readers` | User | paging + filtr. Standart: `card_number` desc | `PagedList<ReaderListItem>` |
| `GET /api/readers/{id}` | User | — | `ReaderResponse` |
| `POST /api/readers` | User | `PersonDetails` maydonlari + `photoFileId` | `{ id, cardNumber }` |
| `PUT /api/readers/{id}` | User | `PersonDetails` maydonlari + `photoFileId` (to'liq body) | — |
| `POST /api/readers/{id}/renew` | User | — | `{ issuedOn, expiresOn }` (bugundan +2 yil, raqam o'zgarmaydi) |
| `POST /api/readers/{id}/prints` | User | — | — (karta chop etilganini qayd qiladi) |
| `DELETE /api/readers/{id}` | Admin | — | — (yumshoq o'chirish) |
| `GET /api/readers/export` | Admin | ro'yxat filtrlari, paging'siz | `.xlsx` fayl |

Filtr parametrlari (ro'yxat va eksport uchun bir xil): `search`, `category`, `gender`, `citizenship`, `source`, `status` (`CardStatus`), `registeredFrom`, `registeredTo` (`DateOnly`).

`search` bitta maydon bilan hammasini qidiradi:
- F.I.Sh. so'zlari (tartibi muhim emas, apostrof variantlari farq qilmaydi);
- karta raqami (`1` ham, `0000001` ham);
- telefon (kamida 4 raqam).

```ts
ReaderListItem {
  id; photoFileId; cardNumber; category; lastName; firstName; middleName: string | null;
  birthDate; gender: Gender | null; citizenship: Citizenship | null; phone;
  source: RegistrationSource; issuedOn; expiresOn; isExpired: boolean; printCount: number; createdAt;
}

ReaderResponse {
  id; photoFileId; cardNumber; category; lastName; firstName; middleName: string | null;
  birthDate; gender: Gender | null; citizenship: Citizenship | null; phone;
  source; issuedOn; expiresOn; isExpired: boolean;
  printCount: number; lastPrintedAt: string | null;
  createdAt; createdByName: string | null; updatedAt: string | null;
}
```

- `gender` va `citizenship` 2026-09-30 dan oldin yozilgan kitobxon va arizalarda `null` (ular hujjat maydonlari o'rniga qo'shilgan). Yangi yozuvda ikkalasi majburiy, eski yozuvni tahrirlaganda ham to'ldirilishi kerak.
- Bitta telefon raqamiga bitta kitobxon: `409 Reader.PhoneAlreadyRegistered`. Bazada unique index yo'q (eski yozuvlarda bir xil raqamlar bor), tekshiruv handler'da.
- O'chirilgan kitobxon hamma joyda `404 Reader.NotFound`. Karta raqami boshqa odamga qayta berilmaydi.
- Eksport: javob `Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, `Content-Disposition: attachment; filename=kitobxonlar-20260926.xlsx`. Token kerak bo'lgani uchun `fetch` → blob → yuklab olish havolasi orqali olinadi.

### 6.7 Dashboard va hisobot (User)

| Metod va yo'l | Query | Javob `data` |
|---|---|---|
| `GET /api/dashboard` | — | `DashboardResponse` |
| `GET /api/reports/registrations` | `from`, `to` (`DateOnly`, majburiy), `groupBy` (`ReportGrouping`, standart Day) | `RegistrationReportResponse` |

```ts
DashboardResponse {
  totals: { total; active; expired; expiringSoon; registeredToday; registeredThisMonth };
  pendingRequests: number;
  byCategory: { category: ReaderCategory; count: number }[];
  byGender: { gender: Gender | null; count: number }[];              // null: ko'rsatilmagan (eski yozuvlar)
  byCitizenship: { citizenship: Citizenship | null; count: number }[];
  lastDays: { day: string; count: number }[];   // doim 30 ta, bo'sh kunlar 0 bilan
}

RegistrationReportResponse {
  from; to; groupBy: ReportGrouping; total: number;
  byPeriod: { period: string; total; reception; selfService }[];   // period: kun yoki oyning 1-sanasi
  byCategory: { category; count }[];
  byGender: { gender; count }[];
  byCitizenship: { citizenship; count }[];
  byUser: { userId; fullName; count }[];
}
```

Hisobot davri: `to >= from`, ko'pi bilan 3 yil (1098 kun), aks holda `400`. Kunlar `Asia/Tashkent` bo'yicha hisoblanadi.

### 6.8 Real vaqt (SignalR)

| | |
|---|---|
| Hub | `/api/hubs/registrations` |
| Ruxsat | User |
| Token | `accessTokenFactory: () => accessToken` (query'dagi `access_token` orqali ketadi) |
| Hodisa | `RegistrationSubmitted` |
| Payload | `{ id, code, fullName, submittedAt }` (`fullName` = "Familiya Ism") |

Yangi QR ariza kelganda hamma ulangan foydalanuvchilarga yuboriladi. Frontend toast va ovoz chiqaradi hamda navbat so'rovini yangilaydi (`invalidateQueries`). Token yangilanganda yangi ulanish yangi tokenni oladi (`accessTokenFactory` har safar joriy tokenni qaytaradi). `withAutomaticReconnect()` kerak.

### 6.9 Holat

`GET /api/health` — ochiq, `Healthy` matni.

---

## 7. Validatsiya qoidalari (Zod sxemalari uchun)

Frontend foydalanuvchiga xatoni tezroq ko'rsatish uchun shu qoidalarni takrorlaydi. Yakuniy tekshiruv baribir backend'da.

### PersonDetails (kitobxon, ariza, QR anketa)

| Maydon | Qoida |
|---|---|
| `category` | enum |
| `lastName`, `firstName` | majburiy, ≤ 100, harf bilan boshlanadi; harf, `'`, `ʻ`, `ʼ`, `‘`, `’`, `` ` ``, bo'sh joy, `-`. Regex: `` ^\s*\p{L}[\p{L}\p{M}'ʻʼ‘’` -]*$ `` (`u` flag) |
| `middleName` | ixtiyoriy, xuddi shu qoida |
| `birthDate` | `>= 1900-01-01` va bugundan (Toshkent) oldin |
| `gender` | majburiy enum (`null` → `NotNullValidator`) |
| `citizenship` | majburiy enum (`null` → `NotNullValidator`) |
| `phone` | O'zbekiston fuqarosi: raqamlardan 9 tasi (`90 123 45 67`) yoki `998` bilan 12 tasi (`Reader.InvalidPhone`). Chet el fuqarosi: O'zbekiston raqami yoki 8–15 raqamli xalqaro raqam, `0` bilan boshlanmaydi (`Reader.InvalidInternationalPhone`). Server `+` va raqamlar qilib saqlaydi: `+998901234567`, `+79012345678` |

### Boshqalar

| Maydon | Qoida |
|---|---|
| `username` | `^[A-Za-z0-9._-]{3,50}$` |
| `password`, `newPassword` | 8–128 belgi, kamida bitta harf va bitta raqam |
| `fullName` (foydalanuvchi) | majburiy, ≤ 150 |
| login `username` / `password` | bo'sh emas, ≤ 50 / ≤ 128 |
| `reason` (rad etish) | majburiy, ≤ 500 |
| `consentGiven` | `true` |

---

## 8. Xato kodlari

| Kod | Status | Qayerda |
|---|---|---|
| `General.Validation` | 400 | hamma joyda, `errors[]` bilan |
| `General.Unexpected` | 500 | kutilmagan |
| `Conflict.DuplicateKey` | 409 | bir vaqtdagi ikki yozuv |
| `User.InvalidCredentials` | 401 | login |
| `User.LockedOut`, `User.Inactive` | 403 | login |
| `User.InvalidRefreshToken` | 401 | refresh |
| `User.NotFound` | 404 | me, foydalanuvchi |
| `User.UsernameTaken` | 409 | foydalanuvchi yaratish |
| `User.CannotDemoteSelf` | 409 | foydalanuvchini tahrirlash |
| `User.WrongCurrentPassword` | 400 | parol almashtirish |
| `User.WeakPassword`, `User.InvalidUsername` | 400 | `errors[]` ichida |
| `Reader.NotFound` | 404 | kitobxon |
| `Reader.PhoneAlreadyRegistered` | 409 | yaratish, tahrirlash, tasdiqlash |
| `Reader.InvalidPhone`, `Reader.InvalidInternationalPhone`, `Reader.InvalidBirthDate` | 400 | `errors[]` ichida |
| `RegistrationRequest.NotFound` | 404 | ariza |
| `RegistrationRequest.NotPending`, `RegistrationRequest.Expired` | 409 | tahrirlash, tasdiqlash, rad etish |
| `RegistrationRequest.ConsentRequired` | 400 | `errors[]` ichida |
| `StoredFile.NotFound` | 404 | rasm, `photoFileId` |
| `StoredFile.InUse` | 409 | rasmni o'chirish |
| `StoredFile.Empty`, `UnsupportedContent`, `TooLarge`, `ContentMismatch` | 400 | yuklash |

---

## 9. Lokal foydalanuvchilar

API foydalanuvchi yaratmaydi. Lokal bazaga SQL bilan qo'shiladi (`psql -d iccu -f seed-users.sql`):

```sql
INSERT INTO iccu.users (id, username, full_name, password_hash, role, is_active, failed_login_attempts, created_at)
VALUES
    (gen_random_uuid(), 'admin',    'Administrator',   'AQAAAAIAAYagAAAAEB+d9zWWKSgHcFGg0C83LxJ4M1zbwrNl8Fzs76wUXPxTYHYH2pWFQUKQyOc8EPVqaw==', 1, true, 0, now()),
    (gen_random_uuid(), 'resepshn', 'Resepshn Xodimi', 'AQAAAAIAAYagAAAAEF6YSB/EV3UJjYwUGm1aR6DoBUEDQ9+VzoS59/awWowJncwIRRHnNTLhBL6AQ1kInA==', 0, true, 0, now());
```

Parollar: `admin` / `Admin12345`, `resepshn` / `Resep12345`. Faqat lokal dev uchun.

---

## 10. Deploy bilan bog'liq

- Frontend build (`dist/`) serverda `/opt/iccu/web` ga ko'chiriladi. nginx uni `/` da beradi, SPA uchun `try_files $uri $uri/ /index.html`. `index.html` keshlanmaydi.
- `/api/public/*` internetdan ochiq (QR anketa). Qolgan `/api/*` va hub faqat kutubxona tarmog'idan ochiq. Tashqaridan admin panel sahifasi ochiladi, lekin API `403` (nginx HTML) qaytaradi. UI buni "faqat kutubxona tarmog'idan" deb ko'rsatishi kerak.
- nginx `X-Frame-Options: DENY` qo'yadi, ilova iframe'da ochilmaydi.
- Bitta domen, bitta origin: frontend `/`, API `/api`. Production'da CORS ishlatilmaydi.

# ICCU — Frontend rejasi

Yangi repo: ICCU frontend'i (React). Bu reja yangi chatda ishni boshlash uchun yozilgan. Frontend repo'sida `docs/roadmap.md` va `docs/architecture.md` ga bo'linib ko'chiriladi.

Birga o'qiladi:
- `docs/frontend-contract.md` — backend API shartnomasi (frontend repo'sida `docs/backend-contract.md`).
- `docs/project-overview.md` — talablar, rollar, jarayon.
- `D:\Projects\tofan-ui\CLAUDE.md` — foydalanuvchining tasdiqlangan frontend qoidalari (Angular). Bu reja o'sha qoidalarni React'ga ko'chiradi, xuddi backend `tofan`'ga moslanganidek.

---

## 1. Maqsad

Bitta ilovada ikki zona:

| Zona | Kim uchun | Qurilma |
|---|---|---|
| `/royxat` | Tashrif buyuruvchi (QR orqali) | Telefon, mobilga moslangan |
| `/admin` | Receptionist va Admin | Kutubxona kompyuteri |

Admin panelda:
- arizalar navbati real vaqtda yangilanadi;
- kitobxonlarni qidirish, yaratish, tahrirlash, uzaytirish va karta chop etish;
- dashboard va hisobotlar;
- foydalanuvchilarni boshqarish va Excel eksport (faqat Admin).

---

## 2. Texnologiyalar

`project-overview.md` da kelishilgan to'plam, ustiga shu to'plam talab qiladigan minimal qo'shimchalar:

| Vazifa | Tanlov | Izoh |
|---|---|---|
| Asos | React 19, Vite, TypeScript (`strict`) | |
| UI | Tailwind CSS 4, shadcn/ui | Figma shablon asosida |
| Server holati | TanStack Query | tofan-ui'dagi signal store'lar o'rnida |
| Jadval | TanStack Table | Server tomonda sahifalash va tartiblash |
| Forma | React Hook Form + Zod | Zod sxemalari backend qoidalarini takrorlaydi |
| Marshrut | React Router | Lazy yuklanadigan feature route'lari |
| Real vaqt | `@microsoft/signalr` | Arizalar hub'i |
| Grafik | Recharts | Dashboard, hisobot |
| Rasm kesish | react-easy-crop | 3:4 |
| Test | Vitest, Testing Library, jsdom | |
| Sifat | ESLint (typescript-eslint strict), Prettier, Sheriff | Sheriff — tofan-ui'dagidek chegara nazorati |

Paketlar `npm` bilan o'rnatiladi (tofan-ui kabi). Ro'yxatdan tashqari paket faqat foydalanuvchi ruxsati bilan qo'shiladi.

**Ataylab ishlatilmaydi (YAGNI, tofan-ui qoidalari):**
- OpenAPI codegen;
- axios (`fetch` yetadi);
- global state kutubxonasi (Redux, Zustand);
- mock backend;
- Clean Architecture qatlam papkalari.

---

## 3. Arxitektura

tofan-ui bilan bir xil feature-based tuzilma. UI loyihada `domain/application/infrastructure` qatlamlari **yo'q**.

```text
src/
  main.tsx
  app/                        App, provider'lar (QueryClient, Auth, i18n), router
  routes/
    app-routes.tsx            yuqori darajadagi route'lar, har feature lazy
  core/                       ilova bo'yi yagona narsalar, feature'ga xos emas
    http/                     api-client (fetch), ApiError, Result va PagedList turlari, paging query
    auth/                     xotiradagi sessiya, AuthProvider, useAuth, refresh (single-flight), RequireRole guard
    realtime/                 SignalR ulanishi (arizalar hub'i)
    layout/                   admin shell, sidebar (rolga qarab menyu), public layout
    feedback/                 toast, tasdiqlash oynasi, xato → xabar
    i18n/                     lug'atlar (uz, ru, en), typed kalitlar, useT, sana formati (Asia/Tashkent)
  shared/                     biznesga bog'lanmagan, istalgan feature'da ishlatiladi
    ui/                       shadcn/ui komponentlari
    components/               data-table, authorized-image, photo-cropper, file-download
    person-details/           PersonDetails formasi maydonlari va Zod sxemasi (3 feature ishlatadi)
    models/                   enum'lar va ularning yorliqlari, PagingState
    utils/                    sof funksiyalar
  features/
    auth/                     login, parolni almashtirish, ruxsat yo'q
    public-registration/      /royxat: anketa, rasm, kod ekrani
    registration-requests/    navbat, ariza kartochkasi, tasdiqlash/rad etish
    readers/                  ro'yxat, kartochka, yaratish/tahrirlash, uzaytirish, eksport, karta chop etish
    dashboard/
    reports/
    users/                    faqat Admin
    not-found/
```

### 3.1 Feature ichida

```text
features/readers/
  pages/readers-page.tsx, reader-page.tsx, reader-form-page.tsx
  components/reader-filters.tsx, reader-card-print.tsx
  models/reader.ts, reader-filter.ts, reader-form.schema.ts
  api/readers.dto.ts        backend javobi aynan shaklda
  api/readers.mapper.ts     DTO ↔ model
  api/readers.service.ts    HTTP funksiyalar (faqat core/http orqali)
  api/readers.queries.ts    TanStack Query hook'lari va query kalitlari
  readers.routes.tsx
```

tofan-ui'dagi `<feature>.store.ts` o'rnini `api/<feature>.queries.ts` egallaydi. Server holati faqat TanStack Query'da turadi, UI holati komponentning o'zida (`useState`).

### 3.2 Bog'lanish yo'nalishi (Sheriff majburlaydi)

| Papka | Import qila oladi | Import qila olmaydi |
|---|---|---|
| `core` | boshqa `core`, `shared` | `features` |
| `shared` | `shared`, `core/http`, `core/feedback`, `core/i18n` | `features`, boshqa `core` |
| `features/<x>` | `core`, `shared`, o'zi (nisbiy import) | boshqa feature'lar |
| `routes` | `features` (lazy), `core/auth`, `core/layout` | `shared` |

Ikki feature bir xil kodga muhtoj bo'lsa, u `shared` ga ko'chadi. Feature'ni o'chirish faqat uning route'i va menyu bandini buzadi.

### 3.3 Qatlamlar oqimi

```text
page / component  →  queries hook  →  service  →  core/http api-client  →  /api
       ↑ model            ↑ mapper(dto)
```

- Komponent HTTP chaqirmaydi. U faqat hook ishlatadi.
- Service faqat `core/http` orqali ishlaydi.
- Token faqat `core/auth` da o'qiladi.

---

## 4. Asosiy qarorlar

1. **Token xotirada**, refresh `HttpOnly` cookie'da. Ilova ochilganda `refresh` bilan sessiya tiklanadi. `401` bo'lsa, bitta umumiy refresh chaqiriladi va so'rov qayta yuboriladi.
2. **`api-client`** bitta joyda:
   - `/api` ga qo'shadi va Bearer header'ni biriktiradi;
   - `Result` konvertidan `data` ni ochadi;
   - ProblemDetails'ni `ApiError { status, code, messages, fieldErrors }` ga aylantiradi, bo'sh body'ni ham;
   - `propertyName` dagi `details.` prefiksini olib tashlaydi.
3. **Xatolar kod bo'yicha** taniladi, matn bo'yicha emas. Forma maydon xatolari React Hook Form'ga `setError` bilan beriladi.
4. **Rasmlar token bilan**: `shared/components/authorized-image` rasmni blob qilib oladi. Yuklashdan oldin `photo-cropper` 3:4 ga kesadi.
5. **Excel va boshqa fayllar**: `fetch` → blob → yuklab olish (`file-download`).
6. **Sana va vaqt**: backend UTC beradi, UI `Asia/Tashkent` da ko'rsatadi. `DateOnly` satr sifatida qoladi.
7. **Enum'lar** `shared/models` da `const` obyekt va o'zbekcha yorliqlar bilan turadi. Ro'yxatlar shu yerdan olinadi (DRY).
8. **SignalR** faqat admin zonasida, login'dan keyin ulanadi. `RegistrationSubmitted` kelganda toast, ovoz va navbat query'sini `invalidate` qilish.
9. **Karta chop etish**: 85 × 55 mm, CSS `@page`, alohida print sahifasi. Chop etilgandan keyin `POST /readers/{id}/prints`.

---

## 5. Kod qoidalari (tofan-ui'dan)

- **SOLID**:
  - **S**: bitta komponent bitta vazifa, ~200 qatordan oshsa bo'linadi.
  - **O**: `if (type === ...)` zanjiri o'rniga props, composition va strategy map.
  - **I**: har resurs uchun kichik service va hook.
  - **D**: komponent → hook → service → api-client.
- **KISS**: eng oddiy ishlaydigan yechim. **DRY**: api-client, PersonDetails formasi va enum yorliqlari bitta joyda. **YAGNI**: faqat hozirgi talab.
- Izoh yo'q (TS, TSX, CSS, konfiglar). Lint tekshiradi, markdown bundan mustasno.
- `any`, `as unknown as`, `@ts-ignore`, `eslint-disable` yo'q.
- Funksiya ≤ 25 qator, komponent ≤ 200 qator.
- UI matni kodda yozilmaydi, hammasi `core/i18n` kalitlari orqali.
- Magic string/son yo'q.
- Nomlar ma'nosini aytadi. `Helper`, `Util`, `Manager`, `data` taqiqlangan.
- Fayl nomlari kebab-case: `reader-form-page.tsx`, komponent `ReaderFormPage`.
- Early return, `readonly` ma'lumot, hook qoidalariga amal qilinadi.

---

## 6. Testlar

- Har bir mapper, Zod sxemasi, `api-client` (refresh, xato parse), auth va sof qoidalar uchun unit test.
- Formalar va muhim komponentlar Testing Library bilan: kirish → natija.
- Hook va komponent testlari service funksiyasini `vi.mock` qiladi, `fetch` ni emas.
- Test fayli kod yonida: `readers.mapper.test.ts`.
- Test nomi: `should <natija> when <shart>`.
- Har bosqich oxirida haqiqiy backend bilan qo'lda tekshiriladi (backend'dagi "kutubxonada bir kun" ssenariysi).

---

## 7. Bosqichlar

Har bosqich: kod, testlar, `lint` + `test` + `build` o'tadi, haqiqiy backend bilan tekshiriladi, commit faqat foydalanuvchi so'raganda.

| # | Bosqich | Natija |
|---|---|---|
| 0 | **Poydevor** | Vite + React + TS strict, Tailwind + shadcn/ui, ESLint + Prettier + Sheriff + izoh tekshiruvi, Vitest, `@core` / `@shared` / `@features` alias'lari, Vite proxy, `CLAUDE.md`, `docs/` (contract, architecture, roadmap), README |
| 1 | **core** | `http` (api-client, ApiError, paging), `i18n` (uz/ru/en typed), `feedback` (toast, confirm) va ularning testlari |
| 2 | **Auth va shell** | login, sessiyani tiklash, 401 → refresh, rol guard'i, chiqish, parolni almashtirish, admin layout va rolga qarab menyu, 403/404 sahifalari |
| 3 | **QR anketa `/royxat`** | mobil forma, galereya/kamera, 3:4 kesish, anonim yuklash, rozilik, kod ekrani, rate limit va validatsiya xabarlari |
| 4 | **Arizalar navbati** | ro'yxat (status, qidiruv), SignalR (toast, ovoz, yangilash), kartochka va rasm, tahrirlash, tasdiqlash → kitobxonga o'tish, rad etish, "hujjat allaqachon bor" ogohlantirishi |
| 5 | **Kitobxonlar** | jadval (server paging/sort, filtrlar, qidiruv), kartochka, yaratish/tahrirlash rasm bilan, uzaytirish, o'chirish va eksport (Admin) |
| 6 | **Karta chop etish** | 85 × 55 mm print sahifasi, chop etishni qayd qilish. Karta dizayni va printer aniqlanguncha kutadi |
| 7 | **Dashboard va hisobot** | kartochkalar, 30 kunlik grafik, toifalar; davr, kun/oy, manba va xodim kesimi |
| 8 | **Foydalanuvchilar** | ro'yxat, yaratish, tahrirlash (rol, faollik), parolni tiklash |
| 9 | **Deploy** | production build, `/opt/iccu/web`, nginx orqali to'liq ssenariy |

---

## 8. Yangi chatda birinchi qiladigan ishlar

1. Shu faylni, `frontend-contract.md` ni va `tofan-ui/CLAUDE.md` ni o'qish.
2. Foydalanuvchidan 9-bo'limdagi ochiq savollarni so'rash (bosqich 0 dan oldin kerak bo'lganlari: 1, 2, 5).
3. Bosqich 0: poydevor, `CLAUDE.md` va `docs/`.
4. Keyin bosqichlar tartib bilan.

---

## 9. Ochiq savollar

1. **Figma**: shablon havolasi. Qaysi ekranlar tayyor?
2. **Tillar**: tofan-ui'dagidek uz/ru/en, yoki boshlanishiga faqat o'zbekcha (lotin)? QR anketa qaysi tillarda bo'ladi?
3. **Karta dizayni**: kartada nimalar bo'ladi (rasm, F.I.Sh., toifa, raqam, muddat, logotip)? Shtrix-kod yoki QR kerakmi? Rasmiy logotip SVG'da bormi?
4. **Printer**: Canon'ning aniq modeli. Chetsiz chop etish va hoshiyalar shunga bog'liq.
5. **Deploy**: `dist/` ni serverdagi `/opt/iccu/web` ga ko'chirish (hozirgi nginx shunga moslangan) yoki tofan-ui'dagidek nginx'li alohida Docker image?
6. **Ovozli bildirishnoma**: yangi ariza kelganda receptionist kompyuterida ovoz chiqsinmi?
7. **Brauzerlar**: kutubxona kompyuterlarida qaysi brauzer bor (Chrome/Edge)?

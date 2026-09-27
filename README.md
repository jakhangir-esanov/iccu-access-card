# ICCU Access Card

O'zbekiston Islom sivilizatsiyasi markazi kutubxonasi uchun kitobxonlarni ro'yxatga olish va 85 × 55 mm kirish kartasini chop etish tizimining backend'i.

- 7 xonali karta raqami, hech qachon reset bo'lmaydi, kitobxon tasdiqlanganda beriladi.
- Karta 2 yil amal qiladi, uzaytirishda raqam o'zgarmaydi.
- Tashrif buyuruvchi QR kod orqali anketa to'ldiradi, receptionist uni tasdiqlaydi yoki rad etadi (real vaqtda, SignalR orqali).
- Kitobxonlar ro'yxati, qidiruv, Excelga eksport, dashboard va hisobotlar.
- Tizimga faqat xodimlar kiradi: `Receptionist` va `Admin`.

## Texnologiyalar

.NET 10, ASP.NET Core Minimal API, MediatR (CQRS), EF Core 10 (yozish) va Dapper (o'qish), PostgreSQL 17+, FluentValidation, o'zimizning JWT auth, SignalR, Hangfire, Serilog, Swagger, xUnit va NetArchTest.

## Tuzilma

```text
src/
  Iccu.Domain/           entity'lar, Errors, IRepository
  Iccu.Application/      command/query handler'lar, validator'lar, pipeline behavior'lar
  Iccu.Infrastructure/   DbContext, migratsiyalar, repository'lar, JWT, fayl saqlash, Hangfire, DI
  Iccu.Presentation/     endpoint'lar, SignalR hub
  Iccu.Api/              Program.cs, middleware, Swagger
test/
  Iccu.ArchitectureTests/
  Iccu.UnitTests/
docs/
```

## Lokal ishga tushirish

Kerak: .NET 10 SDK va PostgreSQL. `appsettings.Development.json` bo'yicha baza `localhost:5432/iccu`, foydalanuvchi va parol `postgres`.

```bash
dotnet run --project src/Iccu.Api
```

API `http://localhost:5080` da ishga tushadi. Swagger va Hangfire dashboard faqat Development muhitida ochiladi. Holatni tekshirish: `GET /health`.

API ishga tushganda bazani yaratadi va migratsiyalarni qo'llaydi. Boshlang'ich ma'lumot (foydalanuvchilar ham) yaratilmaydi.

## Testlar

```bash
dotnet test iccu.slnx
```

## Docker

```bash
docker build -t iccu-api .
```

Konteyner `8080` portda ishlaydi. Rasmlar `/var/iccu/files` ga yoziladi, bu papka albatta volume sifatida mount qilinishi kerak. Serverga o'rnatish: [docs/deployment.md](docs/deployment.md).

## Hujjatlar

- [Loyihaning umumiy ko'rinishi](docs/project-overview.md): talablar, rollar, ma'lumotlar modeli, API rejasi
- [Arxitektura](docs/architecture.md): qatlamlar va kod yozish qoidalari
- [Deploy](docs/deployment.md): Docker Swarm'ga o'rnatish
- [Frontend shartnomasi](docs/frontend-contract.md): frontend uchun API, auth oqimi, xatolar, validatsiya
- [Frontend rejasi](docs/frontend-plan.md): frontend arxitekturasi va bosqichlari

## Litsenziya

[LICENSE](LICENSE)

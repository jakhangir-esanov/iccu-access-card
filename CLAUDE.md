# CLAUDE.md

Backend for the reader registration and 85 × 55 mm access-card system of the library of O'zbekiston Islom sivilizatsiyasi markazi. .NET 10, plain Clean Architecture modelled on the user's earlier `tofan` project.

Read before larger changes:
- [docs/project-overview.md](docs/project-overview.md): requirements, roles, data model, API plan, open questions
- [docs/architecture.md](docs/architecture.md): layer rules and the reasons behind them
- [docs/deployment.md](docs/deployment.md) and `deploy/`: Docker Swarm setup

Docs are written in Uzbek (Latin). Keep new docs and doc edits in Uzbek, in the same plain style.

## Commands

```bash
dotnet build iccu.slnx
dotnet test iccu.slnx
dotnet run --project src/Iccu.Api
dotnet tool restore
dotnet ef migrations add <Name> -p src/Iccu.Infrastructure -s src/Iccu.Api -o Database/Migrations
dotnet ef database update -p src/Iccu.Infrastructure -s src/Iccu.Api
```

Local API: `http://localhost:5080`, Swagger and the Hangfire dashboard are on only in Development. Local database comes from `appsettings.Development.json` (`localhost:5432/iccu`, `postgres`/`postgres`).

A change is done when the build has no warnings (SonarAnalyzer is on for every project) and all tests pass, including `test/Iccu.ArchitectureTests`.

## Layers

```text
Iccu.Api             Program.cs, middleware, Swagger, CORS
  Iccu.Infrastructure  EF Core, Dapper, JWT, ObjectStorage, Hangfire, SignalR, rate limiting, ALL DI
    Iccu.Presentation    endpoints (IEndpoint), Requests/, ApiResults, SignalR hub
    Iccu.Application     command/query handlers, validators, pipeline behaviors, Dapper SQL
      Iccu.Domain          Common/ + one folder per entity
```

Architecture tests enforce the dependency direction, naming, and `sealed`/`internal` rules. Do not weaken a test to make a change pass.

## Rules

### Domain
- `Common/` holds `Result`, `Error`, `ValidationError`, `PersonDetails` and `Enums/` (all enums live there).
- Each entity has its own folder with exactly three kinds of file: the entity, its static `XxxErrors` catalog, and `IXxxRepository`. Current folders: `Readers`, `RegistrationRequests`, `Users`, `RefreshTokens`, `StoredFiles`.
- Entities are `sealed`, have no public constructor, only a static factory and state-changing methods. They never return `Result` and never make decisions.
- Every `Error` has `en`, `uz` and `ru` messages. No hard-coded error strings in handlers.

### Business logic lives in handlers
- Rules go inside the command/query handler as `private const` values and inline checks: card validity 2 years, card number format `D7`, login lockout 5 attempts / 15 minutes, refresh-token rotation, 24-hour request lifetime.
- Never create helper, policy or rules classes (`CardNumber`, `CardValidity`, `LoginPolicy`, `ReaderFilter`, ...). The user explicitly rejected them.
- Code shared by several handlers is referenced from the owning handler's `internal static` member (e.g. `GetReadersQueryHandler.BuildFilter`).

### CQRS with MediatR
- MediatR 12.5 (not Cortex.Mediator). Endpoints send through `ISender`.
- Command/query record and its handler sit in the same file; response records sit next to them. Validators are separate `XxxValidator.cs` files.
- Handlers and validators are `internal sealed`, use primary constructors, and the handler parameter is always named `request`. Validators use `x =>`.
- Application has no DI code. MediatR, behaviors and validators are registered in `InfrastructureConfiguration` through `AssemblyReference.Assembly`.

### Writes and reads
- Writes: EF Core through a repository and `IUnitOfWork.SaveChangesAsync`, in command handlers only. Repositories are write-side only (`GetAsync`, `Insert`, uniqueness checks).
- Reads: Dapper through `IDbConnectionFactory`, SQL inside the query handler file. Responses are positional records; SELECT column order and types must match the record parameters exactly (`COUNT(*)::int`, `DateOnly` type handler).
- List queries follow `tofan`'s `GetSoldiersQuery`: CTE, `List<string> conditions`, `dataSql` + `countSql`, `PagingRequest<T>` (`first`, `rows`, `sortField`, `sortOrder`), `PagedList<T>`. Filter fields stay on the query record, no separate `XFilter` record.
- Dynamic SQL only ever concatenates constant fragments from code. Every value is a parameter. Sort columns come from the whitelist built in `PagingRequest<T>`.

### API
- Routes have no `/api` prefix; nginx strips it. The refresh cookie path is still `/api/auth` because that is what the browser sees.
- Success returns the whole `Result` (`result.ToResponse()`); `PagedList` is returned unwrapped. Failures are ProblemDetails with `uz`/`ru`/`en` messages.
- Enums travel as numbers. Request records live in `Requests/` as `internal sealed record`. Endpoints take no `CancellationToken` and declare `.Produces<...>`, `.RequireAuthorization(Policies.X)` and `.WithTags(...)`.

### Users and auth
- There is one user concept: `User` with `UserRole` `Admin` or `Receptionist`; policies `Policies.User` (both) and `Policies.Admin`. Never introduce the word "staff" in types, routes, tables or docs.
- Readers never log in. They are data records created by users or by approving a QR registration request. Do not propose reader login or self-service profiles unless the user asks.
- Own JWT (HS256, 15 minutes) plus a rotated refresh token in the `iccu_refresh` HttpOnly cookie; only its SHA-256 hash is stored.
- Koha is not a `User`. It reads `koha/` endpoints with HTTP Basic (`Koha:Username`/`Koha:Password`, `Policies.Koha`, Basic scheme only). JWT stays the default scheme. See [docs/koha-integration.md](docs/koha-integration.md).

### Other
- YAGNI. Add only what a current requirement uses: no speculative options, interfaces, parameters, columns or "might need later" helpers. Delete code, config and columns that nothing reads.
- No comments in C# code (`//`, `/* */`, `///`). Explain reasons in commit messages or `docs/`.
- Time only through `IDateTimeProvider`; "today" is calculated in `Clock:TimeZone` (`Asia/Tashkent`). Store UTC.
- `using` directives go inside the namespace, ordered by line length, as in existing files.
- Background work is a Hangfire `XJob` + `XJobScheduler` pair; the job only sends a MediatR command.
- Photos are stored as uploaded (no image processing) through `IFileStore`; entities hold `PhotoFileId`.
- Only `created_by` and `reviewed_by` are recorded. There is no audit log on purpose.
- Tests: xUnit `Assert` only (FluentAssertions 8+ is commercial), hand-written fakes in `test/Iccu.UnitTests/Fakes`, no mocking library.

## Do not change without asking
- At startup the API applies EF migrations (`ApplyMigrationsAsync`) and creates nothing else. There is no seeding and no first-admin bootstrap; the user rejected one on 2026-09-26. How the first admin reaches the server is still undecided.
- The nginx `geo $is_library_network` block in `deploy/nginx/conf.d/iccu.conf` still lists generic private ranges; the real library range is unknown.
- `Newtonsoft.Json` is pinned to 13.0.3 because Hangfire otherwise pulls a vulnerable version.
- `Microsoft.EntityFrameworkCore.Relational` is pinned to 10.0.12 in Infrastructure so it matches `Microsoft.EntityFrameworkCore.Design` in Api; without it the build warns MSB3277.

## Git
Commit or push only when the user asks. Repository: `jakhangir-esanov/iccu-access-card`, branch `main`.

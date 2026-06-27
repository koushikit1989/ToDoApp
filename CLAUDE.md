# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Commands

```bash
# Build entire solution
dotnet build

# Run the API (dev — auto-migrates DB on startup)
dotnet run --project src/ToDoManagementSystem.API

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/ToDoManagementSystem.UnitTests
dotnet test tests/ToDoManagementSystem.IntegrationTests
dotnet test tests/ToDoManagementSystem.E2ETests

# Run a single test by name
dotnet test tests/ToDoManagementSystem.UnitTests --filter "FullyQualifiedName~CreateTaskCommandHandlerTests"

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> \
  --project src/ToDoManagementSystem.Persistence \
  --startup-project src/ToDoManagementSystem.API \
  --output-dir Migrations

# Apply migrations manually
dotnet ef database update \
  --project src/ToDoManagementSystem.Persistence \
  --startup-project src/ToDoManagementSystem.API

# Docker (API on :8080, SQL Server on :1433)
docker-compose up
```

The dev database is a remote SQL Server at `192.168.1.253` — connection string is in `src/ToDoManagementSystem.API/appsettings.json`. Integration tests use an in-memory EF provider and do not hit the real DB.

---

## Architecture

Clean Architecture in strict dependency order:

```
Shared → Domain → Application → Persistence
                             → Infrastructure
                             → API
```

**Shared** — `ApiResponse<T>`, `PagedResponse<T>`, `AppConstants`, `DateTimeHelper`. No dependencies on other layers.

**Domain** — entities (`User`, `TaskItem`, `Project`, `RefreshToken`), enums (`TaskPriority`, `TaskStatus`, `UserRole`), custom exceptions (`NotFoundException`, `UnauthorizedException`, `ValidationException`). All entities extend `BaseEntity` (Guid Id, CreatedDate, UpdatedDate — auto-set in `SaveChangesAsync`).

**Application** — all business logic. Organised as:
- `Features/{Area}/Commands/` and `Features/{Area}/Queries/` — MediatR `IRequest<T>` records
- `Features/{Area}/Handlers/` — `IRequestHandler<TRequest, TResponse>` implementations
- `Interfaces/` — repository and service contracts (no implementations here)
- `Validators/` — FluentValidation validators for all DTOs
- `Behaviors/` — MediatR pipeline: `ValidationBehavior` (runs validators before handlers), `LoggingBehavior`
- `Mappings/` — AutoMapper profiles

**Persistence** — EF Core implementations. `AppDbContext` is in `Context/`. Entity configurations use Fluent API in `Configurations/` (auto-discovered via `ApplyConfigurationsFromAssembly`) — no DataAnnotations on entities. Repository implementations are in `Repositories/`. Two migrations exist: `InitialCreate` (Users/Tasks/RefreshTokens) and `AddProjectModule` (Projects).

**Infrastructure** — cross-cutting concerns: `JwtTokenService` (HS256 JWT + refresh token generation), `EmailService` (MailKit/SMTP), `ReminderJobService` (Hangfire background job, hourly).

**API** — controllers, Razor Pages frontend, middleware. Controllers inject only `IMediator` — no repositories or services directly. Razor Pages (`Pages/`) provide the browser UI backed by the same API.

---

## Key Patterns

### Adding a new feature
1. Add command/query record in `Application/Features/{Area}/Commands/` or `/Queries/`
2. Add handler in `Application/Features/{Area}/Handlers/`
3. Add FluentValidation validator in `Application/Validators/` (required for all input DTOs)
4. Add controller action that calls `_mediator.Send(...)` and returns `ApiResponse<T>.Ok(...)`
5. Add unit test in `tests/ToDoManagementSystem.UnitTests/Features/{Area}/`

### CQRS dispatch
```csharp
// All controllers follow this pattern
var result = await _mediator.Send(new GetAllTasksQuery(userId), ct);
return Ok(ApiResponse<IEnumerable<TaskResponse>>.Ok(result));
```

### Extracting the current user in controllers
```csharp
private Guid GetCurrentUserId() =>
    Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

### Soft deletes
`TaskItem` and `Project` have a global EF query filter `!t.IsDeleted` applied in their `IEntityTypeConfiguration` class. Set `IsDeleted = true` to delete — never call `repository.Delete()` on tasks. Hard deletes are intentional only for `User` cascades.

### Unit of Work
Use `IUnitOfWork` (not repositories directly) when a handler needs to persist across multiple entities in a single transaction. Call `await _unitOfWork.SaveChangesAsync(ct)` — not `DbContext.SaveChangesAsync` — so audit timestamps are managed centrally.

### Razor Pages frontend
Razor Page Models are intentionally thin (most have empty `OnGet()`). All data loading and mutations happen client-side via `apiFetch(path, options)`, a helper defined in `_Layout.cshtml` that:
- Reads `access_token` from `localStorage`
- Adds `Authorization: Bearer <token>` to every request
- Handles 401 by redirecting to `/login`

Authentication state (tokens + user info) lives in `localStorage`, not server-side session. Adding a new protected page means adding client-side auth check and using `apiFetch` for all API calls — the Razor Page Model itself needs no auth logic.

### JWT token lifecycle
- **Access token** — HS256 JWT, 60-minute expiry, claims: `NameIdentifier`, `Email`, `Name`, `Role`, `Jti`
- **Refresh token** — 64-byte random value, Base64-encoded, 7-day expiry, stored in the `RefreshTokens` DB table (never in JWT claims)
- **Refresh flow** — `GetPrincipalFromExpiredToken()` validates the token signature without checking lifetime; the DB token is then rotated

### Background jobs (Hangfire)
- `ReminderJobService.SendUpcomingDueRemindersAsync` — runs hourly (`0 * * * *`), emails users whose tasks are due within 24 hours
- `ReminderJobService.LogOverdueTasksAsync` — runs daily, logs overdue task counts
- Both carry `[AutomaticRetry(Attempts = 3)]`. Use `IServiceScopeFactory` when injecting scoped services into Hangfire jobs (as shown in `ReminderJobService`).
- Dashboard: `/hangfire`

---

## Non-Negotiables

1. **Never use `var` for ambiguous types** — always use explicit types
2. **Every public method needs XML doc comments** (`/// <summary>`)
3. **All async methods use `CancellationToken`** — pass it through to EF calls
4. **FluentValidation for ALL input DTOs** — no `[Required]` DataAnnotations
5. **Serilog structured logging** — use `Log.ForContext<T>()` not `ILogger<T>` directly
6. **Global exception handler** via `ExceptionHandlingMiddleware` — no try/catch in controllers or handlers
7. **Soft deletes only** on `Tasks` and `Projects` — set `IsDeleted = true`
8. **All endpoints return `ApiResponse<T>`** — never return raw objects or plain `Ok(data)`
9. **`DateTime.UtcNow`** everywhere — never `DateTime.Now`
10. **Refresh tokens stored in DB only** — never put them in JWT claims
11. **Passwords hashed with BCrypt** at work factor 12 — never store plain text or use weaker hashing

---

## Testing Conventions

- Test class names: `{Feature}CommandHandlerTests` / `{Feature}QueryHandlerTests`
- Use `Microsoft.EntityFrameworkCore.InMemory` for handler tests that need a real DbContext
- Use `Moq` for repository/service mocks
- `FluentAssertions` for all assertions
- Integration tests use `CustomWebApplicationFactory` (in `tests/ToDoManagementSystem.IntegrationTests/`) with in-memory EF and `Hangfire.InMemory` — no SQL Server required; each factory instance gets a unique in-memory DB name to avoid cross-test contamination
- E2E tests use Playwright and require the API to be running

---

## Configuration

`appsettings.json` keys that must be overridden in production via environment variables:

| Env var | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Key` | HS256 signing key (32+ chars) |
| `Email__SmtpPassword` | SMTP credentials |

Rate limiting: `FixedWindowLimiter` — 100 requests per 60-second window per IP (configured in `appsettings.json` under `RateLimit`).

Hangfire dashboard is at `/hangfire`. Health check is at `/health`. Swagger UI is at `/swagger` (all environments).

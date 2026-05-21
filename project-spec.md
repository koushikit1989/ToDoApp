# project-spec.md — Claude Code Prompt

## Project: To-Do Management System
**Type**: Production-Ready REST API  
**Framework**: ASP.NET Core 8  
**Architecture**: Clean Architecture (5 layers)  
**Deploy Target**: Azure App Service / Docker / IIS

---

## Prompt for Claude Code

You are building a **complete, production-ready To-Do Management System** as an ASP.NET Core 8 Web API. Every file must compile, every endpoint must work, and the app must be deployable with `dotnet publish`. Follow the architecture and requirements below precisely.

---

## What to Build

### 1. Create the Solution Structure

```bash
dotnet new sln -n ToDoManagementSystem
dotnet new webapi -n ToDoManagementSystem.API -o src/ToDoManagementSystem.API
dotnet new classlib -n ToDoManagementSystem.Application -o src/ToDoManagementSystem.Application
dotnet new classlib -n ToDoManagementSystem.Domain -o src/ToDoManagementSystem.Domain
dotnet new classlib -n ToDoManagementSystem.Infrastructure -o src/ToDoManagementSystem.Infrastructure
dotnet new classlib -n ToDoManagementSystem.Persistence -o src/ToDoManagementSystem.Persistence
dotnet new classlib -n ToDoManagementSystem.Shared -o src/ToDoManagementSystem.Shared
dotnet new xunit -n ToDoManagementSystem.UnitTests -o tests/ToDoManagementSystem.UnitTests
dotnet sln add src/*/
dotnet sln add tests/*/
```

Set up project references in order:
- Domain → Shared
- Application → Domain, Shared
- Persistence → Application, Domain
- Infrastructure → Application, Domain, Shared
- API → Application, Infrastructure, Persistence, Shared

---

### 2. Domain Layer (`ToDoManagementSystem.Domain`)

Create these files with full implementation:

**`Common/BaseEntity.cs`**
- Abstract base class with audit properties

**`Enums/TaskPriority.cs`**
```csharp
public enum TaskPriority { Low = 1, Medium = 2, High = 3 }
```

**`Enums/TaskStatus.cs`**
```csharp
public enum TaskStatus { Pending = 0, InProgress = 1, Completed = 2 }
```

**`Enums/UserRole.cs`**
```csharp
public enum UserRole { User = 0, Admin = 1 }
```

**`Entities/User.cs`** — full entity with navigation properties

**`Entities/TaskItem.cs`** — full entity with navigation properties

**`Entities/RefreshToken.cs`** — JWT refresh token entity

**`Exceptions/`** — NotFoundException, UnauthorizedException, ValidationException (all inherit from Exception)

---

### 3. Shared Layer (`ToDoManagementSystem.Shared`)

**`Responses/ApiResponse.cs`** — Generic wrapper with static factory methods `Ok()`, `Fail()`, `Created()`

**`Responses/PagedResponse.cs`** — Paginated result wrapper with TotalCount, PageNumber, PageSize, TotalPages

**`Constants/AppConstants.cs`** — JWT claim names, role strings, pagination defaults

**`Helpers/DateTimeHelper.cs`** — UTC helpers, due date formatting

---

### 4. Application Layer (`ToDoManagementSystem.Application`)

#### Interfaces
- `IRepository<T>` — generic CRUD interface
- `ITaskRepository : IRepository<TaskItem>` — + filter, search, overdue methods
- `IUserRepository : IRepository<User>` — + GetByEmailAsync
- `ITokenService` — GenerateAccessToken, GenerateRefreshToken, GetPrincipalFromExpiredToken
- `IEmailService` — SendPasswordResetEmailAsync, SendReminderEmailAsync
- `IUnitOfWork` — SaveChangesAsync, BeginTransactionAsync, CommitAsync, RollbackAsync

#### DTOs
Create all request/response DTOs for:
- Auth: RegisterRequest, LoginRequest, LoginResponse (includes both tokens), RefreshTokenRequest, ForgotPasswordRequest, ResetPasswordRequest
- Tasks: CreateTaskRequest, UpdateTaskRequest, TaskResponse, TaskFilterRequest (status, priority, dueDateFrom, dueDateTo, searchTerm), PagedTasksResponse
- Dashboard: DashboardSummaryResponse (totalTasks, completedTasks, pendingTasks, inProgressTasks, overdueTasks), TasksByPriorityResponse, ReportResponse

#### Validators (FluentValidation)
- RegisterRequestValidator: email format, password strength (8+ chars, uppercase, lowercase, digit, special), fullName max 200
- LoginRequestValidator: email format, password required
- CreateTaskRequestValidator: title required max 300, priority 1-3, dueDate >= today
- UpdateTaskRequestValidator: same as create but all fields optional

#### CQRS Commands & Queries (MediatR)

**Auth Commands:**
- `RegisterUserCommand` → `LoginResponse`
- `LoginCommand` → `LoginResponse`
- `RefreshTokenCommand` → `LoginResponse`
- `ForgotPasswordCommand` → `bool`
- `ResetPasswordCommand` → `bool`

**Task Commands:**
- `CreateTaskCommand` → `TaskResponse`
- `UpdateTaskCommand` → `TaskResponse`
- `DeleteTaskCommand` → `bool`
- `MarkTaskStatusCommand` → `TaskResponse`

**Task Queries:**
- `GetTaskByIdQuery` → `TaskResponse`
- `GetAllTasksQuery` → `PagedResponse<TaskResponse>`
- `GetFilteredTasksQuery` → `PagedResponse<TaskResponse>`

**Dashboard Queries:**
- `GetDashboardSummaryQuery` → `DashboardSummaryResponse`
- `GetReportsQuery` → `ReportResponse`

#### Handlers — implement ALL handlers with full business logic

#### Behaviors
- `ValidationBehavior<TRequest, TResponse>` — runs FluentValidation before handler, throws if invalid
- `LoggingBehavior<TRequest, TResponse>` — logs command name, execution time using Serilog

#### AutoMapper Profiles
- `TaskMappingProfile` — TaskItem ↔ TaskResponse, CreateTaskRequest → TaskItem
- `UserMappingProfile` — User ↔ RegisterRequest

#### `DependencyInjection.cs`
Register MediatR, AutoMapper, FluentValidation, behaviors.

---

### 5. Persistence Layer (`ToDoManagementSystem.Persistence`)

**`Context/AppDbContext.cs`**
- Inherit `DbContext`
- DbSet for Users, Tasks (TaskItems), RefreshTokens
- Override `SaveChangesAsync` to set `UpdatedDate` automatically
- Apply all configurations from assembly

**`Configurations/UserConfiguration.cs`**
- Fluent API, no DataAnnotations
- Email: unique index, max 200
- PasswordHash: required, no max length

**`Configurations/TaskConfiguration.cs`**
- Global query filter: `!t.IsDeleted`
- Title: max 300, required
- Enum conversions: Priority and Status stored as int

**`Configurations/RefreshTokenConfiguration.cs`**
- Token: unique index, max 500

**`Repositories/GenericRepository.cs`** — implements `IRepository<T>` using EF Core

**`Repositories/TaskRepository.cs`** — implements `ITaskRepository` with LINQ filters for status, priority, date range, soft delete, search

**`Repositories/UserRepository.cs`** — implements `IUserRepository` with email lookup

**`UnitOfWork.cs`** — wraps DbContext SaveChanges and transactions

**`DependencyInjection.cs`** — registers DbContext (SQL Server), repositories, UnitOfWork

---

### 6. Infrastructure Layer (`ToDoManagementSystem.Infrastructure`)

**`Authentication/JwtTokenService.cs`**
- Implements `ITokenService`
- `GenerateAccessToken(User user)` → JWT with claims: NameIdentifier, Email, Name, Role, jti
- `GenerateRefreshToken()` → cryptographically random 64-byte base64 string
- `GetPrincipalFromExpiredToken(string token)` → `ClaimsPrincipal` (for refresh)
- Uses `IConfiguration` for JWT settings

**`Email/EmailService.cs`**
- Implements `IEmailService`
- Uses MailKit / SMTP
- Templates for password reset and task reminders

**`BackgroundJobs/ReminderJobService.cs`**
- Implements `IHostedService` OR Hangfire recurring job
- Every hour: check tasks with DueDate within 24 hours and Status != Completed
- Every day at midnight: flag overdue tasks

**`DependencyInjection.cs`** — registers JwtTokenService, EmailService, Hangfire, MemoryCache

---

### 7. API Layer (`ToDoManagementSystem.API`)

**`Middleware/ExceptionHandlingMiddleware.cs`**
Handle these exceptions → HTTP status codes:
- `NotFoundException` → 404
- `UnauthorizedException` → 401
- `ValidationException` → 400
- `Exception` (fallback) → 500
All return `ApiResponse<object>` with error details. Log all 5xx with Serilog.

**`Middleware/RateLimitingMiddleware.cs`** — or use ASP.NET Core built-in `AddRateLimiter` with fixed window policy: 100 requests/minute per IP.

**`Controllers/AuthController.cs`**
```
POST /api/auth/register        → RegisterUserCommand
POST /api/auth/login           → LoginCommand
POST /api/auth/refresh-token   → RefreshTokenCommand
POST /api/auth/forgot-password → ForgotPasswordCommand
POST /api/auth/reset-password  → ResetPasswordCommand
```
All endpoints: `[AllowAnonymous]`, return `ApiResponse<T>`

**`Controllers/TasksController.cs`**
```
GET    /api/tasks              → GetAllTasksQuery (paginated, query params: page, pageSize)
GET    /api/tasks/{id}         → GetTaskByIdQuery
POST   /api/tasks              → CreateTaskCommand
PUT    /api/tasks/{id}         → UpdateTaskCommand
DELETE /api/tasks/{id}         → DeleteTaskCommand
GET    /api/tasks/filter       → GetFilteredTasksQuery (query params: status, priority, dueDateFrom, dueDateTo)
GET    /api/tasks/search       → GetFilteredTasksQuery with searchTerm
PATCH  /api/tasks/{id}/status  → MarkTaskStatusCommand
```
All endpoints: `[Authorize]`, extract userId from JWT claims, return `ApiResponse<T>`

**`Controllers/DashboardController.cs`**
```
GET /api/dashboard/summary        → GetDashboardSummaryQuery
GET /api/dashboard/reports        → GetReportsQuery
GET /api/dashboard/export?format= → export Excel (ClosedXML) or PDF
```
All endpoints: `[Authorize]`

**`Extensions/SwaggerExtensions.cs`**
Configure Swagger with JWT Bearer authentication support (so the "Authorize" button appears in Swagger UI).

**`Extensions/HealthCheckExtensions.cs`**
Add SQL Server health check, custom "self" health check.

**`Program.cs`** — complete, runnable setup as described in CLAUDE.md

**`appsettings.json`** — all config sections: ConnectionStrings, Jwt, Serilog, Hangfire, Email, RateLimit

**`appsettings.Development.json`** — localdb connection, console-only logging

---

### 8. Unit Tests (`ToDoManagementSystem.UnitTests`)

Write tests for:

**`Features/Auth/RegisterUserCommandHandlerTests.cs`**
- Happy path: valid registration creates user and returns tokens
- Duplicate email: throws or returns error
- Weak password: validation fails

**`Features/Tasks/CreateTaskCommandHandlerTests.cs`**
- Valid task created successfully
- Past due date: validation fails
- Title too long: validation fails

**`Features/Tasks/GetAllTasksQueryHandlerTests.cs`**
- Returns only current user's tasks
- Pagination works correctly

**`Validators/CreateTaskRequestValidatorTests.cs`**
- All validation rules tested individually

---

## Important Implementation Notes

### User Isolation
Every task query **must** filter by `UserId` from the JWT claims. Users can never see or modify other users' tasks. The handler must extract `userId` from the command/query (set by the controller from JWT claims) and always include it in EF queries.

### Password Reset Flow
1. `ForgotPasswordCommand`: generate a secure random token, store hash in DB (add `PasswordResetToken` and `PasswordResetExpiry` to User entity), send email with reset link
2. `ResetPasswordCommand`: validate token, check expiry, hash new password with BCrypt, clear token fields

### Refresh Token Flow
1. On login: generate refresh token, store in `RefreshTokens` table with `ExpiresAt = 7 days`
2. On `RefreshTokenCommand`: validate token exists and not expired/revoked, revoke old token, issue new access + refresh token pair
3. On logout (optional endpoint): revoke refresh token

### Dashboard Summary Query
```csharp
// Single efficient DB query using GroupBy or multiple counts
var tasks = await context.Tasks
    .Where(t => t.UserId == userId)
    .GroupBy(t => 1)
    .Select(g => new DashboardSummaryResponse
    {
        TotalTasks = g.Count(),
        CompletedTasks = g.Count(t => t.Status == TaskStatus.Completed),
        PendingTasks = g.Count(t => t.Status == TaskStatus.Pending),
        InProgressTasks = g.Count(t => t.Status == TaskStatus.InProgress),
        OverdueTasks = g.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed)
    })
    .FirstOrDefaultAsync(ct);
```

### Report Export
- Excel: use `ClosedXML` — create workbook with styled header row, data rows, auto-fit columns
- Return as `FileContentResult` with proper content type

---

## Final Verification Checklist

After generating all files, verify:

- [ ] `dotnet build` succeeds with zero errors and zero warnings
- [ ] `dotnet ef migrations add InitialCreate` succeeds
- [ ] All controllers have `[ApiController]` and `[Route]` attributes
- [ ] All protected endpoints have `[Authorize]` attribute
- [ ] Auth endpoints have `[AllowAnonymous]` attribute
- [ ] Every repository method has a corresponding interface method
- [ ] `DependencyInjection.cs` in each layer registers all services
- [ ] `Program.cs` calls all `Add*Services()` extension methods
- [ ] Swagger UI shows JWT bearer auth button
- [ ] Health check endpoint returns 200 at `/health`
- [ ] All DTOs have corresponding FluentValidation validators
- [ ] AutoMapper profiles cover all entity ↔ DTO mappings
- [ ] No `DateTime.Now` — only `DateTime.UtcNow`
- [ ] No hardcoded connection strings or secrets in code

---

## Quick Start (after generation)

```bash
# 1. Restore packages
dotnet restore

# 2. Update connection string in appsettings.Development.json

# 3. Run migrations
dotnet ef database update \
  --project src/ToDoManagementSystem.Persistence \
  --startup-project src/ToDoManagementSystem.API

# 4. Run the API
cd src/ToDoManagementSystem.API
dotnet run

# 5. Open Swagger
# https://localhost:5001/swagger

# 6. Run tests
dotnet test
```

---

## Docker Support (Optional)

Generate a `Dockerfile` for the API project:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj", "src/ToDoManagementSystem.API/"]
# ... COPY all project files
RUN dotnet restore "src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj"
COPY . .
RUN dotnet build "src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ToDoManagementSystem.API/ToDoManagementSystem.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoManagementSystem.API.dll"]
```

Also generate `docker-compose.yml` with API + SQL Server services.

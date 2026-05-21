# SPEC.md — To-Do Management System
## Technical Specification for Claude Code

---

## Project Overview

A production-ready, full-stack To-Do Management System built with ASP.NET Core 8 following Clean Architecture principles. The system provides secure task management with JWT authentication, role-based access, background notifications, and reporting.

---

## Technology Stack

| Component | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.x |
| Database | SQL Server | 2022 / LocalDB (dev) |
| Auth | JWT Bearer + Refresh Tokens | — |
| API Docs | Swagger / OpenAPI | Swashbuckle 6.x |
| Logging | Serilog | 3.x |
| Validation | FluentValidation | 11.x |
| Mapping | AutoMapper | 12.x |
| CQRS | MediatR | 12.x |
| Background Jobs | Hangfire | 1.8.x |
| Caching | MemoryCache (default) / Redis (optional) |
| Unit Tests | xUnit + Moq | — |
| Password Hash | BCrypt.Net-Next | — |

---

## Solution Structure

```
ToDoManagementSystem/
├── src/
│   ├── ToDoManagementSystem.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── TasksController.cs
│   │   │   └── DashboardController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Filters/
│   │   │   └── ValidationFilter.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── ApplicationBuilderExtensions.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── Program.cs
│   │
│   ├── ToDoManagementSystem.Application/
│   │   ├── Interfaces/
│   │   │   ├── ITaskRepository.cs
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ITokenService.cs
│   │   │   ├── IEmailService.cs
│   │   │   └── IUnitOfWork.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── RegisterRequest.cs
│   │   │   │   ├── LoginRequest.cs
│   │   │   │   ├── LoginResponse.cs
│   │   │   │   ├── RefreshTokenRequest.cs
│   │   │   │   ├── ForgotPasswordRequest.cs
│   │   │   │   └── ResetPasswordRequest.cs
│   │   │   ├── Tasks/
│   │   │   │   ├── CreateTaskRequest.cs
│   │   │   │   ├── UpdateTaskRequest.cs
│   │   │   │   ├── TaskResponse.cs
│   │   │   │   └── TaskFilterRequest.cs
│   │   │   └── Dashboard/
│   │   │       ├── DashboardSummaryResponse.cs
│   │   │       └── ReportResponse.cs
│   │   ├── Features/
│   │   │   ├── Authentication/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   │   ├── ForgotPasswordCommand.cs
│   │   │   │   │   └── ResetPasswordCommand.cs
│   │   │   │   └── Handlers/
│   │   │   │       ├── RegisterUserCommandHandler.cs
│   │   │   │       ├── LoginCommandHandler.cs
│   │   │   │       └── ...
│   │   │   ├── Tasks/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateTaskCommand.cs
│   │   │   │   │   ├── UpdateTaskCommand.cs
│   │   │   │   │   └── DeleteTaskCommand.cs
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetTaskByIdQuery.cs
│   │   │   │   │   ├── GetAllTasksQuery.cs
│   │   │   │   │   └── GetFilteredTasksQuery.cs
│   │   │   │   └── Handlers/
│   │   │   │       └── ...
│   │   │   └── Dashboard/
│   │   │       ├── Queries/
│   │   │       │   ├── GetDashboardSummaryQuery.cs
│   │   │       │   └── GetReportsQuery.cs
│   │   │       └── Handlers/
│   │   │           └── ...
│   │   ├── Validators/
│   │   │   ├── RegisterRequestValidator.cs
│   │   │   ├── LoginRequestValidator.cs
│   │   │   ├── CreateTaskRequestValidator.cs
│   │   │   └── UpdateTaskRequestValidator.cs
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── LoggingBehavior.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── ToDoManagementSystem.Domain/
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── TaskItem.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   ├── TaskPriority.cs
│   │   │   ├── TaskStatus.cs
│   │   │   └── UserRole.cs
│   │   ├── Common/
│   │   │   └── BaseEntity.cs
│   │   └── Exceptions/
│   │       ├── NotFoundException.cs
│   │       ├── UnauthorizedException.cs
│   │       └── ValidationException.cs
│   │
│   ├── ToDoManagementSystem.Infrastructure/
│   │   ├── Authentication/
│   │   │   └── JwtTokenService.cs
│   │   ├── Email/
│   │   │   └── EmailService.cs
│   │   ├── Logging/
│   │   │   └── SerilogConfiguration.cs
│   │   ├── BackgroundJobs/
│   │   │   └── ReminderJobService.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── ToDoManagementSystem.Persistence/
│   │   ├── Context/
│   │   │   └── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   └── TaskConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── GenericRepository.cs
│   │   │   ├── TaskRepository.cs
│   │   │   └── UserRepository.cs
│   │   ├── Migrations/
│   │   └── DependencyInjection.cs
│   │
│   └── ToDoManagementSystem.Shared/
│       ├── Constants/
│       │   └── AppConstants.cs
│       ├── Helpers/
│       │   └── DateTimeHelper.cs
│       └── Responses/
│           ├── ApiResponse.cs
│           └── PagedResponse.cs
│
└── tests/
    ├── ToDoManagementSystem.UnitTests/
    │   ├── Features/
    │   │   ├── Auth/
    │   │   └── Tasks/
    │   └── Validators/
    └── ToDoManagementSystem.IntegrationTests/
```

---

## Domain Entities

### User Entity
```csharp
public class User : BaseEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }       // "Admin" | "User"
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public ICollection<TaskItem> Tasks { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

### TaskItem Entity
```csharp
public class TaskItem : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskPriority Priority { get; set; }    // Low=1, Medium=2, High=3
    public Domain.Enums.TaskStatus Status { get; set; } // Pending=0, InProgress=1, Completed=2
    public DateTime DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public User User { get; set; }
}
```

### RefreshToken Entity
```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; }
}
```

---

## Database Schema (MSSQL)

```sql
CREATE TABLE Users (
    Id           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName     NVARCHAR(200)    NOT NULL,
    Email        NVARCHAR(200)    NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX)    NOT NULL,
    Role         NVARCHAR(50)     NOT NULL DEFAULT 'User',
    CreatedDate  DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    IsActive     BIT              NOT NULL DEFAULT 1
);

CREATE TABLE Tasks (
    Id          UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId      UNIQUEIDENTIFIER NOT NULL,
    Title       NVARCHAR(300)    NOT NULL,
    Description NVARCHAR(MAX),
    Priority    INT              NOT NULL DEFAULT 1,
    Status      INT              NOT NULL DEFAULT 0,
    DueDate     DATETIME2        NOT NULL,
    CreatedDate DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2,
    IsDeleted   BIT              NOT NULL DEFAULT 0,
    CONSTRAINT FK_Tasks_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE RefreshTokens (
    Id        UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId    UNIQUEIDENTIFIER NOT NULL,
    Token     NVARCHAR(500)    NOT NULL UNIQUE,
    ExpiresAt DATETIME2        NOT NULL,
    IsRevoked BIT              NOT NULL DEFAULT 0,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

---

## API Endpoints

### Auth
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | /api/auth/register | ❌ | Register new user |
| POST | /api/auth/login | ❌ | Login, returns JWT + refresh token |
| POST | /api/auth/refresh-token | ❌ | Exchange refresh token for new JWT |
| POST | /api/auth/forgot-password | ❌ | Send reset email |
| POST | /api/auth/reset-password | ❌ | Reset password with token |

### Tasks
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | /api/tasks | ✅ | Get all tasks (paginated) |
| GET | /api/tasks/{id} | ✅ | Get task by ID |
| POST | /api/tasks | ✅ | Create task |
| PUT | /api/tasks/{id} | ✅ | Update task |
| DELETE | /api/tasks/{id} | ✅ | Soft delete task |
| GET | /api/tasks/filter | ✅ | Filter by status/priority/date |
| GET | /api/tasks/search?q= | ✅ | Search by title/description |

### Dashboard
| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | /api/dashboard/summary | ✅ | Total/Completed/Pending/Overdue counts |
| GET | /api/dashboard/reports | ✅ | Detailed report data |
| GET | /api/dashboard/export?format=pdf | ✅ | Export report |

---

## Security Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyWith32+CharactersHere!",
    "Issuer": "ToDoManagementSystem",
    "Audience": "ToDoManagementSystemUsers",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ToDoManagementSystem;Trusted_Connection=true;"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  },
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "ReminderJobCronExpression": "0 * * * *"
  }
}
```

---

## Validation Rules

### Registration
- Email: required, valid format, unique
- Password: required, min 8 chars, must contain uppercase, lowercase, digit, special char
- FullName: required, max 200 chars

### Task Creation
- Title: required, max 300 chars
- Priority: must be 1 (Low), 2 (Medium), or 3 (High)
- DueDate: must be today or future date
- Description: optional, max 2000 chars

---

## Error Response Format

All errors return consistent JSON:
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Email is already registered", "Password too weak"],
  "statusCode": 400,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

---

## Background Jobs (Hangfire)

- **ReminderJob**: Runs every hour — queries tasks with DueDate within 24 hours and Status != Completed, marks them as needing reminder
- **OverdueJob**: Runs daily at midnight — marks tasks past DueDate as overdue in logs/notifications

---

## Non-Functional Targets

| Concern | Target |
|---|---|
| API response time | < 300ms p95 |
| JWT expiry | 60 minutes |
| Refresh token expiry | 7 days |
| Password hashing | BCrypt work factor 12 |
| Rate limiting | 100 req/min per IP |
| Soft deletes | All task deletes are soft (IsDeleted flag) |
| Pagination default | 20 items per page |

# CLAUDE.md — Claude Code Instructions

This file guides Claude Code when building the **To-Do Management System**.

---

## Project Goal

Build a **production-ready**, **deployable** ASP.NET Core 8 Web API following Clean Architecture. Every layer must compile, all NuGet packages must be pinned, and the app must run with `dotnet run` after a single `dotnet ef database update`.

---

## Non-Negotiables

1. **Never use `var` for ambiguous types** — always use explicit types for readability
2. **Every public method needs XML doc comments** (`/// <summary>`)
3. **No hardcoded secrets** — all config via `appsettings.json` and environment variables
4. **All async methods use `CancellationToken`** where applicable
5. **No direct DbContext injection in Controllers** — always go through repositories/CQRS
6. **FluentValidation for ALL input DTOs** — no `[Required]` DataAnnotations
7. **Serilog structured logging** — use `Log.ForContext<T>()` not `ILogger<T>` directly
8. **Global exception handler** via middleware — no try/catch in controllers
9. **Soft deletes only** — never `DELETE` rows from Tasks table; set `IsDeleted = true`
10. **All endpoints return `ApiResponse<T>`** wrapper — never raw objects

---

## Build Order

Follow this exact order to avoid circular dependency issues:

```
1. ToDoManagementSystem.Shared
2. ToDoManagementSystem.Domain
3. ToDoManagementSystem.Application
4. ToDoManagementSystem.Persistence
5. ToDoManagementSystem.Infrastructure
6. ToDoManagementSystem.API
7. Tests (UnitTests, IntegrationTests)
```

---

## Key Patterns

### CQRS with MediatR
- All business logic lives in `Application/Features/*/Commands/` and `Application/Features/*/Queries/`
- Controllers only dispatch commands/queries via `IMediator`
- Use `IRequest<T>` for commands/queries and `IRequestHandler<TRequest, TResponse>` for handlers

### Repository Pattern
```csharp
// Generic base
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}

// Specific repositories extend the generic
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetFilteredAsync(Guid userId, TaskFilterRequest filter, CancellationToken ct = default);
}
```

### ApiResponse Wrapper
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success") => new() { Success = true, Data = data, Message = message, StatusCode = 200 };
    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null) => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new() };
}
```

---

## NuGet Packages (exact versions)

### All projects
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
```

### Application project
```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
<PackageReference Include="FluentValidation" Version="11.9.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
```

### Infrastructure project
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.4.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="MailKit" Version="4.5.0" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.9" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.9" />
```

### Persistence project
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

### API project
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.RateLimiting" Version="8.0.0" />
<PackageReference Include="ClosedXML" Version="0.102.2" />
```

### Test projects
```xml
<PackageReference Include="xunit" Version="2.7.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.7" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

---

## Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskResponse>>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetAllTasksQuery(userId), ct);
        return Ok(ApiResponse<IEnumerable<TaskResponse>>.Ok(result));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
```

---

## JWT Implementation

```csharp
// Claims to include in JWT
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Name, user.FullName),
new Claim(ClaimTypes.Role, user.Role),
new Claim("jti", Guid.NewGuid().ToString())
```

---

## EF Core Configuration

Use **Fluent API** in `IEntityTypeConfiguration<T>` classes — no DataAnnotations on entities.

```csharp
// Persistence/Configurations/TaskConfiguration.cs
public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(300);
        builder.HasQueryFilter(t => !t.IsDeleted); // Global soft delete filter
        builder.HasOne(t => t.User)
               .WithMany(u => u.Tasks)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Important**: Apply global query filter `!t.IsDeleted` on TaskItem so soft-deleted records are automatically excluded.

---

## AutoMapper Profiles

Create mapping profiles in `Application/` layer:

```csharp
public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
        CreateMap<CreateTaskRequest, TaskItem>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

---

## Dependency Injection Setup

Each project has its own `DependencyInjection.cs` with an extension method:

```csharp
// Called in Program.cs
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
```

---

## Program.cs Structure

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Register layers
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // with JWT bearer config
builder.Services.AddAuthentication(); // JWT Bearer
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(); // 100 req/min per IP
builder.Services.AddHangfire();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks().AddSqlServer(connectionString);

var app = builder.Build();

// Middleware pipeline (ORDER MATTERS)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.UseHangfireDashboard("/hangfire");
app.UseSwagger();
app.UseSwaggerUI();

// Auto-migrate on startup (dev only — guard with env check)
if (app.Environment.IsDevelopment())
    await app.MigrateDatabase();

app.Run();
```

---

## Migrations

After implementing all entities and configurations:

```bash
# From solution root
dotnet ef migrations add InitialCreate \
  --project src/ToDoManagementSystem.Persistence \
  --startup-project src/ToDoManagementSystem.API \
  --output-dir Migrations

dotnet ef database update \
  --project src/ToDoManagementSystem.Persistence \
  --startup-project src/ToDoManagementSystem.API
```

---

## Testing Conventions

- Test class naming: `{Feature}CommandHandlerTests`, `{Feature}QueryHandlerTests`
- Use `InMemory` EF provider for handler tests
- Use `Moq` for service/repository mocks
- `FluentAssertions` for readable assertions
- Each test: Arrange / Act / Assert sections with comments

```csharp
[Fact]
public async Task Handle_ValidRequest_ReturnsCreatedTask()
{
    // Arrange
    var command = new CreateTaskCommand { ... };
    _mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>(), default)).Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be(command.Title);
}
```

---

## Deployment Checklist

- [ ] All `appsettings.json` secrets replaced with environment variables
- [ ] `ASPNETCORE_ENVIRONMENT=Production` set
- [ ] SQL Server connection string updated
- [ ] JWT secret key set via env var `Jwt__Key`
- [ ] HTTPS certificate configured
- [ ] Health check endpoint `/health` accessible
- [ ] Hangfire dashboard protected with auth
- [ ] Serilog writing to persistent log storage
- [ ] Database migrations applied: `dotnet ef database update`

---

## Common Mistakes to Avoid

- ❌ Don't inject `AppDbContext` directly into controllers
- ❌ Don't use `DateTime.Now` — always use `DateTime.UtcNow`
- ❌ Don't return plain `200 OK` with raw objects — always wrap in `ApiResponse<T>`
- ❌ Don't skip `cancellationToken` parameters in async EF calls
- ❌ Don't store refresh tokens in JWT claims — store in DB only
- ❌ Don't catch `Exception` broadly in handlers — let the middleware handle it
- ❌ Don't skip the global query filter for soft deletes

using System.Threading.RateLimiting;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using ToDoManagementSystem.API.Extensions;
using ToDoManagementSystem.API.Middleware;
using ToDoManagementSystem.Application;
using ToDoManagementSystem.Infrastructure;
using ToDoManagementSystem.Infrastructure.BackgroundJobs;
using ToDoManagementSystem.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Application layers
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddAuthorization();

// Rate limiting — 100 requests per minute per IP
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        int permitLimit = builder.Configuration.GetValue<int>("RateLimit:PermitLimit", 100);
        int windowSeconds = builder.Configuration.GetValue<int>("RateLimit:WindowSeconds", 60);

        limiterOptions.PermitLimit = permitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
});

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        tags: new[] { "db", "sql" });

WebApplication app = builder.Build();

// Middleware pipeline (order matters)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.UseHangfireDashboard("/hangfire");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "To-Do Management System v1");
    options.RoutePrefix = "swagger";
});

// Register Hangfire recurring jobs
IRecurringJobManager recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
IServiceScopeFactory scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
ReminderJobService.RegisterRecurringJobs(recurringJobManager, scopeFactory);

// Auto-migrate on startup in Development only
if (app.Environment.IsDevelopment())
    await app.MigrateDatabase();

app.Run();

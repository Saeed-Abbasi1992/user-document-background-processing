using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserDocumentProcessor.API.HealthChecks;
using UserDocumentProcessor.API.Options;
using UserDocumentProcessor.Application;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Repositories;
using UserDocumentProcessor.Infrastructure;
using UserDocumentProcessor.Infrastructure.Persistence;
using UserDocumentProcessor.Infrastructure.Persistence.Repositories;
using UserDocumentProcessor.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<NotificationSettings>(
    builder.Configuration.GetSection("NotificationSettings")
);

var notificationSettings = builder.Configuration
    .GetSection("NotificationSettings")
    .Get<NotificationSettings>();


foreach (var channel in notificationSettings.EnabledChannels)
{
    switch (channel)
    {
        case UserDocumentProcessor.Application.Enums.NotificationChannelType.Console:
            builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
            break;

        case UserDocumentProcessor.Application.Enums.NotificationChannelType.Email:
            throw new NotSupportedException("not supported email channel yet...");
            // add sms later...
    }
}


// Database (EF Core)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Hangfire
builder.Services.AddHangfire(configuration =>
{
    configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                 .UseSimpleAssemblyNameTypeSerializer()
                 .UseRecommendedSerializerSettings()
                 .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
});
builder.Services.AddHangfireServer();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// DI: Repositories, UnitOfWork, Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IUnitOfWork, EFUnitOfWork>();
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorageSettings")
);
builder.Services.AddScoped<IFileStorageProvider, FileStorageProvider>();

builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddScoped<IFileCleanupJob, FileCleanupJob>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddProjectHealthChecks();

var app = builder.Build();

// Middleware
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserDocumentProcessor API V1");
        c.RoutePrefix = string.Empty;
    });
}

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Recurring Job: هر شب ساعت 00:00
RecurringJob.AddOrUpdate<IFileCleanupJob>(
    "nightly-file-cleanup",
    job => job.CleanupAsync(),
    "0 0 * * *"
);

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Name == "self",
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
        }));
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
        }));
    }
});

app.Run();

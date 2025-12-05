using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserDocumentProcessor.Application;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Repositories;
using UserDocumentProcessor.Infrastructure.Persistence;
using UserDocumentProcessor.Infrastructure.Persistence.Repositories;
using UserDocumentProcessor.Infrastructure.Services;

public static class TestServiceCollection
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging();

        // DbContext - InMemory for tests
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));

        // Repositories & UnitOfWork
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, EFUnitOfWork>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
        });

        // File Storage Provider
        services.AddScoped<IFileStorageProvider, FileStorageProvider>();

        // Mock Notification Services
        var mockNotification = new Mock<INotificationService>();
        services.AddSingleton(mockNotification.Object);

        // Mock Hangfire Job Client
        var mockJobClient = new Mock<IBackgroundJobClient>();
        services.AddSingleton(mockJobClient.Object);

        // Background Job Service
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // File Cleanup Job
        services.AddScoped<IFileCleanupJob, FileCleanupJob>();
        var mockCleanupLogger = new Mock<ILogger<FileCleanupJob>>();
        services.AddSingleton(mockCleanupLogger.Object);

        return services.BuildServiceProvider();
    }
}
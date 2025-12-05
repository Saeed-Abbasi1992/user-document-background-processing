using Microsoft.Extensions.DependencyInjection;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;

public class BackgroundJobAndCleanupTests
{
    private readonly ServiceProvider _serviceProvider;

    public BackgroundJobAndCleanupTests()
    {
        _serviceProvider = TestServiceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task FileCleanupJob_ShouldDeleteStaleAndOrphanFiles()
    {
        var fileStorage = _serviceProvider.GetRequiredService<IFileStorageProvider>();
        var cleanupJob = _serviceProvider.GetRequiredService<IFileCleanupJob>();
        var documentRepo = _serviceProvider.GetRequiredService<IDocumentRepository>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create dummy stale file in storage and DB
        var userId = Guid.NewGuid();
        var fileName = "stalefile.txt";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", userId.ToString(), fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        await File.WriteAllTextAsync(filePath, "stale content");

        var staleDocument = new DocumentEntity(userId, filePath, fileName);
        staleDocument.MarkFailed(); // Make it stale
        await documentRepo.AddAsync(staleDocument);
        await unitOfWork.SaveChangesAsync();

        // Create orphan file (not in DB)
        var orphanFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "orphan.txt");
        await File.WriteAllTextAsync(orphanFilePath, "orphan content");

        await cleanupJob.CleanupAsync();

        Assert.False(File.Exists(filePath)); // stale DB file deleted
        Assert.False(File.Exists(orphanFilePath)); // orphan file deleted
    }
}

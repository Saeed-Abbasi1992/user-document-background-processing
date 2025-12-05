using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Application.Users.Commands;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;

public class IntegrationTests
{
    private readonly ServiceProvider _serviceProvider;


    public IntegrationTests()
    {
        _serviceProvider = TestServiceCollection.BuildServiceProvider();
    }


    [Fact]
    public async Task RegisterUser_Workflow_IntegrationTest()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        var fileName = "testdoc.txt";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Hello World"));
        IFormFile formFile = new FormFile(fileContent, 0, fileContent.Length, "Document", fileName);

        var command = new RegisterUserCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Document = formFile
        };

        var result = await mediator.Send(command);

        Assert.NotNull(result);
        Assert.Equal("Registered", result.Status);
        Assert.Equal("User registered successfully.", result.Message);

        var savedPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", result.UserId.ToString(), fileName);
        Assert.True(File.Exists(savedPath));

        File.Delete(savedPath);
    }

    [Fact]
    public async Task FileCleanupJob_ShouldDeleteStaleAndOrphanFiles_Properly()
    {
        var fileStorage = _serviceProvider.GetRequiredService<IFileStorageProvider>();
        var cleanupJob = _serviceProvider.GetRequiredService<IFileCleanupJob>();
        var docRepo = _serviceProvider.GetRequiredService<IDocumentRepository>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        var userId = Guid.NewGuid();
        var staleFileName = "stale.txt";

        var staleFilePath = await fileStorage.SaveFileAsync(userId, staleFileName, new MemoryStream(Encoding.UTF8.GetBytes("stale content")));

        var staleDoc = new DocumentEntity(userId, staleFilePath, staleFileName)
        {
            CreationDateTime = DateTime.Now.AddDays(-2)
        };
        staleDoc.MarkFailed();//marked as stailed document
        await docRepo.AddAsync(staleDoc);
        await unitOfWork.SaveChangesAsync();

        var orphanFilePath = await fileStorage.SaveFileAsync(userId, $"orphan-{userId}.txt", new MemoryStream(Encoding.UTF8.GetBytes("orphan content")));

        await cleanupJob.CleanupAsync();

        var docInDb = await docRepo.GetByIdAsync(staleDoc.Id);
        Assert.True(docInDb.IsDeleted);
        Assert.False(File.Exists(staleFilePath));
        Assert.False(File.Exists(orphanFilePath));
    }
}
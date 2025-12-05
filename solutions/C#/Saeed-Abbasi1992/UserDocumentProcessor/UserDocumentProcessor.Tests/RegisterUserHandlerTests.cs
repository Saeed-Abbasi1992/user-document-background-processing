using Microsoft.Extensions.Options;
using Moq;
using System.Text;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Application.Users.Commands;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;
using UserDocumentProcessor.Infrastructure;

namespace UserDocumentProcessor.Tests
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IDocumentRepository> _docRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IBackgroundJobService> _backgroundJobs = new();
        private readonly IFileStorageProvider _fileStorage;

        private readonly string _tempFolder;

        public RegisterUserHandlerTests()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);

            var settings = Options.Create(new FileStorageSettings
            {
                BasePath = _tempFolder
            });

            _fileStorage = new UserDocumentProcessor.Infrastructure.Services.FileStorageProvider(settings);
        }

        [Fact]
        public async Task Handle_ShouldRegisterUser_AndSaveFileInConfiguredPath()
        {
            var handler = new RegisterUserHandler(
                _userRepo.Object,
                _docRepo.Object,
                _unitOfWork.Object,
                _fileStorage,
                _backgroundJobs.Object);

            var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("dummy file content"));
            var fileName = "doc.txt";
            var formFile = new Microsoft.AspNetCore.Http.FormFile(fileContent, 0, fileContent.Length, "Document", fileName);

            var command = new RegisterUserCommand
            {
                Name = "Test User",
                Email = "test@example.com",
                Document = formFile
            };

            var result = await handler.Handle(command, CancellationToken.None);

            _userRepo.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Once);
            _backgroundJobs.Verify(b => b.EnqueueWelcomeMessage(It.IsAny<UserEntity>()), Times.Once);
            _backgroundJobs.Verify(b => b.ScheduleDocumentProcessing(It.IsAny<Guid>(), TimeSpan.FromSeconds(30)), Times.Once);

            Assert.Equal("Registered", result.Status);

            var expectedPath = Path.Combine(_tempFolder, result.UserId.ToString(), fileName);
            Assert.True(File.Exists(expectedPath));

            if (File.Exists(expectedPath)) File.Delete(expectedPath);
            if (Directory.Exists(_tempFolder)) Directory.Delete(_tempFolder, true);
        }
    }
}

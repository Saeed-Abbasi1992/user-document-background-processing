using Moq;
using System.Text;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Application.Users.Commands;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;

namespace UserDocumentProcessor.Tests
{
    public class RegisterUserHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IDocumentRepository> _docRepo = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IFileStorageProvider> _fileStorage = new();
        private readonly Mock<IBackgroundJobService> _backgroundJobs = new();

        [Fact]
        public async Task Handle_ShouldRegisterUser_AndEnqueueJobs()
        {
            var handler = new RegisterUserHandler(
                _userRepo.Object,
                _docRepo.Object,
                _unitOfWork.Object,
                _fileStorage.Object,
                _backgroundJobs.Object);

            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("dummy file"));
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.FileName).Returns("doc.txt");

            var command = new RegisterUserCommand
            {
                Name = "Test User",
                Email = "test@example.com",
                Document = fileMock.Object
            };

            _fileStorage.Setup(f => f.SaveFileAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Stream>()))
                        .ReturnsAsync("path/to/file");

            var result = await handler.Handle(command, CancellationToken.None);

            _userRepo.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Once);
            _backgroundJobs.Verify(b => b.EnqueueWelcomeMessage(It.IsAny<UserEntity>()), Times.Once);
            _backgroundJobs.Verify(b => b.ScheduleDocumentProcessing(It.IsAny<Guid>(), TimeSpan.FromSeconds(30)), Times.Once);
            Assert.Equal("Registered", result.Status);
        }
    }
}

using MediatR;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;

namespace UserDocumentProcessor.Application.Users.Commands
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly IBackgroundJobService _backgroundJobs;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IDocumentRepository documentRepository,
            IUnitOfWork unitOfWork,
            IFileStorageProvider fileStorageProvider,
            IBackgroundJobService backgroundJobs)
        {
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _fileStorageProvider = fileStorageProvider;
            _backgroundJobs = backgroundJobs;
        }

        public async Task<RegisterUserResultDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new UserEntity(request.Name, request.Email);

            await _userRepository.AddAsync(user);

            var filePath = await _fileStorageProvider.SaveFileAsync(user.Id, request.Document.FileName, request.Document.OpenReadStream());

            var document = new DocumentEntity(user.Id, filePath, request.Document.FileName);

            user.AddDocument(document);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _backgroundJobs.EnqueueWelcomeMessage(user);

            _backgroundJobs.ScheduleDocumentProcessing(document.Id, TimeSpan.FromSeconds(30));

            return new RegisterUserResultDto
            {
                UserId = user.Id,
                Status = "Registered",
                Message = "User registered successfully."
            };
        }
    }
}

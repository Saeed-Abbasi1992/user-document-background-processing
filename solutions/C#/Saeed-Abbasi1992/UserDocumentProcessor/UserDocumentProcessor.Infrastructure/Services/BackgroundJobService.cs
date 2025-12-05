using Hangfire;
using Microsoft.Extensions.Logging;
using UserDocumentProcessor.Application.Interfaces;
using UserDocumentProcessor.Domain;
using UserDocumentProcessor.Domain.Entities;
using UserDocumentProcessor.Domain.Repositories;
using UserDocumentProcessor.Infrastructure.Helper;

namespace UserDocumentProcessor.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<INotificationService> _notificationServices;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly IBackgroundJobClient _jobClient;

        public BackgroundJobService(
            IUserRepository userRepository,
            IDocumentRepository documentRepository,
            IUnitOfWork unitOfWork,
            IEnumerable<INotificationService> notificationServices,
            ILogger<BackgroundJobService> logger,
            IBackgroundJobClient jobClient)
        {
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _notificationServices = notificationServices;
            _logger = logger;
            _jobClient = jobClient;
        }

        public void EnqueueWelcomeMessage(UserEntity user)
        {
            _jobClient.Enqueue(() => SendWelcomeMessage(user));
        }

        public void ScheduleDocumentProcessing(Guid documentId, TimeSpan delay)
        {
            _jobClient.Schedule(() => ProcessDocument(documentId), delay);
        }

        public void EnqueueCompletionMessage(UserEntity user, Guid documentId)
        {
            _jobClient.Enqueue(() => SendCompletionMessage(user, documentId));
        }


        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 300, 600 })] // Retry #1=5min, #2=10min
        public async Task SendWelcomeMessage(UserEntity user)
        {
            _logger.LogInformation("Sending Welcome message. UserId: {UserId}, UserName: {UserName}", user.Id, user.Name);

            foreach (var notificationService in _notificationServices)
            {
                var receiver = NotificationHelper.GetReceiver(user, notificationService.ChannelType);

                if (!string.IsNullOrEmpty(receiver))
                    await notificationService.NotifyAsync(new Application.DTOs.NotificationItem(receiver, "Welcome !"));
            }
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 300, 600 })]
        public async Task ProcessDocument(Guid documentId)
        {
            _logger.LogInformation("[Job] Starting document processing. DocumentId={DocumentId}", documentId);

            var document = await _documentRepository.GetByIdAsync(documentId);

            if (document == null)
            {
                _logger.LogWarning("Document not found. DocumentId: {DocumentId}", documentId);
                return;
            }

            var user = await _userRepository.GetByIdAsync(document.UserId);

            var pdfPath = Path.ChangeExtension(document.Path, ".pdf");

            try
            {
                File.Copy(document.Path, pdfPath, true);
                document.MarkProcessed();
                document.ProcessResultPath = pdfPath;

                await _documentRepository.UpdateAsync(document);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Document processed to PDF. DocumentId: {DocumentId}, UserId: {UserId}, Path: {Path}", document.Id, document.UserId, pdfPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Job] Failed to process document DocumentId:{DocumentId}", documentId);
                throw; // Hangfire will retry automatically
            }

            if (user != null)
                EnqueueCompletionMessage(user, document.Id);
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 300, 600 })]
        public async Task SendCompletionMessage(UserEntity user, Guid documentId)
        {
            _logger.LogInformation("Sending completion message. UserId: {UserId}, DocumentId: {DocumentId}", user.Id, documentId);

            foreach (var notificationService in _notificationServices)
            {
                var receiver = NotificationHelper.GetReceiver(user, notificationService.ChannelType);

                if (!string.IsNullOrEmpty(receiver))
                    await notificationService.NotifyAsync(new Application.DTOs.NotificationItem(receiver, "Your document has been processed and is now available."));
            }
        }
    }
}

using UserDocumentProcessor.Application.Enums;
using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Application.Interfaces
{
    public interface IBackgroundJobService
    {
        void EnqueueWelcomeMessage(UserEntity user);

        void ScheduleDocumentProcessing(Guid documentId, TimeSpan delay);

        void EnqueueCompletionMessage(UserEntity user, Guid documentId);
    }
}

using UserDocumentProcessor.Application.DTOs;
using UserDocumentProcessor.Application.Enums;

namespace UserDocumentProcessor.Application.Interfaces
{
    public interface INotificationService
    {
        NotificationChannelType ChannelType { get; }
        Task NotifyAsync(NotificationItem item);
    }
}

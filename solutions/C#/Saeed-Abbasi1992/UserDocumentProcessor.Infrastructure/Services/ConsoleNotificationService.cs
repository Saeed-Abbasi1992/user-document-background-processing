using UserDocumentProcessor.Application.DTOs;
using UserDocumentProcessor.Application.Enums;
using UserDocumentProcessor.Application.Interfaces;

namespace UserDocumentProcessor.Infrastructure.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        public NotificationChannelType ChannelType => NotificationChannelType.Console;

        public Task NotifyAsync(NotificationItem item)
        {
            Console.WriteLine($"[CONSOLE NOTIFIER] To: {item.Receiver} | {item.Content}");
            return Task.CompletedTask;
        }
    }
}
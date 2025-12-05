using UserDocumentProcessor.Application.Enums;

namespace UserDocumentProcessor.Application.DTOs;

public class NotificationItem
{
    public string Receiver { get; set; } = default!;
    public string Content { get; set; } = default!;
    public NotificationItem(string receiver, string content)
    {
        Receiver = receiver;
        Content = content;
    }
}
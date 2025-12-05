using UserDocumentProcessor.Application.Enums;

namespace UserDocumentProcessor.API.Options;

public class NotificationSettings
{
    public List<NotificationChannelType> EnabledChannels { get; set; }
}

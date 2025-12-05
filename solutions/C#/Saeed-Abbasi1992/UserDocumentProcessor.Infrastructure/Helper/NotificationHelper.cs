using UserDocumentProcessor.Application.Enums;
using UserDocumentProcessor.Domain.Entities;

namespace UserDocumentProcessor.Infrastructure.Helper
{
    public class NotificationHelper
    {
        public static string GetReceiver(UserEntity user, NotificationChannelType notificationChannelType)
        {
            switch (notificationChannelType)
            {
                case NotificationChannelType.Console:
                    return user.Name;

                case NotificationChannelType.Email:
                    return user.Email;

                default: return "";
            }
        }
    }
}

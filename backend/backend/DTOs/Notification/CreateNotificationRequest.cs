using backend.Models;

namespace backend.DTOs.Notification
{
    public class CreateNotificationRequest
    {
        public int UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public int? RelatedEntityId { get; set; }
    }
}

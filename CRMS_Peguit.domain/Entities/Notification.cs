using System;

namespace CRMS_Peguit.domain.entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int TenantId { get; set; }
        public int RecipientUserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public virtual User RecipientUser { get; set; } = null!;
    }
}

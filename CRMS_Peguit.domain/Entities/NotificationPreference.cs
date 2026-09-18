using System;

namespace CRMS_Peguit.domain.entities
{
    public class NotificationPreference
    {
        public int NotificationPreferenceId { get; set; }
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public bool IsEnabled { get; set; } = true;

        public virtual User User { get; set; } = null!;
    }
}

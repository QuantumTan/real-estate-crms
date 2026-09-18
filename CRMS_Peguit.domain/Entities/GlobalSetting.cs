using System;

namespace CRMS_Peguit.domain.entities
{
    public class GlobalSetting
    {
        public int GlobalSettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int UpdatedBySuperAdminId { get; set; }
        public SuperAdmin? UpdatedBySuperAdmin { get; set; }
    }
}

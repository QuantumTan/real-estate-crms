using System;

namespace CRMS_Peguit.domain.entities
{
    public class Campaign
    {
        public int CampaignId { get; set; }
        public int TenantId { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public string? Channel { get; set; }
        public string Status { get; set; } = "Active";
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

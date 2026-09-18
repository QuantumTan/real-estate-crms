using System;
using System.Text.Json.Serialization;

namespace CRMS_Peguit.domain.entities
{
    public class DealClause
    {
        public int DealClauseId { get; set; }
        public int DealId { get; set; }
        public string ClauseId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ClauseText { get; set; }
        public bool IsApproved { get; set; } = true;
        public DateTime? ApprovedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public virtual Deal? Deal { get; set; }
    }
}

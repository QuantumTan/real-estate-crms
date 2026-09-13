using System;

namespace CRMS_Peguit.winforms.Models.ViewModels
{
    public class PendingApprovalItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SubmitterName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AssignedTo { get; set; } = "Unassigned";
        public int? AssignedAgentId { get; set; }
        public string Status { get; set; } = "PENDING REVIEW";
        public object OriginalEntity { get; set; } = null!;
    }
}

using System;

namespace CRMS_Peguit.domain.entities
{
    public enum CallOutcome
    {
        Connected,
        LeftVoicemail,
        NoAnswer,
        Busy,
        WrongNumber
    }

    public class Activity
    {
        public int ActivityId { get; set; }

        public string Type { get; set; } = string.Empty;
        public int? RelatedLeadId { get; set; }
        public int? RelatedCustomerId { get; set; }
        public int LoggedByAgentId { get; set; }
        public string? Notes { get; set; }
        public DateTime ActivityDate { get; set; }

        // Extended fields for unified timeline & call logging
        public CallOutcome? Outcome { get; set; }
        public int? DurationMinutes { get; set; }

        public virtual User LoggedByAgent { get; set; } = null!;
        public virtual Lead? RelatedLead { get; set; }
        public virtual Customer? RelatedCustomer { get; set; }

        public void LinkToCustomer(int customerId)
        {
            RelatedCustomerId = customerId;
            RelatedLeadId = null;
        }

        public void LinkToLead(int leadId)
        {
            RelatedLeadId = leadId;
            RelatedCustomerId = null;
        }
    }
}

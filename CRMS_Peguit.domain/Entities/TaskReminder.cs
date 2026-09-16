using System;

namespace CRMS_Peguit.domain.entities
{
    public class TaskReminder
    {
        public int TaskReminderId { get; set; }

        public int Id
        {
            get => TaskReminderId;
            set => TaskReminderId = value;
        }

        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int AssignedToUserId { get; set; }
        public int? RelatedCustomerId { get; set; }
        public int? RelatedLeadId { get; set; }

        public string Status { get; set; } = "Pending";
        public string Type { get; set; } = "Call";
        public string? Notes { get; set; }
        public string Priority { get; set; } = "Medium";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual User AssignedToUser { get; set; } = null!;
        public virtual Customer? RelatedCustomer { get; set; }
        public virtual Lead? RelatedLead { get; set; }

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

        public void Reschedule(DateTime newDueDate)
        {
            DueDate = newDueDate;
            Status = "Pending";
            UpdatedAt = DateTime.UtcNow;
        }

        public void Snooze(TimeSpan interval)
        {
            DueDate = DueDate.Add(interval);
            Status = "Pending";
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkComplete()
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

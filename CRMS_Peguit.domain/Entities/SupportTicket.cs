using System;
using System.Collections.Generic;

namespace CRMS_Peguit.domain.entities
{
    public class SupportTicket
    {
        public int TicketId { get; set; }

        public string TicketNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int RaisedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FirstRespondedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual User RaisedByUser { get; set; } = null!;
        public virtual User? AssignedToUser { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
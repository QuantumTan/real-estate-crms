using System;

namespace CRMS_Peguit.domain.entities
{
    public class TicketComment
    {
        public int TicketCommentId { get; set; }

        public int TicketId { get; set; }
        public int AuthorUserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string CommentType { get; set; } = "Comment"; // "Comment", "StatusChange", "Assignment", "Reopened"
        public bool IsInternal { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public virtual SupportTicket Ticket { get; set; } = null!;
        public virtual User AuthorUser { get; set; } = null!;
    }
}

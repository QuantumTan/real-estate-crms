using System;

namespace CRMS_Peguit.domain.entities
{
    public class PropertyShowingDetail
    {
        public int ShowingDetailId { get; set; }

        public int ActivityId { get; set; }
        public int PropertyId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? FeedbackNotes { get; set; }

        public virtual Activity Activity { get; set; } = null!;
        public virtual Property Property { get; set; } = null!;
    }
}
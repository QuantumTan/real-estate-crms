using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS_Peguit.domain.entities
{
    public class Lead
    {
        public int LeadId { get; set; }
        public int TenantId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Join(" ",
                    new[]
                    {
                        FirstName,
                        MiddleName,
                        LastName,
                        Suffix
                    }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
            }
        }

        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Source { get; set; }

        public string? Notes { get; set; }

        public string Stage { get; set; } = "new";

        public string? Priority { get; set; }
        public decimal? ExpectedValue { get; set; }

        public int? CreatedByUserId { get; set; }
        public int? AssignedAgentId { get; set; }
        public string AssignmentStatus { get; set; } = "pending_review";
        public int? AssignmentReviewedByUserId { get; set; }
        public DateTime? AssignmentReviewedAt { get; set; }
        public string? AssignmentReviewNotes { get; set; }

        public int? ConvertedCustomerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

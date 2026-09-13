using System;
using System.Linq;

namespace CRMS_Peguit.domain.entities
{
    public class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string FullName =>
            string.Join(" ", new[] { FirstName, MiddleName, LastName, Suffix }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}

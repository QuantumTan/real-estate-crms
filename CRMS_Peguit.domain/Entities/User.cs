using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRMS_Peguit.domain.entities
{
    public class User
    {
        public int UserId { get; set; }
        public int TenantId { get; set; } //added
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string? Suffix { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
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
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

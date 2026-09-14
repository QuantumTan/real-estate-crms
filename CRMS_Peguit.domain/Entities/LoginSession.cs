using System;

namespace CRMS_Peguit.domain.entities
{
    public class LoginSession
    {
        public int SessionId { get; set; }

        public int UserId { get; set; }
        public DateTime LoginAt { get; set; }
        public DateTime? LogoutAt { get; set; }
        public string? IpAddress { get; set; }

        public virtual User User { get; set; } = null!;
    }
}

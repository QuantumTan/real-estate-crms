using System;

namespace CRMS_Peguit.domain.entities
{
    public class BuyerProfile
    {
        public int CustomerId { get; set; }

        public decimal Budget { get; set; }
        public string? PreferredLocation { get; set; }
        public string? PreferredPropertyType { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
}

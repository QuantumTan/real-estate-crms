using System;
using System.Collections.Generic;
using System.Text;

namespace CRMS_Peguit.domain.entities
{
    public class Deal
    {
        public int DealId { get; set; }

        public int TenantId { get; set; }
        public int CustomerId { get; set; }
        public int PropertyId { get; set; }
        public int? AgentId { get; set; }
        public decimal Value { get; set; }
        public decimal CommissionRate { get; set; }
        public string Stage { get; set; } = string.Empty;
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- Structured Commercial Terms & Conditions ---
        public string? PaymentScheme { get; set; } = "Bank Financing";
        public decimal? ReservationFee { get; set; }
        public decimal? DownPaymentPercent { get; set; } = 20m;
        public decimal? DownPaymentAmount { get; set; }
        public decimal? BalanceAmount { get; set; }

        // --- Statutory Tax & Closing Cost Allocation ---
        public string? CgtPayer { get; set; } = "Seller";
        public string? DstPayer { get; set; } = "Buyer";
        public string? TransferTaxPayer { get; set; } = "Buyer";
        public string? RegistrationFeePayer { get; set; } = "Buyer";

        // --- Closing Contingencies & Approved Clauses ---
        public string? ContingenciesJson { get; set; }
        public string? ApprovedClauseIds { get; set; }
        public string? SpecialStipulations { get; set; }
        public DateTime? ContractSignedDate { get; set; }
    }
}
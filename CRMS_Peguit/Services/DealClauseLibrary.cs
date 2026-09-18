using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRMS_Peguit.domain.entities;

namespace CRMS_Peguit.winforms.Models.Services
{
    public class BrokerageClause
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ClauseText { get; set; } = string.Empty;
        public bool IsDefaultSelected { get; set; }
    }

    /// <summary>
    /// Repository of legally vetted standard real estate contract clauses
    /// and closing contingencies conforming to Philippine and international brokerage practice.
    /// </summary>
    public static class DealClauseLibrary
    {
        private static readonly List<BrokerageClause> Clauses = new()
        {
            new BrokerageClause
            {
                Id = "TTL-01",
                Title = "Clean Title Guarantee & Non-Encumbrance Warranty",
                Category = "Title & Due Diligence",
                ClauseText = "The Seller warrants that the Property is free and clear of all adverse claims, mortgages, tax liens, encumbrances, lis pendens, or tenancy claims of any kind. The Seller shall present the certified true Owner's Duplicate Certificate of Title and updated Tax Declaration prior to execution of the final deed.",
                IsDefaultSelected = true
            },
            new BrokerageClause
            {
                Id = "TAX-01",
                Title = "Statutory Tax and Transfer Cost Apportionment",
                Category = "Taxes & Closing Fees",
                ClauseText = "The Seller shall be solely responsible for the payment of the Capital Gains Tax (6% of gross selling price or zonal/fair market value, whichever is higher) and professional brokerage fees. The Buyer shall shoulder the Documentary Stamp Tax (1.5%), Local Transfer Tax, Registry of Deeds Registration Fees, and Notarial Fees.",
                IsDefaultSelected = true
            },
            new BrokerageClause
            {
                Id = "FIN-01",
                Title = "Mortgage Financing & Bank Letter of Guarantee (LOG) Contingency",
                Category = "Financing",
                ClauseText = "This agreement is conditional upon the Buyer securing an official Bank Letter of Guarantee (LOG) or mortgage loan approval for the balance amount within thirty (30) calendar days from the date of the Reservation Agreement. If financing is denied after diligent good-faith application, all payments less documented administrative processing expenses shall be returned to the Buyer.",
                IsDefaultSelected = true
            },
            new BrokerageClause
            {
                Id = "TRN-01",
                Title = "Turnover, Punchlist, and Possession Protocol",
                Category = "Turnover & Condition",
                ClauseText = "The Property shall be handed over to the Buyer in an as-is, where-is condition, subject to the completion of all mutually agreed rectification items on the punchlist. Physical keys and occupancy shall only be released upon receipt of full balance clearance or the Bank Letter of Guarantee.",
                IsDefaultSelected = true
            },
            new BrokerageClause
            {
                Id = "DEF-01",
                Title = "Default, Grace Period, and Earnest Money Forfeiture",
                Category = "Default & Cancellation",
                ClauseText = "Should the Buyer fail to remit scheduled installment or downpayment amounts within the mandatory thirty (30) day grace period following formal notice, the reservation may be cancelled, with the earnest money forfeited as liquidated damages. Should the Seller default, the Buyer is entitled to an immediate refund in full plus statutory interest.",
                IsDefaultSelected = true
            },
            new BrokerageClause
            {
                Id = "ALT-01",
                Title = "Contingent on Sale of Buyer's Existing Property",
                Category = "Contingencies",
                ClauseText = "This sale is contingent upon the successful closing of the Buyer's current property located at the declared address within forty-five (45) calendar days from this agreement. Seller retains the right to market the property subject to a 72-hour right of first refusal.",
                IsDefaultSelected = false
            }
        };

        public static IReadOnlyList<BrokerageClause> GetStandardClauses() => Clauses;

        /// <summary>
        /// Generates standard closing contingencies for a deal based on payment scheme.
        /// </summary>
        public static List<DealContingency> GetDefaultContingencies(string? paymentScheme)
        {
            var contingencies = new List<DealContingency>
            {
                new DealContingency
                {
                    Name = "Clean Title & Encumbrance Verification",
                    Status = "Satisfied",
                    DueDate = DateTime.UtcNow.AddDays(15),
                    Notes = "Title search with Registry of Deeds"
                }
            };

            bool isBankFinancing = string.Equals(paymentScheme, "Bank Financing", StringComparison.OrdinalIgnoreCase);
            if (isBankFinancing)
            {
                contingencies.Add(new DealContingency
                {
                    Name = "Bank Loan Approval / Letter of Guarantee (LOG)",
                    Status = "Pending",
                    DueDate = DateTime.UtcNow.AddDays(30),
                    Notes = "Formal bank credit approval required"
                });
            }

            contingencies.Add(new DealContingency
            {
                Name = "Property Physical Inspection & Turnover Acceptance",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddDays(25),
                Notes = "Joint walkthrough inspection and punchlist verification"
            });

            contingencies.Add(new DealContingency
            {
                Name = "BIR Tax Clearance / Certificate Authorizing Registration (CAR)",
                Status = "Pending",
                DueDate = DateTime.UtcNow.AddDays(45),
                Notes = "Required for registry transfer of title"
            });

            return contingencies;
        }

        /// <summary>
        /// Formats a complete, professional Real Estate Reservation Agreement / Term Sheet document.
        /// </summary>
        public static string FormatTermSheetText(
            Deal deal,
            string customerName,
            string propertyAddress,
            string agentName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                      NEXA CRM - REAL ESTATE TERM SHEET                        ");
            sb.AppendLine("                          RESERVATION & SALES AGREEMENT                         ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Document Date: {DateTime.Now:MMMM d, yyyy}              Reference ID: DEAL-{deal.DealId:D5}");
            sb.AppendLine();
            sb.AppendLine("--- 1. TRANSACTION PARTIES & PROPERTY ---");
            sb.AppendLine($"Buyer Name        : {customerName}");
            sb.AppendLine($"Property Subject  : {propertyAddress}");
            sb.AppendLine($"Assigned Agent    : {agentName}");
            sb.AppendLine($"Current Stage     : {deal.Stage.ToUpperInvariant()}");
            sb.AppendLine($"Target Close Date : {(deal.ExpectedCloseDate.HasValue ? deal.ExpectedCloseDate.Value.ToString("MMMM d, yyyy") : "To be scheduled")}");
            sb.AppendLine();
            sb.AppendLine("--- 2. COMMERCIAL FINANCIAL TERMS ---");
            sb.AppendLine($"Total Purchase Price: ₱{deal.Value:N2}");
            sb.AppendLine($"Payment Scheme      : {deal.PaymentScheme ?? "Spot Cash"}");
            sb.AppendLine($"Reservation Deposit : ₱{(deal.ReservationFee.HasValue ? deal.ReservationFee.Value.ToString("N2") : "0.00")}");
            sb.AppendLine($"Downpayment ({deal.DownPaymentPercent ?? 20:N1}%): ₱{deal.DownPaymentAmount:N2}");
            sb.AppendLine($"Balance Payable     : ₱{deal.BalanceAmount:N2}");
            sb.AppendLine($"Commission Rate     : {deal.CommissionRate:P1}");
            sb.AppendLine();
            sb.AppendLine("--- 3. STATUTORY TAX & CLOSING EXPENSE ALLOCATION ---");
            sb.AppendLine($"• Capital Gains Tax (6%)              : Shouldered by {deal.CgtPayer}");
            sb.AppendLine($"• Documentary Stamp Tax (1.5%)       : Shouldered by {deal.DstPayer}");
            sb.AppendLine($"• Local Transfer Tax                 : Shouldered by {deal.TransferTaxPayer}");
            sb.AppendLine($"• Title Registration & Notarial Fees : Shouldered by {deal.RegistrationFeePayer}");
            sb.AppendLine();
            // --- 4. CONDITIONS PRECEDENT & CONTINGENCIES ---
            var contingencies = (deal.Contingencies != null && deal.Contingencies.Count > 0)
                ? deal.Contingencies.OrderBy(c => c.DealContingencyId).ToList()
                : DealContingency.DeserializeList(deal.ContingenciesJson);
            if (contingencies.Count == 0)
            {
                sb.AppendLine("Standard due diligence conditions apply.");
            }
            else
            {
                foreach (var c in contingencies)
                {
                    string dueStr = c.DueDate.HasValue ? $"[Due: {c.DueDate.Value:MMM d, yyyy}]" : "";
                    sb.AppendLine($"[{c.Status.ToUpperInvariant()}] {c.Name} {dueStr}");
                    if (!string.IsNullOrWhiteSpace(c.Notes)) sb.AppendLine($"       Notes: {c.Notes}");
                }
            }
            sb.AppendLine();
            sb.AppendLine("--- 5. AGREED CONTRACT CLAUSES & STIPULATIONS ---");
            var clauseIds = (deal.ApprovedClauseIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var selectedClauses = Clauses.Where(c => clauseIds.Contains(c.Id)).ToList();
            if (selectedClauses.Count == 0) selectedClauses = Clauses.Where(c => c.IsDefaultSelected).ToList();

            foreach (var clause in selectedClauses)
            {
                sb.AppendLine($"[{clause.Id}] {clause.Title.ToUpperInvariant()}");
                sb.AppendLine($"   {clause.ClauseText}");
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(deal.SpecialStipulations))
            {
                sb.AppendLine("[ADDENDUM] SPECIAL STIPULATIONS & RIDERS");
                sb.AppendLine($"   {deal.SpecialStipulations}");
                sb.AppendLine();
            }

            sb.AppendLine("================================================================================");
            sb.AppendLine("ACKNOWLEDGEMENT & SIGN-OFF:");
            sb.AppendLine();
            sb.AppendLine("Buyer Signature: _______________________      Date: _______________");
            sb.AppendLine();
            sb.AppendLine("Seller Signature: ______________________      Date: _______________");
            sb.AppendLine();
            sb.AppendLine("Broker / Agent : _______________________      Date: _______________");
            sb.AppendLine("================================================================================");

            return sb.ToString();
        }
    }
}

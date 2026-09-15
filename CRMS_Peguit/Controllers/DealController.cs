using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRMS_Peguit.winforms.Controllers
{
    public class DealController : IDisposable
    {
        private readonly RealEstateDbContext _db;
        public int TenantId => CurrentSession.TenantId;

        public DealController()
        {
            _db = LocalDb.CreateContext(tenantId: TenantId);
        }

        public List<Deal> GetAll()
        {
            try
            {
                var query = _db.Deals
                    .Include(d => d.Contingencies)
                    .Include(d => d.DealClauses)
                    .AsNoTracking();

                if (!RbacService.HasFullOversight && RbacService.IsAgent)
                {
                    int currentUserId = CurrentSession.UserId;
                    query = query.Where(d => d.AgentId == currentUserId);
                }

                return query
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DealController.GetAll] Error: {ex.Message}");
                return new List<Deal>();
            }
        }

        public Deal? GetById(int id)
        {
            var item = _db.Deals
                .Include(d => d.Contingencies)
                .Include(d => d.DealClauses)
                .AsNoTracking()
                .SingleOrDefault(x => x.DealId == id);

            if (item is null) return null;

            if (!RbacService.HasFullOversight && RbacService.IsAgent && item.AgentId != CurrentSession.UserId)
                return null;

            return item;
        }

        public Deal Add(Deal deal)
        {
            deal.CreatedByUserId = CurrentSession.UserId > 0 ? CurrentSession.UserId : 1;
            deal.CreatedAt = DateTime.UtcNow;

            // Apply defaults for commercial terms if not specified
            if (string.IsNullOrWhiteSpace(deal.PaymentScheme))
                deal.PaymentScheme = "Bank Financing";

            if (deal.Contingencies.Count == 0)
            {
                var defaults = DealClauseLibrary.GetDefaultContingencies(deal.PaymentScheme);
                foreach (var dc in defaults)
                {
                    deal.Contingencies.Add(new DealContingency
                    {
                        ContingencyName = dc.ContingencyName,
                        Description = dc.Description,
                        DueDate = dc.DueDate,
                        IsSatisfied = dc.IsSatisfied,
                        SatisfiedAt = dc.SatisfiedAt,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (deal.DealClauses.Count == 0)
            {
                var defaultClauseIds = new[] { "TTL-01", "TAX-01", "FIN-01", "TRN-01", "DEF-01" };
                foreach (var cid in defaultClauseIds)
                {
                    var clauseDef = DealClauseLibrary.GetStandardClauses().FirstOrDefault(c => c.Id == cid);
                    deal.DealClauses.Add(new DealClause
                    {
                        ClauseId = cid,
                        Title = clauseDef?.Title ?? cid,
                        ClauseText = clauseDef?.ClauseText,
                        IsApproved = true,
                        ApprovedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _db.Deals.Add(deal);
            _db.SaveChanges();
            return deal;
        }

        public void Update(Deal deal)
        {
            var item = _db.Deals.Include(d => d.Contingencies).Include(d => d.DealClauses).SingleOrDefault(x => x.DealId == deal.DealId);
            if (item is null) return;

            item.CustomerId = deal.CustomerId;
            item.PropertyId = deal.PropertyId;
            item.AgentId = deal.AgentId;
            item.Value = deal.Value;
            item.CommissionRate = deal.CommissionRate;
            item.Stage = deal.Stage;
            item.ExpectedCloseDate = deal.ExpectedCloseDate;

            // Structured Terms and Conditions updates
            item.PaymentScheme = deal.PaymentScheme;
            item.ReservationFee = deal.ReservationFee;
            item.DownPaymentPercent = deal.DownPaymentPercent;
            item.CgtPayer = deal.CgtPayer;
            item.DstPayer = deal.DstPayer;
            item.TransferTaxPayer = deal.TransferTaxPayer;
            item.RegistrationFeePayer = deal.RegistrationFeePayer;
            item.SpecialStipulations = deal.SpecialStipulations;
            item.ContractSignedDate = deal.ContractSignedDate;

            _db.SaveChanges();
        }

        public void UpdateContingencyStatus(int dealId, int contingencyIndex, string newStatus, string? notes = null)
        {
            var item = _db.Deals.Include(d => d.Contingencies).SingleOrDefault(x => x.DealId == dealId);
            if (item is null) return;

            var contingencies = item.Contingencies.OrderBy(c => c.DealContingencyId).ToList();
            if (contingencyIndex >= 0 && contingencyIndex < contingencies.Count)
            {
                var c = contingencies[contingencyIndex];
                c.Status = newStatus;
                if (string.Equals(newStatus, "Satisfied", StringComparison.OrdinalIgnoreCase))
                {
                    c.SatisfiedAt = DateTime.UtcNow;
                    c.IsSatisfied = true;
                }
                else
                {
                    c.SatisfiedAt = null;
                    c.IsSatisfied = false;
                }
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    c.Description = notes;
                }
                _db.SaveChanges();
            }
        }

        public void Delete(Deal deal)
        {
            var item = _db.Deals.SingleOrDefault(x => x.DealId == deal.DealId);
            if (item is null) return;

            _db.Deals.Remove(item);
            _db.SaveChanges();
        }

        public Dictionary<int, string> GetCustomerNames()
        {
            return _db.Customers
                .Include(x => x.Person)
                .AsNoTracking()
                .OrderBy(x => x.Person.LastName)
                .ThenBy(x => x.Person.FirstName)
                .ToDictionary(
                    x => x.CustomerId,
                    x => x.FullName
                );
        }

        public Dictionary<int, string> GetPropertyAddresses()
        {
            return _db.Properties
                .AsNoTracking()
                .OrderBy(x => x.Address)
                .ToDictionary(
                    x => x.PropertyId,
                    x => x.Address
                );
        }

        public Dictionary<int, string> GetAgentNames()
        {
            return _db.Users
                .Include(x => x.Person)
                .AsNoTracking()
                .OrderBy(x => x.Person.FirstName)
                .ThenBy(x => x.Person.LastName)
                .ToDictionary(
                    x => x.UserId,
                    x => x.FullName
                );
        }

        public List<KeyValuePair<int, string>> GetCustomerPickerList(int? includeCustomerId = null)
        {
            var buyerTypes = new[] { "buyer", "both" };

            return _db.Customers
                .Include(c => c.Person)
                .AsNoTracking()
                .Where(c => (buyerTypes.Contains(c.Type.ToLower()) && c.Status.ToLower() != "inactive") ||
                            (includeCustomerId.HasValue && c.CustomerId == includeCustomerId.Value))
                .OrderBy(c => c.Person.LastName)
                .ThenBy(c => c.Person.FirstName)
                .ToList()
                .Select(c => new KeyValuePair<int, string>(c.CustomerId, c.FullName))
                .ToList();
        }

        public List<KeyValuePair<int, string>> GetPropertyPickerList()
        {
            return _db.Properties
                .AsNoTracking()
                .OrderBy(p => p.Address)
                .Select(p => new KeyValuePair<int, string>(p.PropertyId, $"{p.Address} (₱{p.Price:N2})"))
                .ToList();
        }

        public List<KeyValuePair<int, string>> GetAgentPickerList()
        {
            return _db.Users
                .Include(u => u.Person)
                .AsNoTracking()
                .Where(u => u.Status != "inactive")
                .OrderBy(u => u.Person.LastName)
                .ToList()
                .Select(u => new KeyValuePair<int, string>(u.UserId, u.FullName))
                .ToList();
        }

        public static DealFinancingResult CalculateFinancing(decimal dealValue, decimal downPaymentPercent, string? paymentScheme)
        {
            if (string.Equals(paymentScheme, "Spot Cash", StringComparison.OrdinalIgnoreCase))
            {
                return new DealFinancingResult
                {
                    DownPaymentAmount = dealValue,
                    BalanceAmount = 0,
                    DownPaymentDisplay = $"Full Payment: ₱{dealValue:N2}",
                    BalanceDisplay = "Balance: ₱0.00 (Cash Settlement)"
                };
            }

            decimal downAmt = dealValue * (downPaymentPercent / 100m);
            decimal balAmt = Math.Max(0, dealValue - downAmt);
            return new DealFinancingResult
            {
                DownPaymentAmount = downAmt,
                BalanceAmount = balAmt,
                DownPaymentDisplay = $"Downpayment ({downPaymentPercent:0.##}%): ₱{downAmt:N2}",
                BalanceDisplay = $"Balance to Finance: ₱{balAmt:N2}"
            };
        }

        public static bool ValidateDealInput(object? customerValue, object? propertyValue, string dealValueText, out decimal dealValue, out string? errorMessage)
        {
            dealValue = 0;
            if (customerValue == null)
            {
                errorMessage = "Please select a Buyer / Customer.";
                return false;
            }

            if (propertyValue == null)
            {
                errorMessage = "Please select a Subject Property.";
                return false;
            }

            if (!decimal.TryParse(dealValueText.Replace(",", "").Trim(), out dealValue) || dealValue <= 0)
            {
                errorMessage = "Please enter a valid positive Deal Value.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }

    public class DealFinancingResult
    {
        public decimal DownPaymentAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public string DownPaymentDisplay { get; set; } = string.Empty;
        public string BalanceDisplay { get; set; } = string.Empty;
    }
}

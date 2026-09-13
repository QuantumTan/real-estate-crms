using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using CRMS_Peguit.winforms.Models.Services;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.winforms.Controllers
{
    public class DealController : IDisposable
    {
        private readonly RealEstateDbContext _db;

        private int TenantId => CurrentSession.TenantId;

        public DealController()
        {
            _db = LocalDb.CreateContext(TenantId);
        }

        public List<Deal> GetAll()
        {
            var query = _db.Deals.AsNoTracking();

            if (!RbacService.HasFullOversight && RbacService.IsAgent)
            {
                int currentUserId = CurrentSession.UserId;
                query = query.Where(d => d.AgentId == currentUserId);
            }

            return query
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public Deal? GetById(int id)
        {
            var item = _db.Deals
                .AsNoTracking()
                .SingleOrDefault(x => x.DealId == id);

            if (item is null) return null;

            if (!RbacService.HasFullOversight && RbacService.IsAgent && item.AgentId != CurrentSession.UserId)
                return null;

            return item;
        }

        public Deal Add(Deal deal)
        {
            deal.TenantId = TenantId;
            deal.CreatedAt = DateTime.UtcNow;

            // Apply defaults for commercial terms if not specified
            if (string.IsNullOrWhiteSpace(deal.PaymentScheme))
                deal.PaymentScheme = "Bank Financing";

            if (deal.DownPaymentPercent.HasValue && !deal.DownPaymentAmount.HasValue)
                deal.DownPaymentAmount = deal.Value * (deal.DownPaymentPercent.Value / 100m);

            if (!deal.BalanceAmount.HasValue)
                deal.BalanceAmount = deal.Value - (deal.DownPaymentAmount ?? 0);

            if (string.IsNullOrWhiteSpace(deal.ContingenciesJson))
                deal.ContingenciesJson = DealContingency.SerializeList(DealClauseLibrary.GetDefaultContingencies(deal.PaymentScheme));

            if (string.IsNullOrWhiteSpace(deal.ApprovedClauseIds))
                deal.ApprovedClauseIds = "TTL-01,TAX-01,FIN-01,TRN-01,DEF-01";

            _db.Deals.Add(deal);
            _db.SaveChanges();
            return deal;
        }

        public void Update(Deal deal)
        {
            var item = _db.Deals.SingleOrDefault(x => x.DealId == deal.DealId);
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
            item.DownPaymentAmount = deal.DownPaymentAmount;
            item.BalanceAmount = deal.BalanceAmount;
            item.CgtPayer = deal.CgtPayer;
            item.DstPayer = deal.DstPayer;
            item.TransferTaxPayer = deal.TransferTaxPayer;
            item.RegistrationFeePayer = deal.RegistrationFeePayer;
            item.ContingenciesJson = deal.ContingenciesJson;
            item.ApprovedClauseIds = deal.ApprovedClauseIds;
            item.SpecialStipulations = deal.SpecialStipulations;
            item.ContractSignedDate = deal.ContractSignedDate;

            _db.SaveChanges();
        }

        public void UpdateContingencyStatus(int dealId, int contingencyIndex, string newStatus, string? notes = null)
        {
            var item = _db.Deals.SingleOrDefault(x => x.DealId == dealId);
            if (item is null) return;

            var contingencies = DealContingency.DeserializeList(item.ContingenciesJson);
            if (contingencyIndex >= 0 && contingencyIndex < contingencies.Count)
            {
                contingencies[contingencyIndex].Status = newStatus;
                if (string.Equals(newStatus, "Satisfied", StringComparison.OrdinalIgnoreCase))
                {
                    contingencies[contingencyIndex].ResolvedAt = DateTime.UtcNow;
                }
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    contingencies[contingencyIndex].Notes = notes;
                }
                item.ContingenciesJson = DealContingency.SerializeList(contingencies);
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
                .AsNoTracking()
                .ToDictionary(c => c.CustomerId, c => $"{c.FirstName} {c.LastName}".Trim());
        }

        public Dictionary<int, string> GetPropertyAddresses()
        {
            return _db.Properties
                .AsNoTracking()
                .ToDictionary(p => p.PropertyId, p => p.Address);
        }

        public Dictionary<int, string> GetAgentNames()
        {
            return _db.Users
                .AsNoTracking()
                .ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
        }

        public List<KeyValuePair<int, string>> GetCustomerPickerList()
        {
            return _db.Customers
                .AsNoTracking()
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new KeyValuePair<int, string>(c.CustomerId, $"{c.FirstName} {c.LastName}".Trim()))
                .ToList();
        }

        public List<KeyValuePair<int, string>> GetPropertyPickerList()
        {
            return _db.Properties
                .AsNoTracking()
                .OrderBy(p => p.Address)
                .Select(p => new KeyValuePair<int, string>(p.PropertyId, $"{p.Address} (₱{p.Price:N0})"))
                .ToList();
        }

        public List<KeyValuePair<int, string>> GetAgentPickerList()
        {
            return _db.Users
                .AsNoTracking()
                .Where(u => u.Status != "inactive")
                .OrderBy(u => u.LastName)
                .Select(u => new KeyValuePair<int, string>(u.UserId, $"{u.FirstName} {u.LastName}".Trim()))
                .ToList();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}

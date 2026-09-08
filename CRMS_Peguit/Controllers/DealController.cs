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
            var connectionString =
                Environment.GetEnvironmentVariable("CRMS_CONNECTION")
                ?? throw new InvalidOperationException(
                    "CRMS_CONNECTION environment variable is not set.");

            var options = new DbContextOptionsBuilder<RealEstateDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            _db = new RealEstateDbContext(options, TenantId);
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

            _db.SaveChanges();
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

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}

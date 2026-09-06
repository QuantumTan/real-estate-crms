using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.winforms.Controllers
{
    public class CustomerController : IDisposable
    {
        private readonly RealEstateDbContext _db;

        // FIXED: was hardcoded to 1 - now uses whoever is actually logged in.
        private int TenantId => CurrentSession.TenantId;

        public CustomerController()
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

        public List<Customer> GetAll()
        {
            return _db.Customers
                .AsNoTracking()
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();
        }

        public Customer? GetById(int id)
        {
            return _db.Customers
                .AsNoTracking()
                .SingleOrDefault(x => x.CustomerId == id);
        }

        public Customer Add(Customer customer)
        {
            customer.TenantId = TenantId;
            customer.CreatedAt = DateTime.UtcNow;
            customer.IsDeleted = false;
            customer.DeletedAt = null;

            _db.Customers.Add(customer);
            _db.SaveChanges();
            return customer;
        }

        public void Update(Customer customer)
        {
            var item = _db.Customers
                .SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            item.FirstName = customer.FirstName;
            item.MiddleName = customer.MiddleName;
            item.LastName = customer.LastName;
            item.Suffix = customer.Suffix;
            item.Phone = customer.Phone;
            item.Email = customer.Email;
            item.Type = customer.Type;
            item.Status = customer.Status;
            item.AssignedAgentId = customer.AssignedAgentId;

            _db.SaveChanges();
        }

        public void SoftDelete(Customer customer)
        {
            var item = _db.Customers
                .SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void Restore(Customer customer)
        {
            var item = _db.Customers
                .IgnoreQueryFilters()
                .SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            item.IsDeleted = false;
            item.DeletedAt = null;
            _db.SaveChanges();
        }

        public void Delete(Customer customer) => SoftDelete(customer);

        // ---- NEW: for the customer detail view ----

        public string? GetAssignedAgentName(int? assignedAgentId)
        {
            if (assignedAgentId is null) return null;
            return _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == assignedAgentId)
                .Select(u => u.FullName)
                .SingleOrDefault();
        }

        // Only meaningful when Customer.Type is "seller" or "both"
        public List<Property> GetOwnedProperties(int customerId)
        {
            return _db.Properties
                .AsNoTracking()
                .Where(p => p.OwnerCustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public List<Activity> GetActivityHistory(int customerId)
        {
            return _db.Activities
                .AsNoTracking()
                .Where(a => a.RelatedCustomerId == customerId)
                .OrderByDescending(a => a.ActivityDate)
                .ToList();
        }

        // KPI counts - computed here so CustomersView doesn't need its own queries
        public CustomerKpiCounts GetKpiCounts()
        {
            var all = _db.Customers.AsNoTracking().ToList();
            var now = DateTime.UtcNow;

            return new CustomerKpiCounts
            {
                Total = all.Count,
                Active = all.Count(c => string.Equals(c.Status, "active", StringComparison.OrdinalIgnoreCase)),
                Inactive = all.Count(c => string.Equals(c.Status, "inactive", StringComparison.OrdinalIgnoreCase)),
                ThisMonth = all.Count(c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month)
            };
        }

        public void Dispose() => _db.Dispose();
    }

    public class CustomerKpiCounts
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Inactive { get; set; }
        public int ThisMonth { get; set; }
    }
}
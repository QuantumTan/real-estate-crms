using System;
using System.Collections.Generic;
using System.Linq;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.winforms.Auth;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.winforms.Controllers
{
    public class PropertyController : IDisposable
    {
        private readonly RealEstateDbContext _db;

        private int TenantId => CurrentSession.TenantId;

        public PropertyController()
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

        public List<Property> GetAll()
        {
            return _db.Properties
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenBy(x => x.Address)
                .ToList();
        }

        public Property Add(Property property)
        {
            property.TenantId = TenantId;
            property.CreatedAt = DateTime.UtcNow;

            _db.Properties.Add(property);
            _db.SaveChanges();
            return property;
        }

        public void Update(Property property)
        {
            var item = _db.Properties
                .SingleOrDefault(x => x.PropertyId == property.PropertyId);
            if (item is null) return;

            item.Address = property.Address;
            item.PropertyType = property.PropertyType;
            item.Price = property.Price;
            item.Status = property.Status;
            item.OwnerCustomerId = property.OwnerCustomerId;
            item.ListedByAgentId = property.ListedByAgentId;

            _db.SaveChanges();
        }

        public void Delete(Property property)
        {
            var item = _db.Properties
                .SingleOrDefault(x => x.PropertyId == property.PropertyId);
            if (item is null) return;

            _db.Properties.Remove(item);
            _db.SaveChanges();
        }

        public List<CustomerPickerItem> GetOwnerCustomers()
        {
            var sellerTypes = new[] { "seller", "both" };

            return _db.Customers
                .AsNoTracking()
                .Where(c => sellerTypes.Contains(c.Type.ToLower()))
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new
                {
                    c.CustomerId,
                    c.FirstName,
                    c.MiddleName,
                    c.LastName,
                    c.Suffix,
                    c.Email
                })
                .AsEnumerable()
                .Select(c => new CustomerPickerItem(
                    c.CustomerId,
                    BuildFullName(c.FirstName, c.MiddleName, c.LastName, c.Suffix),
                    c.Email))
                .ToList();
        }

        public List<AgentPickerItem> GetAgents()
        {
            var agentRoleIds = _db.Roles
                .AsNoTracking()
                .Where(r => r.RoleName.ToLower() == "agent")
                .Select(r => r.RoleId)
                .ToList();

            return _db.Users
                .AsNoTracking()
                .Where(u => agentRoleIds.Contains(u.RoleId) && u.Status.ToLower() != "inactive")
                .OrderBy(u => u.FullName)
                .Select(u => new AgentPickerItem(u.UserId, u.FullName, u.Email))
                .ToList();
        }

        public string? GetOwnerName(int ownerCustomerId)
        {
            return _db.Customers
                .AsNoTracking()
                .Where(c => c.CustomerId == ownerCustomerId)
                .Select(c => new
                {
                    c.FirstName,
                    c.MiddleName,
                    c.LastName,
                    c.Suffix
                })
                .AsEnumerable()
                .Select(c => BuildFullName(c.FirstName, c.MiddleName, c.LastName, c.Suffix))
                .SingleOrDefault();
        }

        public string? GetListedAgentName(int listedByAgentId)
        {
            return _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == listedByAgentId)
                .Select(u => u.FullName)
                .SingleOrDefault();
        }

        public void Dispose() => _db.Dispose();

        private static string BuildFullName(
            string firstName,
            string? middleName,
            string lastName,
            string? suffix)
        {
            return string.Join(" ",
                new[] { firstName, middleName, lastName, suffix }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }

    public sealed record CustomerPickerItem(int CustomerId, string FullName, string? Email)
    {
        public override string ToString() => FullName;
    }

    public sealed record AgentPickerItem(int UserId, string FullName, string Email)
    {
        public override string ToString() => FullName;
    }
}

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
            SchemaRepairService.EnsureCrmPolishColumns(_db);
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
            ApplyAssignmentDefaults(property);

            _db.Properties.Add(property);
            _db.SaveChanges();
            LogActivity("Property Created", null, null, $"Property listing '{property.Address}' was created.");
            return property;
        }

        public void Update(Property property)
        {
            var item = _db.Properties
                .SingleOrDefault(x => x.PropertyId == property.PropertyId);
            if (item is null) return;

            var oldAgentId = item.ListedByAgentId;

            item.Address = property.Address;
            item.PropertyType = property.PropertyType;
            item.Price = property.Price;
            item.Status = property.Status;
            item.OwnerCustomerId = property.OwnerCustomerId;
            item.ListedByAgentId = property.ListedByAgentId;
            item.AssignmentStatus = property.AssignmentStatus;
            item.AssignmentReviewedByUserId = property.AssignmentReviewedByUserId;
            item.AssignmentReviewedAt = property.AssignmentReviewedAt;
            item.AssignmentReviewNotes = property.AssignmentReviewNotes;

            _db.SaveChanges();

            if (oldAgentId != property.ListedByAgentId)
            {
                LogActivity("Property Assignment Changed", null, null,
                    $"Property listing '{item.Address}' assignment changed from Agent #{oldAgentId?.ToString() ?? "Unassigned"} to Agent #{property.ListedByAgentId?.ToString() ?? "Unassigned"} by User #{CurrentSession.UserId}.");
            }

            LogActivity("Property Updated", null, null, $"Property listing '{item.Address}' was updated.");
        }

        public void Delete(Property property)
        {
            var item = _db.Properties
                .SingleOrDefault(x => x.PropertyId == property.PropertyId);
            if (item is null) return;

            _db.Properties.Remove(item);
            _db.SaveChanges();
            LogActivity("Property Removed", null, null, $"Property listing '{item.Address}' was removed.");
        }

        public void ApproveAssignment(Property property, string? notes = null)
        {
            var item = _db.Properties
                .SingleOrDefault(x => x.PropertyId == property.PropertyId);
            if (item is null) return;

            item.AssignmentStatus = "approved";
            item.AssignmentReviewedByUserId = CurrentSession.UserId;
            item.AssignmentReviewedAt = DateTime.UtcNow;
            item.AssignmentReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            _db.SaveChanges();
            LogActivity("Property Assignment Approved", null, null, $"Assignment for property listing '{item.Address}' was approved.");
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
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .AsEnumerable()
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

        public string? GetListedAgentName(int? listedByAgentId)
        {
            if (listedByAgentId is null) return null;
            return _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == listedByAgentId.Value)
                .AsEnumerable()
                .Select(u => u.FullName)
                .SingleOrDefault();
        }

        public void Dispose() => _db.Dispose();

        private void LogActivity(string type, int? leadId, int? customerId, string notes)
        {
            if (CurrentSession.UserId <= 0) return;

            _db.Activities.Add(new Activity
            {
                TenantId = TenantId,
                Type = type,
                RelatedLeadId = leadId,
                RelatedCustomerId = customerId,
                LoggedByAgentId = CurrentSession.UserId,
                Notes = notes,
                ActivityDate = DateTime.UtcNow
            });
            _db.SaveChanges();
        }

        private static void ApplyAssignmentDefaults(Property property)
        {
            // R23. Default state is Unassigned — never auto-assigned to creator.
            property.ListedByAgentId = null;
            property.AssignmentStatus = "pending_review";
        }

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

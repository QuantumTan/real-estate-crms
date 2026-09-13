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
    public class CustomerController : IDisposable
    {
        private readonly RealEstateDbContext _db;

        // FIXED: was hardcoded to 1 - now uses whoever is actually logged in.
        private int TenantId => CurrentSession.TenantId;

        public CustomerController()
        {
            _db = LocalDb.CreateContext(TenantId);
        }

        public List<Customer> GetAll()
        {
            var query = _db.Customers.AsNoTracking();

            // R23 & R25 (revised): Visibility scoped to creator while Pending, assignee once assigned.
            // Manager/Admin retain full oversight (R26).
            if (!RbacService.HasFullOversight && RbacService.IsAgent)
            {
                int currentUserId = CurrentSession.UserId;
                query = query.Where(c =>
                    (c.AssignedAgentId.HasValue && c.AssignedAgentId.Value > 0)
                        ? c.AssignedAgentId.Value == currentUserId
                        : (c.CreatedByUserId.HasValue && c.CreatedByUserId.Value == currentUserId));
            }

            return query
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();
        }

        public Customer? GetById(int id)
        {
            var item = _db.Customers
                .AsNoTracking()
                .SingleOrDefault(x => x.CustomerId == id);

            if (item is null) return null;

            if (!RbacService.CanAgentViewRecord(item.AssignedAgentId, item.CreatedByUserId))
                return null;

            return item;
        }

        public Customer Add(Customer customer)
        {
            customer.TenantId = TenantId;
            customer.CreatedAt = DateTime.UtcNow;
            customer.CreatedByUserId = CurrentSession.UserId;
            customer.IsDeleted = false;
            customer.DeletedAt = null;

            if (RbacService.CanAssignRecords)
            {
                if (customer.AssignedAgentId <= 0)
                {
                    customer.AssignedAgentId = null;
                }
            }
            else
            {
                // R23: Default state is Unassigned — never auto-assigned to creator.
                // R24: Only Manager or Admin may set ownership.
                ApplyAssignmentDefaults(customer);
            }

            _db.Customers.Add(customer);
            _db.SaveChanges();
            LogActivity("Customer Created", null, customer.CustomerId, $"Customer '{customer.FullName}' was created.");
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

            // R24: Only Manager or Admin may set or change ownership.
            if (RbacService.CanAssignRecords)
            {
                var oldAgentId = item.AssignedAgentId;
                var newAgentId = customer.AssignedAgentId <= 0 ? null : customer.AssignedAgentId;

                item.AssignedAgentId = newAgentId;
                item.AssignmentStatus = customer.AssignmentStatus;
                item.AssignmentReviewedByUserId = customer.AssignmentReviewedByUserId;
                item.AssignmentReviewedAt = customer.AssignmentReviewedAt;
                item.AssignmentReviewNotes = customer.AssignmentReviewNotes;

                if (oldAgentId != newAgentId)
                {
                    LogActivity("Customer Assignment Changed", null, item.CustomerId,
                        $"Customer '{item.FullName}' assignment changed from Agent #{oldAgentId?.ToString() ?? "Unassigned"} to Agent #{newAgentId?.ToString() ?? "Unassigned"} by User #{CurrentSession.UserId}.");
                }
            }

            _db.SaveChanges();

            LogActivity("Customer Updated", null, item.CustomerId, $"Customer '{item.FullName}' was updated.");
        }

        public void SoftDelete(Customer customer)
        {
            var item = _db.Customers
                .SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            _db.SaveChanges();
            LogActivity("Customer Archived", null, item.CustomerId, $"Customer '{item.FullName}' was archived.");
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

        public string? GetAssignedAgentName(int? assignedAgentId)
        {
            if (assignedAgentId is null) return null;
            return _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == assignedAgentId)
                .AsEnumerable()
                .Select(u => u.FullName)
                .SingleOrDefault();
        }

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

        public void ApproveAssignment(Customer customer, string? notes = null)
        {
            var item = _db.Customers.SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            item.AssignmentStatus = "approved";
            item.AssignmentReviewedByUserId = CurrentSession.UserId;
            item.AssignmentReviewedAt = DateTime.UtcNow;
            item.AssignmentReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            _db.SaveChanges();
            LogActivity("Customer Assignment Approved", null, item.CustomerId, $"Assignment for '{item.FullName}' was approved.");
        }

        public void AssignAgent(Customer customer, int? agentId, bool approve = true, string? notes = null)
        {
            var item = _db.Customers.SingleOrDefault(x => x.CustomerId == customer.CustomerId);
            if (item is null) return;

            var oldAgentId = item.AssignedAgentId;
            var newAgentId = agentId <= 0 ? null : agentId;

            item.AssignedAgentId = newAgentId;
            item.AssignmentStatus = approve ? "approved" : "pending_review";
            item.AssignmentReviewedByUserId = CurrentSession.UserId;
            item.AssignmentReviewedAt = DateTime.UtcNow;
            item.AssignmentReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

            _db.SaveChanges();

            if (oldAgentId != newAgentId)
            {
                LogActivity("Customer Assignment Changed", null, item.CustomerId,
                    $"Customer '{item.FullName}' assigned to Agent #{newAgentId?.ToString() ?? "Unassigned"} by User #{CurrentSession.UserId}.");
            }
        }

        public List<Customer> GetPendingReview()
        {
            return _db.Customers
                .AsNoTracking()
                .Where(c => c.AssignmentStatus == "pending_review" || c.AssignedAgentId == null)
                .OrderByDescending(c => c.CreatedAt)
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

        public void LogEmail(Customer customer, string subject)
        {
            LogActivity("Email", null, customer.CustomerId, $"Email sent to '{customer.FullName}'. Subject: {subject}");
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

        private static void ApplyAssignmentDefaults(Customer customer)
        {
            // R23. Default state is Unassigned — never auto-assigned to creator.
            customer.AssignedAgentId = null;
            customer.AssignmentStatus = "pending_review";
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

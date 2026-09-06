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
    public class LeadController : IDisposable
    {
        private readonly RealEstateDbContext _db;

        // FIXED: was hardcoded to 1 - now uses whoever is actually logged in.
        private int TenantId => CurrentSession.TenantId;

        public LeadController()
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

        public List<Lead> GetAll()
        {
            return _db.Leads
                .AsNoTracking()
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList();
        }

        public Lead? GetById(int id)
        {
            return _db.Leads
                .AsNoTracking()
                .SingleOrDefault(x => x.LeadId == id);
        }

        public Lead Add(Lead lead)
        {
            lead.TenantId = TenantId;
            lead.CreatedAt = DateTime.UtcNow;
            lead.IsDeleted = false;
            lead.DeletedAt = null;
            ApplyAssignmentDefaults(lead);

            _db.Leads.Add(lead);
            _db.SaveChanges();
            LogActivity("Lead Created", lead.LeadId, null, $"Lead '{lead.FullName}' was created.");
            return lead;
        }

        public void Update(Lead lead)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            var oldAgentId = item.AssignedAgentId;

            item.FirstName = lead.FirstName;
            item.MiddleName = lead.MiddleName;
            item.LastName = lead.LastName;
            item.Suffix = lead.Suffix;
            item.Phone = lead.Phone;
            item.Email = lead.Email;
            item.Source = lead.Source;
            item.Stage = lead.Stage;
            item.Notes = lead.Notes;
            item.Priority = lead.Priority;
            item.ExpectedValue = lead.ExpectedValue;
            item.AssignedAgentId = lead.AssignedAgentId;
            item.AssignmentStatus = lead.AssignmentStatus;
            item.AssignmentReviewedByUserId = lead.AssignmentReviewedByUserId;
            item.AssignmentReviewedAt = lead.AssignmentReviewedAt;
            item.AssignmentReviewNotes = lead.AssignmentReviewNotes;
            _db.SaveChanges();

            if (oldAgentId != lead.AssignedAgentId)
            {
                LogActivity("Lead Assignment Changed", item.LeadId, null,
                    $"Lead '{item.FullName}' assignment changed from Agent #{oldAgentId?.ToString() ?? "Unassigned"} to Agent #{lead.AssignedAgentId?.ToString() ?? "Unassigned"} by User #{CurrentSession.UserId}.");
            }

            LogActivity("Lead Updated", item.LeadId, null, $"Lead '{item.FullName}' was updated.");
        }

        public void SoftDelete(Lead lead)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;
            _db.SaveChanges();
            LogActivity("Lead Archived", item.LeadId, null, $"Lead '{item.FullName}' was archived.");
        }
        public void Restore(Lead lead)
        {
            var item = _db.Leads
                .IgnoreQueryFilters()
                .SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            item.IsDeleted = false;
            item.DeletedAt = null;
            _db.SaveChanges();
        }

        public Customer ConvertToCustomer(Lead lead)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null)
                throw new InvalidOperationException("Lead not found.");

            if (string.Equals(item.Stage, "converted", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This lead has already been converted.");

            var customer = new Customer
            {
                TenantId = item.TenantId,
                FirstName = item.FirstName,
                MiddleName = item.MiddleName,
                LastName = item.LastName,
                Suffix = item.Suffix,
                Email = item.Email,
                Phone = item.Phone,
                Type = "buyer",
                Status = "active",
                AssignedAgentId = item.AssignedAgentId,
                AssignmentStatus = item.AssignmentStatus,
                AssignmentReviewedByUserId = item.AssignmentReviewedByUserId,
                AssignmentReviewedAt = item.AssignmentReviewedAt,
                AssignmentReviewNotes = item.AssignmentReviewNotes,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null
            };

            _db.Customers.Add(customer);
            _db.SaveChanges();

            item.Stage = "converted";
            item.ConvertedCustomerId = customer.CustomerId;
            _db.SaveChanges();
            LogActivity("Lead Converted", item.LeadId, customer.CustomerId, $"Lead '{item.FullName}' was converted to customer #{customer.CustomerId}.");

            return customer;
        }

        public void MarkLost(Lead lead)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            item.Stage = "lost";
            _db.SaveChanges();
            LogActivity("Lead Lost", item.LeadId, null, $"Lead '{item.FullName}' was marked as lost.");
        }

        public void RestoreFromLost(Lead lead)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            item.Stage = "contacted";
            _db.SaveChanges();
            LogActivity("Lead Restored", item.LeadId, null, $"Lead '{item.FullName}' was restored from lost.");
        }

        public void ApproveAssignment(Lead lead, string? notes = null)
        {
            var item = _db.Leads.SingleOrDefault(x => x.LeadId == lead.LeadId);
            if (item is null) return;

            item.AssignmentStatus = "approved";
            item.AssignmentReviewedByUserId = CurrentSession.UserId;
            item.AssignmentReviewedAt = DateTime.UtcNow;
            item.AssignmentReviewNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            _db.SaveChanges();
            LogActivity("Lead Assignment Approved", item.LeadId, null, $"Assignment for '{item.FullName}' was approved.");
        }

        public void LogEmail(Lead lead, string subject)
        {
            LogActivity("Email", lead.LeadId, null, $"Email sent to '{lead.FullName}'. Subject: {subject}");
        }

        // ---- NEW: for the lead detail view ----

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

        public List<Activity> GetActivityHistory(int leadId)
        {
            return _db.Activities
                .AsNoTracking()
                .Where(a => a.RelatedLeadId == leadId)
                .OrderByDescending(a => a.ActivityDate)
                .ToList();
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

        private static void ApplyAssignmentDefaults(Lead lead)
        {
            // R23. Default state is Unassigned — never auto-assigned to creator.
            lead.AssignedAgentId = null;
            lead.AssignmentStatus = "pending_review";
        }

        public void Dispose() => _db.Dispose();
    }
}

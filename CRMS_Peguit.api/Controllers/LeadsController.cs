using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeadsController : ControllerBase
    {
        private readonly RealEstateDbContext _db;

        public LeadsController(RealEstateDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leads = await _db.Leads.ToListAsync();
            return Ok(leads);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lead = await _db.Leads
                .SingleOrDefaultAsync(x => x.LeadId == id);

            return lead is null ? NotFound() : Ok(lead);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Lead lead)
        {
            if (lead.PersonId <= 0 && lead.Person == null)
            {
                lead.Person = new CRMS_Peguit.domain.entities.Person
                {
                    FirstName = lead.FirstName,
                    MiddleName = lead.MiddleName,
                    LastName = lead.LastName,
                    Suffix = lead.Suffix,
                    Email = lead.Email,
                    Phone = lead.Phone
                };
            }
            if (lead.CreatedByUserId <= 0)
            {
                lead.CreatedByUserId = 1;
            }
            lead.CreatedAt = DateTime.UtcNow;
            lead.IsDeleted = false;
            lead.DeletedAt = null;
            lead.AssignedAgentId = null; // R23. Default state is Unassigned
            lead.AssignmentStatus = string.IsNullOrWhiteSpace(lead.AssignmentStatus)
                ? "pending_review"
                : lead.AssignmentStatus;

            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = lead.LeadId }, lead);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Lead updated)
        {
            var item = await _db.Leads
                .SingleOrDefaultAsync(x => x.LeadId == id);
            if (item is null) return NotFound();

            item.FirstName = updated.FirstName;
            item.MiddleName = updated.MiddleName;
            item.LastName = updated.LastName;
            item.Suffix = updated.Suffix;
            item.Phone = updated.Phone;
            item.Email = updated.Email;
            item.Source = updated.Source;
            item.Stage = updated.Stage;
            item.Notes = updated.Notes;
            item.Priority = updated.Priority;
            item.ExpectedValue = updated.ExpectedValue;
            item.AssignedAgentId = updated.AssignedAgentId;
            item.AssignmentStatus = updated.AssignmentStatus;
            item.AssignmentReviewedByUserId = updated.AssignmentReviewedByUserId;
            item.AssignmentReviewedAt = updated.AssignmentReviewedAt;
            item.AssignmentReviewNotes = updated.AssignmentReviewNotes;

            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPost("{id:int}/convert")]
        public async Task<IActionResult> ConvertToCustomer(int id)
        {
            var item = await _db.Leads.SingleOrDefaultAsync(x => x.LeadId == id);
            if (item is null) return NotFound();

            if (string.Equals(item.Stage, "converted", StringComparison.OrdinalIgnoreCase))
                return BadRequest("This lead has already been converted.");

            var customer = new Customer
            {
                PersonId = item.PersonId,
                Type = "buyer",
                Status = "active",
                AssignedAgentId = item.AssignedAgentId,
                CreatedByUserId = item.CreatedByUserId,
                AssignmentStatus = item.AssignmentStatus,
                AssignmentReviewedByUserId = item.AssignmentReviewedByUserId,
                AssignmentReviewedAt = item.AssignmentReviewedAt,
                AssignmentReviewNotes = item.AssignmentReviewNotes,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            item.Stage = "converted";
            item.ConvertedCustomerId = customer.CustomerId;
            await _db.SaveChangesAsync();

            return Ok(customer);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Leads
                .SingleOrDefaultAsync(x => x.LeadId == id);
            if (item is null) return NotFound();

            // Soft delete
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly RealEstateDbContext _db;

        public CustomersController(RealEstateDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _db.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _db.Customers
                .SingleOrDefaultAsync(x => x.CustomerId == id);

            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (customer.PersonId <= 0 && customer.Person == null)
            {
                customer.Person = new CRMS_Peguit.domain.entities.Person
                {
                    FirstName = customer.FirstName,
                    MiddleName = customer.MiddleName,
                    LastName = customer.LastName,
                    Suffix = customer.Suffix,
                    Email = customer.Email,
                    Phone = customer.Phone
                };
            }
            if (customer.CreatedByUserId <= 0)
            {
                customer.CreatedByUserId = 1;
            }
            customer.CreatedAt = DateTime.UtcNow;
            customer.IsDeleted = false;
            customer.DeletedAt = null;
            customer.AssignedAgentId = null; // R23. Default state is Unassigned
            customer.AssignmentStatus = string.IsNullOrWhiteSpace(customer.AssignmentStatus)
                ? "pending_review"
                : customer.AssignmentStatus;

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = customer.CustomerId }, customer);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Customer updated)
        {
            var item = await _db.Customers
                .SingleOrDefaultAsync(x => x.CustomerId == id);
            if (item is null) return NotFound();

            item.FirstName = updated.FirstName;
            item.MiddleName = updated.MiddleName;
            item.LastName = updated.LastName;
            item.Suffix = updated.Suffix;
            item.Phone = updated.Phone;
            item.Email = updated.Email;
            item.Type = updated.Type;
            item.Status = updated.Status;
            item.AssignedAgentId = updated.AssignedAgentId;
            item.AssignmentStatus = updated.AssignmentStatus;
            item.AssignmentReviewedByUserId = updated.AssignmentReviewedByUserId;
            item.AssignmentReviewedAt = updated.AssignmentReviewedAt;
            item.AssignmentReviewNotes = updated.AssignmentReviewNotes;

            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Customers
                .SingleOrDefaultAsync(x => x.CustomerId == id);
            if (item is null) return NotFound();

            // Soft delete
            item.IsDeleted = true;
            item.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

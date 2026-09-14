using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportTicketsController : ControllerBase
    {
        private readonly RealEstateDbContext _db;

        public SupportTicketsController(RealEstateDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.SupportTickets.ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _db.SupportTickets.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SupportTicket ticket)
        {
            if (ticket.RaisedByUserId <= 0)
            {
                ticket.RaisedByUserId = 1;
            }
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.AssignedToUserId = null; // R23. Default state is Unassigned
            if (string.IsNullOrWhiteSpace(ticket.Category)) ticket.Category = "Other";
            if (string.IsNullOrWhiteSpace(ticket.Priority)) ticket.Priority = "Medium";
            if (string.IsNullOrWhiteSpace(ticket.Status)) ticket.Status = "Open";

            ticket.DueDate = (ticket.Priority.ToLower()) switch
            {
                "high" or "urgent" => ticket.CreatedAt.AddHours(24),
                "medium" => ticket.CreatedAt.AddDays(3),
                "low" => ticket.CreatedAt.AddDays(7),
                _ => ticket.CreatedAt.AddDays(3)
            };

            ticket.TicketNumber = "TCK-TEMP";
            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync();

            ticket.TicketNumber = $"TCK-{ticket.TicketId:D5}";
            await _db.SaveChangesAsync();

            return Created($"/api/supporttickets/{ticket.TicketId}", ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SupportTicket updated)
        {
            var item = await _db.SupportTickets.FindAsync(id);
            if (item is null) return NotFound();

            item.CustomerId = updated.CustomerId;
            item.RaisedByUserId = updated.RaisedByUserId;
            item.AssignedToUserId = updated.AssignedToUserId;
            item.Category = updated.Category;
            item.Description = updated.Description;
            item.Priority = updated.Priority;
            item.Status = updated.Status;
            item.DueDate = updated.DueDate;
            item.FirstRespondedAt = updated.FirstRespondedAt;
            item.ResolvedAt = updated.ResolvedAt;

            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.SupportTickets.FindAsync(id);
            if (item is null) return NotFound();

            _db.SupportTickets.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
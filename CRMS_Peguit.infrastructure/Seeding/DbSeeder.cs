using CRMS_Peguit.domain.entities;
using CRMS_Peguit.domain.Entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.infrastructure.Seeding
{
    // Run this once (e.g. from a temporary button in the app, or a small
    // console runner) to get test accounts into the database. Delete or
    // guard this before shipping - it's for local/dev testing only.
    public static class DbSeeder
    {
        public static async Task SeedTestUsersAsync(RealEstateDbContext db, int tenantId = 1)
        {
            if (await db.Roles.AnyAsync(r => r.TenantId == tenantId))
                return; // already seeded

            var adminRole = new Role { TenantId = tenantId, RoleName = "Admin" };
            var managerRole = new Role { TenantId = tenantId, RoleName = "Manager" };
            var agentRole = new Role { TenantId = tenantId, RoleName = "Agent" };

            db.Roles.AddRange(adminRole, managerRole, agentRole);
            await db.SaveChangesAsync(); // so RoleId values are generated before use

            var users = new[]
            {
                new User
                {
                    TenantId = tenantId,
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@test.com",
                    PasswordHash = PasswordHasher.Hash("Admin123!"),
                    RoleId = adminRole.RoleId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    TenantId = tenantId,
                    FirstName = "Test",
                    LastName = "Manager",
                    Email = "manager@test.com",
                    PasswordHash = PasswordHasher.Hash("Manager123!"),
                    RoleId = managerRole.RoleId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    TenantId = tenantId,
                    FirstName = "Test",
                    LastName = "Agent",
                    Email = "agent@test.com",
                    PasswordHash = PasswordHasher.Hash("Agent123!"),
                    RoleId = agentRole.RoleId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Users.AddRange(users);
            await db.SaveChangesAsync();

            await SeedSampleDataAsync(db, tenantId);
        }

        public static async Task SeedSampleDataAsync(RealEstateDbContext db, int tenantId = 1)
        {
            if (await db.Customers.AnyAsync(c => c.TenantId == tenantId))
                return;

            var agent = await db.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == "agent@test.com");
            int agentId = agent?.UserId ?? 1;

            var customers = new[]
            {
                new Customer
                {
                    TenantId = tenantId,
                    FirstName = "Maria",
                    LastName = "Santos",
                    Email = "maria.santos@example.com",
                    Phone = "09171234567",
                    Type = "buyer",
                    Status = "active",
                    AssignmentStatus = "approved",
                    AssignedAgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    TenantId = tenantId,
                    FirstName = "Juan",
                    LastName = "Dela Cruz",
                    Email = "juan.delacruz@example.com",
                    Phone = "09181234567",
                    Type = "seller",
                    Status = "active",
                    AssignmentStatus = "approved",
                    AssignedAgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();

            var properties = new[]
            {
                new Property
                {
                    TenantId = tenantId,
                    Address = "Block 12 Lot 5, Grand Villas, Davao City",
                    PropertyType = "house",
                    Price = 4500000m,
                    Status = "available",
                    OwnerCustomerId = customers[1].CustomerId,
                    ListedByAgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    TenantId = tenantId,
                    Address = "Unit 1502, Azure Modern Condominium, Cebu City",
                    PropertyType = "condo",
                    Price = 3200000m,
                    Status = "available",
                    OwnerCustomerId = customers[1].CustomerId,
                    ListedByAgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Properties.AddRange(properties);
            await db.SaveChangesAsync();

            var leads = new[]
            {
                new Lead
                {
                    TenantId = tenantId,
                    FirstName = "Carlos",
                    LastName = "Mendoza",
                    Email = "carlos.mendoza@example.com",
                    Phone = "09201234567",
                    Source = "Facebook",
                    Stage = "qualified",
                    Priority = "high",
                    ExpectedValue = 4500000m,
                    AssignedAgentId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Leads.AddRange(leads);
            await db.SaveChangesAsync();

            var deals = new[]
            {
                new Deal
                {
                    TenantId = tenantId,
                    CustomerId = customers[0].CustomerId,
                    PropertyId = properties[0].PropertyId,
                    AgentId = agentId,
                    Value = 4500000m,
                    CommissionRate = 0.05m,
                    Stage = "Offer",
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Deals.AddRange(deals);
            await db.SaveChangesAsync();
        }
    }
}
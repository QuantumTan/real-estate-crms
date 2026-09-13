using System;
using System.Threading.Tasks;
using CRMS_Peguit.domain.entities;
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

            var adminPerson = new Person { FirstName = "System", LastName = "Admin", Email = "admin@test.com" };
            var managerPerson = new Person { FirstName = "Test", LastName = "Manager", Email = "manager@test.com" };
            var agentPerson = new Person { FirstName = "Test", LastName = "Agent", Email = "agent@test.com" };

            var users = new[]
            {
                new User
                {
                    Person = adminPerson,
                    PasswordHash = PasswordHasher.Hash("Admin123!"),
                    RoleId = adminRole.RoleId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Person = managerPerson,
                    PasswordHash = PasswordHasher.Hash("Manager123!"),
                    RoleId = managerRole.RoleId,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Person = agentPerson,
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
            if (await db.Customers.AnyAsync())
                return;

            var agent = await db.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Person.Email == "agent@test.com");
            int agentId = agent?.UserId ?? 1;

            var custPerson1 = new Person
            {
                FirstName = "Maria",
                LastName = "Santos",
                Email = "maria.santos@example.com",
                Phone = "09171234567"
            };

            var custPerson2 = new Person
            {
                FirstName = "Juan",
                LastName = "Dela Cruz",
                Email = "juan.delacruz@example.com",
                Phone = "09181234567"
            };

            var customers = new[]
            {
                new Customer
                {
                    Person = custPerson1,
                    Type = "buyer",
                    Status = "active",
                    AssignmentStatus = "approved",
                    AssignedAgentId = agentId,
                    CreatedByUserId = agentId,
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    Person = custPerson2,
                    Type = "seller",
                    Status = "active",
                    AssignmentStatus = "approved",
                    AssignedAgentId = agentId,
                    CreatedByUserId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();

            var properties = new[]
            {
                new Property
                {
                    Address = "Block 12 Lot 5, Grand Villas, Davao City",
                    PropertyType = "house",
                    Price = 4500000m,
                    Status = "available",
                    OwnerCustomerId = customers[1].CustomerId,
                    ListedByAgentId = agentId,
                    CreatedByUserId = agentId,
                    CreatedAt = DateTime.UtcNow
                },
                new Property
                {
                    Address = "Unit 1502, Azure Modern Condominium, Cebu City",
                    PropertyType = "condo",
                    Price = 3200000m,
                    Status = "available",
                    OwnerCustomerId = customers[1].CustomerId,
                    ListedByAgentId = agentId,
                    CreatedByUserId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Properties.AddRange(properties);
            await db.SaveChangesAsync();

            var leadPerson = new Person
            {
                FirstName = "Carlos",
                LastName = "Mendoza",
                Email = "carlos.mendoza@example.com",
                Phone = "09201234567"
            };

            var leads = new[]
            {
                new Lead
                {
                    Person = leadPerson,
                    Source = "Facebook",
                    Stage = "qualified",
                    Priority = "high",
                    ExpectedValue = 4500000m,
                    AssignedAgentId = agentId,
                    CreatedByUserId = agentId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            db.Leads.AddRange(leads);
            await db.SaveChangesAsync();

            var deals = new[]
            {
                new Deal
                {
                    CustomerId = customers[0].CustomerId,
                    PropertyId = properties[0].PropertyId,
                    AgentId = agentId,
                    CreatedByUserId = agentId,
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
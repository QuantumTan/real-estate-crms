using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.infrastructure.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedTestUsersAsync(RealEstateDbContext db, int tenantId = 1)
        {
            if (!await db.Roles.AnyAsync(r => r.TenantId == tenantId))
            {
                var adminRole = new Role { TenantId = tenantId, RoleName = "Admin" };
                var managerRole = new Role { TenantId = tenantId, RoleName = "Manager" };
                var agentRole = new Role { TenantId = tenantId, RoleName = "Agent" };

                db.Roles.AddRange(adminRole, managerRole, agentRole);
                await db.SaveChangesAsync();

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
            }

            await SeedSampleDataAsync(db, tenantId);
        }

        public static async Task SeedSampleDataAsync(RealEstateDbContext db, int tenantId = 1)
        {
            var agent = await db.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Person.Email == "agent@test.com");
            int agentId = agent?.UserId ?? 1;

            if (!await db.Customers.AnyAsync())
            {
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
            }

            if (!await db.Properties.AnyAsync())
            {
                var firstCust = await db.Customers.FirstOrDefaultAsync();
                int ownerId = firstCust?.CustomerId ?? 1;

                var properties = new[]
                {
                    new Property
                    {
                        Address = "Block 12 Lot 5, Grand Villas, Davao City",
                        PropertyType = "house",
                        Price = 4500000m,
                        Status = "available",
                        OwnerCustomerId = ownerId,
                        ListedByAgentId = agentId,
                        CreatedByUserId = agentId,
                        AssignmentStatus = "approved",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Property
                    {
                        Address = "Unit 1502, Azure Modern Condominium, Cebu City",
                        PropertyType = "condo",
                        Price = 3200000m,
                        Status = "available",
                        OwnerCustomerId = ownerId,
                        ListedByAgentId = agentId,
                        CreatedByUserId = agentId,
                        AssignmentStatus = "approved",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                db.Properties.AddRange(properties);
                await db.SaveChangesAsync();
            }

            if (!await db.Leads.AnyAsync())
            {
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
                        AssignmentStatus = "approved",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                db.Leads.AddRange(leads);
                await db.SaveChangesAsync();
            }

            // Ensure at least 320 realistic transactions exist
            await SeedTransactionsAsync(db, targetCount: 320, tenantId);
            await SeedLeadsAndTicketsAsync(db, tenantId);
        }

        /// <summary>
        /// Seeds 300+ realistic real estate transactions (Deals) across various stages, properties, customers, and dates.
        /// </summary>
        public static async Task<int> SeedTransactionsAsync(RealEstateDbContext db, int targetCount = 320, int tenantId = 1)
        {
            int currentDealCount = await db.Deals.CountAsync();
            if (currentDealCount >= targetCount)
            {
                return 0; // Already satisfied
            }

            int dealsToCreate = targetCount - currentDealCount;
            if (dealsToCreate < 300 && currentDealCount < 10)
            {
                dealsToCreate = targetCount; // Seed the full target volume for fresh/near-empty DBs
            }

            // 1. Ensure active agents exist
            var agents = await db.Users
                .Include(u => u.Role)
                .Include(u => u.Person)
                .Where(u => u.Status.ToLower() == "active")
                .ToListAsync();

            var salesAgents = agents
                .Where(u => u.Role != null && u.Role.RoleName.Equals("Agent", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (salesAgents.Count == 0)
            {
                salesAgents = agents;
            }

            if (salesAgents.Count == 0)
            {
                var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId)
                    ?? new Role { TenantId = tenantId, RoleName = "Agent" };
                if (defaultRole.RoleId == 0)
                {
                    db.Roles.Add(defaultRole);
                    await db.SaveChangesAsync();
                }

                var defaultPerson = new Person { FirstName = "Default", LastName = "Agent", Email = "agent@nexacrm.local" };
                var defaultUser = new User
                {
                    Person = defaultPerson,
                    PasswordHash = PasswordHasher.Hash("Agent123!"),
                    RoleId = defaultRole.RoleId,
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(defaultUser);
                await db.SaveChangesAsync();
                salesAgents.Add(defaultUser);
            }

            int fallbackUserId = salesAgents[0].UserId;

            // 2. Ensure a rich pool of customers (at least 50)
            var existingCustomers = await db.Customers.Include(c => c.Person).Where(c => !c.IsDeleted).ToListAsync();
            if (existingCustomers.Count < 50)
            {
                var newCustomers = GenerateCustomerPool(55 - existingCustomers.Count, salesAgents, fallbackUserId);
                db.Customers.AddRange(newCustomers);
                await db.SaveChangesAsync();
                existingCustomers = await db.Customers.Include(c => c.Person).Where(c => !c.IsDeleted).ToListAsync();
            }

            // 3. Ensure a rich pool of properties (at least 60)
            var existingProperties = await db.Properties.ToListAsync();
            if (existingProperties.Count < 60)
            {
                var newProperties = GeneratePropertyCatalog(65 - existingProperties.Count, existingCustomers, salesAgents, fallbackUserId);
                db.Properties.AddRange(newProperties);
                await db.SaveChangesAsync();
                existingProperties = await db.Properties.ToListAsync();
            }

            // 4. Generate 320+ realistic transactions (Deals)
            var deals = GenerateDealTransactions(dealsToCreate, existingCustomers, existingProperties, salesAgents, fallbackUserId);
            db.Deals.AddRange(deals);
            await db.SaveChangesAsync();

            return deals.Count;
        }

        private static List<Customer> GenerateCustomerPool(int count, List<User> agents, int fallbackUserId)
        {
            var firstNames = new[]
            {
                "Maria", "Juan", "Carlos", "Lourdes", "Jose", "Ana", "Miguel", "Carmela", "Eduardo", "Teresa",
                "Ferdinand", "Patricia", "Gabriel", "Isabel", "Ramon", "Rowena", "Antonio", "Elena", "Ricardo", "Cristina",
                "Paolo", "Kristine", "Angelo", "Bianca", "Marco", "Camille", "Rafael", "Stephanie", "Enrico", "Katrina",
                "Dante", "Jasmine", "Leandro", "Clarissa", "Benjamin", "Rochelle", "Dominic", "Vanessa", "Victor", "Giselle",
                "Arnel", "Rowell", "Maricel", "Renato", "Liezl", "Glenn", "Aileen", "Dennis", "Bernadette", "Noel"
            };

            var lastNames = new[]
            {
                "Santos", "Dela Cruz", "Mendoza", "Reyes", "Bautista", "Aquino", "Garcia", "Gonzales", "Ramos", "Lopez",
                "Fernandez", "Castillo", "Villanueva", "Torres", "Salazar", "Rivera", "Mercado", "Pascual", "Valenzuela", "Naval",
                "Tan", "Sy", "Lim", "Co", "Chua", "Ong", "Go", "Uy", "Soriano", "Corpuz",
                "Espiritu", "Manalo", "Tolentino", "Alcantara", "Santiago", "Morales", "Perez", "Guerrero", "De Leon", "Cabrera"
            };

            var rnd = new Random(101);
            var customers = new List<Customer>();

            for (int i = 0; i < count; i++)
            {
                string fn = firstNames[rnd.Next(firstNames.Length)];
                string ln = lastNames[rnd.Next(lastNames.Length)];
                string email = $"{fn.ToLower()}.{ln.ToLower()}{rnd.Next(10, 999)}@example.ph";
                string phone = $"09{rnd.Next(10, 99)}{rnd.Next(1000000, 9999999)}";

                var person = new Person
                {
                    FirstName = fn,
                    LastName = ln,
                    Email = email,
                    Phone = phone
                };

                var assignedAgent = agents[rnd.Next(agents.Count)];
                var customer = new Customer
                {
                    Person = person,
                    Type = rnd.Next(100) < 65 ? "buyer" : "seller",
                    Status = "active",
                    AssignmentStatus = "approved",
                    AssignedAgentId = assignedAgent.UserId,
                    CreatedByUserId = assignedAgent.UserId,
                    CreatedAt = DateTime.UtcNow.AddMonths(-rnd.Next(1, 18)).AddDays(-rnd.Next(1, 28))
                };

                customers.Add(customer);
            }

            return customers;
        }

        private static List<Property> GeneratePropertyCatalog(int count, List<Customer> customers, List<User> agents, int fallbackUserId)
        {
            var propertyTemplates = new (string Address, string Type, decimal Price)[]
            {
                // Condos - Metro Manila & Regional Centers
                ("Unit 1204, One Serendra, Bonifacio Global City, Taguig", "condo", 18500000m),
                ("Unit 802, The Rise Makati, Malugay St, San Antonio, Makati", "condo", 7800000m),
                ("Unit 24B, Proscenium at Rockwell, Makati City", "condo", 24000000m),
                ("Unit 1805, Azure Urban Resort Residences, Parañaque City", "condo", 4200000m),
                ("Unit 912, The Florence, McKinley Hill, Taguig", "condo", 6500000m),
                ("Unit 301, Solinea Tower 2, Cebu Business Park, Cebu City", "condo", 5800000m),
                ("Unit 1408, Marco Polo Residences, Nivel Hills, Cebu City", "condo", 7200000m),
                ("Unit 703, Abreeza Residences, Bajada, Davao City", "condo", 5100000m),
                ("Unit 510, Avida Towers Centera, Mandaluyong City", "condo", 4600000m),
                ("Unit 1601, Light Residences, EDSA, Mandaluyong City", "condo", 3800000m),
                ("Unit 2203, Two Maridien, High Street South, BGC, Taguig", "condo", 15200000m),
                ("Unit 1109, Sheridan Towers, Pasig City", "condo", 6300000m),
                ("Unit 808, Flair Towers, Reliance St, Mandaluyong City", "condo", 5500000m),
                ("Unit 405, The Residences at Commonwealth, Quezon City", "condo", 4100000m),
                ("Unit 1902, Megaworld Iloilo Business Park, Mandurriao, Iloilo", "condo", 4900000m),
                ("Unit 615, 32 Sanson by Rockwell, Lahug, Cebu City", "condo", 9800000m),
                ("Unit 1020, One Oasis Davao, Eco West Drive, Davao City", "condo", 3500000m),
                ("Unit 1508, The Lerato Tower 1, Bel-Air, Makati City", "condo", 8900000m),
                ("Unit 28A, Grand Hyatt Manila Residences, BGC, Taguig", "condo", 36000000m),
                ("Unit 1704, Tivoli Garden Residences, Coronado, Mandaluyong", "condo", 4700000m),

                // Houses & Subdivisions
                ("Lot 14 Blk 8, Ayala Alabang Village, Muntinlupa City", "house", 38000000m),
                ("Blk 5 Lot 12, Hillsborough Alabang, Muntinlupa", "house", 26000000m),
                ("Blk 2 Lot 9, Corinthian Gardens, Quezon City", "house", 45000000m),
                ("Lot 22 Blk 3, Greenmeadows Subdivision, Quezon City", "house", 42000000m),
                ("Blk 17 Lot 4, Valle Verde 5, Pasig City", "house", 31000000m),
                ("Lot 8 Blk 10, San Lorenzo Village, Makati City", "house", 48000000m),
                ("Blk 3 Lot 15, Magallanes Village, Makati City", "house", 35000000m),
                ("Blk 9 Lot 7, Grand Villas Subdivision, Buhangin, Davao City", "house", 8500000m),
                ("Lot 11 Blk 2, Maria Luisa Estate Park, Banilad, Cebu City", "house", 22000000m),
                ("Blk 6 Lot 18, North Town Homes, Cabancalan, Mandaue City", "house", 16500000m),
                ("Blk 4 Lot 21, South Peak Subdivision, San Pedro, Laguna", "house", 5200000m),
                ("Blk 8 Lot 3, Woodridge Park, Ma-a, Davao City", "house", 9800000m),
                ("Blk 12 Lot 16, Ladislawa Garden Village, Buhangin, Davao City", "house", 12500000m),
                ("Blk 15 Lot 22, Portofino Heights, Daang Hari, Las Piñas", "house", 21000000m),
                ("Blk 7 Lot 10, Ayala Greenfield Estates, Calamba, Laguna", "house", 17800000m),
                ("Lot 31 Blk 6, Bel-Air Village Phase 2, Makati City", "house", 39000000m),

                // Townhouses
                ("Townhouse Unit 4, Mahogany Place 3, Acacia Estates, Taguig", "townhouse", 14500000m),
                ("Townhouse 2B, Scout Tuason, Diliman, Quezon City", "townhouse", 11200000m),
                ("Townhouse 7, Gilmore Townhomes, New Manila, Quezon City", "townhouse", 16800000m),
                ("Unit C-12, Ametta Place, Mercedes Ave, Pasig City", "townhouse", 9500000m),
                ("Townhouse Unit 8, Alabang 400 Village, Muntinlupa City", "townhouse", 10800000m),
                ("Townhouse 3A, West Greenhills, San Juan City", "townhouse", 22500000m),
                ("Townhouse 10, Casa Verde, Pasig City", "townhouse", 12900000m),
                ("Townhouse Unit 5, Capitol Green Village, Tandang Sora, QC", "townhouse", 8700000m),
                ("Townhouse 6, Loyola Grand Villas, Quezon City", "townhouse", 15400000m),
                ("Townhouse 1B, Horseshoe Village, Quezon City", "townhouse", 19500000m),

                // Lots & Commercial
                ("Commercial Lot 105, Madrigal Business Park, Alabang, Muntinlupa", "commercial", 42000000m),
                ("Prime Commercial Space 301, IT Park, Lahug, Cebu City", "commercial", 19000000m),
                ("Corner Commercial Lot, J.P. Laurel Ave, Bajada, Davao City", "commercial", 28000000m),
                ("Lot 45, Nuvali Heights, Santa Rosa, Laguna", "lot", 6800000m),
                ("Residential Lot 12, Anvaya Cove, Morong, Bataan", "lot", 9200000m),
                ("Industrial Lot 8, Light Industry & Science Park, Cabuyao, Laguna", "lot", 24000000m),
                ("Commercial Lot, Ortigas East, Pasig City", "commercial", 36000000m),
                ("Commercial Building 2A, Shaw Boulevard, Mandaluyong City", "commercial", 48000000m),
                ("Retail Unit 102, Eastwood Citywalk 2, Bagumbayan, Quezon City", "commercial", 14000000m),
                ("Commercial Office 504, Cebu Exchange, Salinas Drive, Cebu City", "commercial", 16500000m),
                ("Prime Commercial Lot, Lanang Premier Business Park, Davao City", "commercial", 32000000m),
                ("Commercial Warehouse 3, Silangan Industrial Estate, Canlubang, Laguna", "commercial", 29000000m),
                ("Agricultural Lot 4, Alfonso, Cavite near Tagaytay", "lot", 7500000m),
                ("Eco-Tourism Lot 9, Panglao Island, Bohol", "lot", 13500000m)
            };

            var rnd = new Random(202);
            var properties = new List<Property>();

            for (int i = 0; i < count; i++)
            {
                var template = propertyTemplates[i % propertyTemplates.Length];
                string address = i >= propertyTemplates.Length
                    ? $"{template.Address} - Phase {i / propertyTemplates.Length + 1}"
                    : template.Address;

                var owner = customers[rnd.Next(customers.Count)];
                var agent = agents[rnd.Next(agents.Count)];

                var prop = new Property
                {
                    Address = address,
                    PropertyType = template.Type,
                    Price = template.Price,
                    Status = "available",
                    AssignmentStatus = "approved",
                    OwnerCustomerId = owner.CustomerId,
                    ListedByAgentId = agent.UserId,
                    CreatedByUserId = agent.UserId,
                    CreatedAt = DateTime.UtcNow.AddMonths(-rnd.Next(1, 18))
                };

                properties.Add(prop);
            }

            return properties;
        }

        private static List<Deal> GenerateDealTransactions(
            int count,
            List<Customer> customers,
            List<Property> properties,
            List<User> agents,
            int fallbackUserId)
        {
            var stipulationsPool = new[]
            {
                "Subject to standard bank loan appraisal and formal credit approval within 30 days.",
                "Includes 1 designated basement parking slot and existing built-in kitchen cabinetry.",
                "Seller warrants settlement of all capital gains tax and real property taxes through closing date.",
                "Property conveyed in as-is-where-is condition with complete architectural turnover clearance.",
                "Move-in and key turnover immediately upon full release of clear bank loan proceeds.",
                "Includes transfer of active country club share and homeowner association membership privileges.",
                "Reservation fee is strictly credited toward the mandatory down payment schedule.",
                "Buyer assumes responsibility for documentary stamp tax, local transfer tax, and title registration fees.",
                "Turnover guaranteed within 45 calendar days following receipt of final contract signatures.",
                "Seller guarantees clear, unencumbered title free from any adverse liens or lis pendens."
            };

            var commissionRates = new[] { 0.03m, 0.035m, 0.04m, 0.045m, 0.05m };
            var paymentSchemes = new[] { "Bank Financing", "Spot Cash", "Deferred In-House" };
            var downPaymentPercents = new[] { 10m, 20m, 30m };
            var reservationFees = new[] { 25000m, 50000m, 75000m, 100000m, 150000m };

            var rnd = new Random(303);
            var deals = new List<Deal>(count);

            // Spread transactions across 18 months: March 2025 - September 2026
            DateTime now = DateTime.UtcNow;
            DateTime startDate = now.AddMonths(-18);

            for (int i = 0; i < count; i++)
            {
                // Calculate historical month offset (0 to 18)
                int monthOffset = (i * 18) / count;
                int dayOffset = rnd.Next(1, 28);
                int hour = rnd.Next(8, 19);
                int minute = rnd.Next(0, 60);

                DateTime dealDate = startDate.AddMonths(monthOffset).AddDays(dayOffset).AddHours(hour).AddMinutes(minute);
                if (dealDate > now)
                {
                    dealDate = now.AddDays(-rnd.Next(1, 5)).AddHours(-rnd.Next(1, 10));
                }

                var customer = customers[rnd.Next(customers.Count)];
                var property = properties[rnd.Next(properties.Count)];
                var agent = agents[rnd.Next(agents.Count)];

                // Determine stage based on age of the deal
                string stage;
                int roll = rnd.Next(100);
                if (monthOffset < 15) // Older than 3 months
                {
                    if (roll < 78) stage = "Closed";
                    else if (roll < 90) stage = "Lost";
                    else if (roll < 96) stage = "Contract";
                    else stage = "Reservation";
                }
                else // Recent 3 months: active pipeline
                {
                    if (roll < 22) stage = "Closed";
                    else if (roll < 48) stage = "Contract";
                    else if (roll < 72) stage = "Reservation";
                    else if (roll < 90) stage = "Offer";
                    else stage = "Lost";
                }

                // Negotiated deal value (around property valuation)
                decimal basePrice = property.Price > 100000m ? property.Price : 4500000m;
                double multiplier = 0.90 + (rnd.NextDouble() * 0.18); // 90% to 108% of asking price
                decimal dealValue = Math.Round((basePrice * (decimal)multiplier) / 50000m) * 50000m;

                decimal commissionRate = commissionRates[rnd.Next(commissionRates.Length)];
                string scheme = paymentSchemes[rnd.Next(paymentSchemes.Length)];
                decimal downPayment = scheme == "Spot Cash" ? 100m : downPaymentPercents[rnd.Next(downPaymentPercents.Length)];
                decimal reservationFee = reservationFees[rnd.Next(reservationFees.Length)];
                string stipulation = stipulationsPool[rnd.Next(stipulationsPool.Length)];

                DateTime? expectedClose = null;
                DateTime? contractSigned = null;

                if (stage == "Closed")
                {
                    int daysToClose = rnd.Next(18, 55);
                    DateTime signedDate = dealDate.AddDays(daysToClose);
                    if (signedDate > now) signedDate = now.AddDays(-1);
                    contractSigned = signedDate;
                    expectedClose = signedDate.AddDays(-rnd.Next(0, 7));
                }
                else if (stage == "Contract")
                {
                    expectedClose = dealDate.AddDays(rnd.Next(25, 75));
                }
                else if (stage == "Reservation")
                {
                    expectedClose = dealDate.AddDays(rnd.Next(40, 110));
                }
                else if (stage == "Offer")
                {
                    expectedClose = dealDate.AddDays(rnd.Next(30, 90));
                }
                else // Lost
                {
                    expectedClose = dealDate.AddDays(rnd.Next(14, 45));
                    stipulation = "Client retracted offer due to alternative commercial opportunities or mortgage contingency.";
                }

                var deal = new Deal
                {
                    CustomerId = customer.CustomerId,
                    PropertyId = property.PropertyId,
                    AgentId = agent.UserId,
                    CreatedByUserId = agent.UserId,
                    Value = dealValue,
                    CommissionRate = commissionRate,
                    Stage = stage,
                    PaymentScheme = scheme,
                    ReservationFee = reservationFee,
                    DownPaymentPercent = downPayment,
                    CgtPayer = rnd.Next(100) < 85 ? "Seller" : "50/50 Shared",
                    DstPayer = rnd.Next(100) < 85 ? "Buyer" : "50/50 Shared",
                    TransferTaxPayer = "Buyer",
                    RegistrationFeePayer = "Buyer",
                    SpecialStipulations = stipulation,
                    ExpectedCloseDate = expectedClose,
                    ContractSignedDate = contractSigned,
                    CreatedAt = dealDate
                };

                deals.Add(deal);
            }

            return deals;
        }

        /// <summary>
        /// Seeds realistic leads and support tickets across various stages, sources, categories, and dates.
        /// </summary>
        public static async Task SeedLeadsAndTicketsAsync(RealEstateDbContext db, int tenantId = 1)
        {
            var agents = await db.Users
                .Include(u => u.Role)
                .Include(u => u.Person)
                .Where(u => u.Status.ToLower() == "active")
                .ToListAsync();

            if (agents.Count == 0) return;

            var customers = await db.Customers
                .Include(c => c.Person)
                .ToListAsync();

            var rnd = new Random(42);

            // 1. Seed Leads if needed (< 35)
            int leadCount = await db.Leads.CountAsync();
            if (leadCount < 35)
            {
                var leadNames = new[]
                {
                    ("Ramon", "Valderama", "ramon.valderama@yahoo.com", "09171112233"),
                    ("Beatrice", "Tan", "beatrice.tan@gmail.com", "09182223344"),
                    ("Leandro", "Santos", "leandro.santos@outlook.com", "09203334455"),
                    ("Maricris", "Reyes", "maricris.reyes@gmail.com", "09224445566"),
                    ("Joshua", "Alcantara", "joshua.alcantara@yahoo.com", "09155556677"),
                    ("Catherine", "Lim", "catherine.lim@gmail.com", "09276667788"),
                    ("Gabriel", "De Jesus", "gabriel.dejesus@outlook.com", "09187778899"),
                    ("Michelle", "Soriano", "michelle.soriano@gmail.com", "09208889900"),
                    ("Antonio", "Chua", "antonio.chua@yahoo.com", "09229990011"),
                    ("Daphne", "Aquino", "daphne.aquino@gmail.com", "09170001122"),
                    ("Paolo", "Gutierrez", "paolo.gutierrez@outlook.com", "09181113355"),
                    ("Rowena", "Villanueva", "rowena.villanueva@gmail.com", "09202224466"),
                    ("Kenneth", "Gonzales", "kenneth.gonzales@yahoo.com", "09153335577"),
                    ("Clarisse", "Navarro", "clarisse.navarro@gmail.com", "09274446688"),
                    ("Eduardo", "Castillo", "eduardo.castillo@outlook.com", "09185557799"),
                    ("Krizza", "Bautista", "krizza.bautista@gmail.com", "09206668800"),
                    ("Victor", "Mendoza", "victor.mendoza@yahoo.com", "09227779911"),
                    ("Aileen", "Santiago", "aileen.santiago@gmail.com", "09178880022"),
                    ("Christian", "Tolentino", "christian.tolentino@outlook.com", "09189991133"),
                    ("Bernadette", "Ocampo", "bernadette.ocampo@gmail.com", "09200002244"),
                    ("Jerome", "Cruz", "jerome.cruz@yahoo.com", "09151113366"),
                    ("Stephanie", "Flores", "stephanie.flores@gmail.com", "09272224477"),
                    ("Rafael", "Mercado", "rafael.mercado@outlook.com", "09183335588"),
                    ("Giselle", "Pascual", "giselle.pascual@gmail.com", "09204446699"),
                    ("Lorenzo", "Aguilar", "lorenzo.aguilar@yahoo.com", "09225557700"),
                    ("Patricia", "Del Rosario", "patricia.delrosario@gmail.com", "09176668811"),
                    ("Francis", "Ramos", "francis.ramos@outlook.com", "09187779922"),
                    ("Diana", "Morales", "diana.morales@gmail.com", "09208880033"),
                    ("Manuel", "Vergara", "manuel.vergara@yahoo.com", "09159991144"),
                    ("Theresa", "Salazar", "theresa.salazar@gmail.com", "09270002255")
                };

                var sources = new[] { "Website", "Referral", "Walk-in", "Social Media", "Cold Call" };
                var stages = new[] { "new", "contacted", "qualified", "converted", "lost" };
                var priorities = new[] { "low", "medium", "high" };

                var leadsToAdd = new List<Lead>();
                for (int i = 0; i < leadNames.Length; i++)
                {
                    var (fn, ln, email, phone) = leadNames[i];
                    var person = new Person
                    {
                        FirstName = fn,
                        LastName = ln,
                        Email = email,
                        Phone = phone
                    };

                    var agent = agents[rnd.Next(agents.Count)];
                    int daysAgo = rnd.Next(10, 450);
                    var created = DateTime.UtcNow.AddDays(-daysAgo);

                    var lead = new Lead
                    {
                        Person = person,
                        Source = sources[rnd.Next(sources.Length)],
                        Stage = stages[rnd.Next(stages.Length)],
                        Priority = priorities[rnd.Next(priorities.Length)],
                        ExpectedValue = rnd.Next(30, 200) * 100000m,
                        AssignedAgentId = agent.UserId,
                        CreatedByUserId = agent.UserId,
                        AssignmentStatus = "approved",
                        CreatedAt = created
                    };

                    leadsToAdd.Add(lead);
                }

                db.Leads.AddRange(leadsToAdd);
                await db.SaveChangesAsync();
            }

            // 2. Seed Support Tickets if needed (< 25)
            int ticketCount = await db.SupportTickets.CountAsync();
            if (ticketCount < 25 && customers.Count > 0)
            {
                var ticketTemplates = new[]
                {
                    ("Billing", "Inquiry on Capital Gains Tax and Documentary Stamp Tax payment computation and schedule", "Medium", "Resolved", 3, 5),
                    ("Billing", "Request for official receipt and updated statement of account for reservation fee", "Low", "Resolved", 2, 2),
                    ("Contract Inquiry", "Clarification regarding Contract to Sell clause 14 amortization schedule", "High", "In Progress", 7, 0),
                    ("Property Inspection", "Punchlisting and ocular inspection request prior to unit turnover", "Medium", "Resolved", 4, 3),
                    ("Title Transfer", "Status update inquiry for Transfer Certificate of Title release at Registry of Deeds", "High", "In Progress", 14, 0),
                    ("Documentation", "Correction needed for buyer middle name spelling in formal Deed of Absolute Sale", "Critical", "Resolved", 2, 1),
                    ("Maintenance", "Minor drywall hairline crack inspection request during turnover warranty", "Low", "Resolved", 10, 8),
                    ("Billing", "Bank financing letter of guarantee verification for developer release", "High", "Open", 5, 0),
                    ("Contract Inquiry", "Request for addendum on dedicated basement parking slot assignment", "Medium", "Open", 6, 0),
                    ("Title Transfer", "Tax Declaration transfer status follow-up with City Assessor Office", "High", "Resolved", 15, 12),
                    ("Property Inspection", "Follow-up re-inspection for bathroom fixtures after developer rectification", "Medium", "Resolved", 5, 4),
                    ("Documentation", "Request for certified true copies of Master Deed of Declaration of Restrictions", "Low", "Resolved", 7, 6),
                    ("Billing", "Early settlement discount computation for remaining developer financing balance", "Medium", "In Progress", 5, 0),
                    ("Contract Inquiry", "Client requesting extension on down payment installment due date", "Critical", "Resolved", 3, 4),
                    ("Maintenance", "Aircon water drainage tapping inspection request", "Low", "Resolved", 6, 7),
                    ("Documentation", "Authority to Inspect and Move-in Clearance release request", "High", "Resolved", 3, 2),
                    ("Property Inspection", "Electrical load testing and water pressure verification request", "Medium", "Open", 4, 0),
                    ("Title Transfer", "BIR Certificate Authorizing Registration (CAR) release inquiry", "Critical", "In Progress", 10, 0),
                    ("Billing", "Post-dated checks replacement request due to bank branch consolidation", "Medium", "Resolved", 4, 3),
                    ("Contract Inquiry", "Assignment of rights to family member request requirements", "High", "Resolved", 8, 6),
                    ("Documentation", "Homeowners Association membership briefing and car sticker application", "Low", "Resolved", 5, 4),
                    ("Maintenance", "Intercom unit repair and reception connection troubleshooting", "Low", "Open", 7, 0),
                    ("Billing", "Real Property Tax (Amilyar) tax clearance copy request for loan release", "Medium", "Resolved", 4, 3),
                    ("Documentation", "Occupancy Permit and Fire Safety Inspection Certificate verification", "High", "Resolved", 6, 5),
                    ("Property Inspection", "Water meter installation coordination with utility provider", "Medium", "In Progress", 5, 0)
                };

                var ticketsToAdd = new List<SupportTicket>();
                int tNum = 1001;

                foreach (var tpl in ticketTemplates)
                {
                    var cust = customers[rnd.Next(customers.Count)];
                    var agent = agents[rnd.Next(agents.Count)];
                    int daysAgo = rnd.Next(5, 300);
                    var createdAt = DateTime.UtcNow.AddDays(-daysAgo);
                    var dueDate = createdAt.AddDays(tpl.Item5);

                    DateTime? resolvedAt = null;
                    if (tpl.Item4 == "Resolved")
                    {
                        resolvedAt = createdAt.AddDays(tpl.Item6);
                    }

                    var ticket = new SupportTicket
                    {
                        TicketNumber = $"TICK-{tNum++}",
                        CustomerId = cust.CustomerId,
                        RaisedByUserId = agent.UserId,
                        AssignedToUserId = agent.UserId,
                        Category = tpl.Item1,
                        Description = tpl.Item2,
                        Priority = tpl.Item3,
                        Status = tpl.Item4,
                        DueDate = dueDate,
                        CreatedAt = createdAt,
                        FirstRespondedAt = createdAt.AddHours(rnd.Next(1, 12)),
                        ResolvedAt = resolvedAt
                    };

                    ticketsToAdd.Add(ticket);
                }

                db.SupportTickets.AddRange(ticketsToAdd);
                await db.SaveChangesAsync();
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.infrastructure.data;
using CRMS_Peguit.infrastructure.Seeding;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.Services;
using CRMS_Peguit.api;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// DATABASE CONNECTION
// ==========================================================

var masterConnection =
    Environment.GetEnvironmentVariable("CRMS_CONNECTION")
    ?? builder.Configuration.GetConnectionString("MasterCrms");

if (string.IsNullOrWhiteSpace(masterConnection))
{
    throw new InvalidOperationException(
        "Database connection string 'MasterCrms' was not found."
    );
}

// ==========================================================
// MASTER DATABASE
// ==========================================================

builder.Services.AddDbContext<MasterCrmsDbContext>(options =>
    options.UseSqlServer(masterConnection)
);

// ==========================================================
// HTTP CONTEXT / TENANT RESOLVER
// ==========================================================

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantResolver, HttpTenantResolver>();
builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

// ==========================================================
// TENANT DATABASE CONTEXT
// ==========================================================

builder.Services.AddScoped<RealEstateDbContext>(serviceProvider =>
{
    var tenantResolver =
        serviceProvider.GetRequiredService<ITenantResolver>();

    var tenantId =
        tenantResolver.GetTenantId();

    var options =
        new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseSqlServer(masterConnection)
            .Options;

    return new RealEstateDbContext(
        options,
        tenantId
    );
});

// ==========================================================
// CONTROLLERS / OPENAPI
// ==========================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// ==========================================================
// BUILD APPLICATION
// ==========================================================

var app = builder.Build();

// ==========================================================
// DATABASE SEEDING
// ==========================================================
//
// There is no HTTP request during application startup,
// therefore HttpTenantResolver cannot determine TenantId.
//
// Seed Tenant A (1), Tenant B (2), and Tenant C (3)
using (var scope = app.Services.CreateScope())
{
    var masterDb = scope.ServiceProvider.GetRequiredService<MasterCrmsDbContext>();

    var comp1 = await masterDb.Companies.FirstOrDefaultAsync(c => c.CompanyId == 1);
    if (comp1 == null)
    {
        comp1 = new Company
        {
            CompanyCode = "COMP001",
            CompanyName = "My First Real Estate CRM Company",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        masterDb.Companies.Add(comp1);
        await masterDb.SaveChangesAsync();
    }

    var compDb1 = await masterDb.CompanyDatabases.FirstOrDefaultAsync(cd => cd.CompanyId == comp1.CompanyId);
    if (compDb1 == null)
    {
        compDb1 = new CompanyDatabase
        {
            CompanyId = comp1.CompanyId,
            ServerName = "db66713.public.databaseasp.net",
            DatabaseName = "db66713",
            CredentialKey = "TenantA",
            IsActive = true
        };
        masterDb.CompanyDatabases.Add(compDb1);
        await masterDb.SaveChangesAsync();
    }
    else if (string.IsNullOrWhiteSpace(compDb1.CredentialKey) || compDb1.CredentialKey != "TenantA")
    {
        compDb1.CredentialKey = "TenantA";
        await masterDb.SaveChangesAsync();
    }

    var options =
        new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseSqlServer(masterConnection)
            .Options;

    foreach (var tenantId in new[] { 1, 2, 3 })
    {
        await using var db =
            new RealEstateDbContext(
                options,
                tenantId: tenantId
            );

        await DbSeeder.SeedTestUsersAsync(
            db,
            tenantId: tenantId
        );
    }
}

// ==========================================================
// DEVELOPMENT OPENAPI
// ==========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==========================================================
// MIDDLEWARE
// ==========================================================

app.UseHttpsRedirection();

app.UseAuthorization();

// ==========================================================
// CONTROLLERS
// ==========================================================

app.MapControllers();

// ==========================================================
// CREATE COMPANY ENDPOINT
// ==========================================================

app.MapPost(
    "/companies",
    async (
        Company company,
        MasterCrmsDbContext db) =>
    {
        db.Companies.Add(company);

        await db.SaveChangesAsync();

        return Results.Created(
            $"/companies/{company.CompanyId}",
            company
        );
    }
);

// ==========================================================
// MASTER DATABASE ENDPOINTS (LAB 4)
// ==========================================================

app.MapPost(
    "/devices",
    async (
        Device device,
        MasterCrmsDbContext db) =>
    {
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return Results.Created($"/devices/{device.DeviceId}", device);
    }
);

app.MapPost(
    "/company-databases",
    async (
        CompanyDatabase companyDatabase,
        MasterCrmsDbContext db) =>
    {
        db.CompanyDatabases.Add(companyDatabase);
        await db.SaveChangesAsync();
        return Results.Created(
            $"/company-databases/{companyDatabase.CompanyDatabaseId}",
            companyDatabase
        );
    }
);

// ==========================================================
// DYNAMIC TENANT DATABASE RESOLUTION ENDPOINTS (LAB 4)
// ==========================================================

app.MapGet(
    "/test-tenant/{companyId:int}",
    async (
        int companyId,
        ITenantDbContextFactory tenantFactory) =>
    {
        await using var tenantDb = await tenantFactory.CreateAsync(companyId);
        var customerCount = await tenantDb.Customers.CountAsync();
        var propertyCount = await tenantDb.Properties.CountAsync();
        return Results.Ok(new
        {
            companyId,
            customerCount,
            propertyCount
        });
    }
);

app.MapPost(
    "/tenant/{companyId:int}/customers",
    async (
        int companyId,
        Customer customer,
        ITenantDbContextFactory tenantFactory) =>
    {
        await using var tenantDb = await tenantFactory.CreateAsync(companyId);
        customer.TenantId = companyId;
        customer.CreatedAt = DateTime.UtcNow;
        tenantDb.Customers.Add(customer);
        await tenantDb.SaveChangesAsync();
        return Results.Created(
            $"/tenant/{companyId}/customers/{customer.CustomerId}",
            customer
        );
    }
);

app.MapGet(
    "/tenant/{companyId:int}/customers",
    async (
        int companyId,
        ITenantDbContextFactory tenantFactory) =>
    {
        await using var tenantDb = await tenantFactory.CreateAsync(companyId);
        var customers = await tenantDb.Customers
            .AsNoTracking()
            .OrderBy(x => x.CustomerId)
            .ToListAsync();
        return Results.Ok(customers);
    }
);

app.MapPost(
    "/tenant/{companyId:int}/properties",
    async (
        int companyId,
        Property property,
        ITenantDbContextFactory tenantFactory) =>
    {
        await using var tenantDb = await tenantFactory.CreateAsync(companyId);
        property.TenantId = companyId;
        tenantDb.Properties.Add(property);
        await tenantDb.SaveChangesAsync();
        return Results.Created(
            $"/tenant/{companyId}/properties/{property.PropertyId}",
            property
        );
    }
);

app.MapGet(
    "/tenant/{companyId:int}/properties",
    async (
        int companyId,
        ITenantDbContextFactory tenantFactory) =>
    {
        await using var tenantDb = await tenantFactory.CreateAsync(companyId);
        var properties = await tenantDb.Properties
            .AsNoTracking()
            .OrderBy(x => x.PropertyId)
            .ToListAsync();
        return Results.Ok(properties);
    }
);

// ==========================================================
// START API
// ==========================================================

app.Run();
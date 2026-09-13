-- Phase 4 Down: Re-add dropped columns

BEGIN TRANSACTION;

-- Re-add TenantId columns
IF COL_LENGTH(N'dbo.LoginSessions', N'TenantId') IS NULL
    ALTER TABLE dbo.LoginSessions ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Users', N'TenantId') IS NULL
    ALTER TABLE dbo.Users ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Activities', N'TenantId') IS NULL
    ALTER TABLE dbo.Activities ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.SupportTickets', N'TenantId') IS NULL
    ALTER TABLE dbo.SupportTickets ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.SystemSettings', N'TenantId') IS NULL
    ALTER TABLE dbo.SystemSettings ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.BackupLogs', N'TenantId') IS NULL
    ALTER TABLE dbo.BackupLogs ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Leads', N'TenantId') IS NULL
    ALTER TABLE dbo.Leads ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Customers', N'TenantId') IS NULL
    ALTER TABLE dbo.Customers ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.BuyerProfiles', N'TenantId') IS NULL
    ALTER TABLE dbo.BuyerProfiles ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Properties', N'TenantId') IS NULL
    ALTER TABLE dbo.Properties ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.Deals', N'TenantId') IS NULL
    ALTER TABLE dbo.Deals ADD [TenantId] INT NOT NULL DEFAULT 0;

IF COL_LENGTH(N'dbo.PropertyShowingDetails', N'TenantId') IS NULL
    ALTER TABLE dbo.PropertyShowingDetails ADD [TenantId] INT NOT NULL DEFAULT 0;

-- Re-populate TenantId from Roles
UPDATE u SET u.TenantId = r.TenantId FROM dbo.Users u INNER JOIN dbo.Roles r ON u.RoleId = r.RoleId;
UPDATE c SET c.TenantId = u.TenantId FROM dbo.Customers c INNER JOIN dbo.Users u ON c.CreatedByUserId = u.UserId;
UPDATE l SET l.TenantId = u.TenantId FROM dbo.Leads l INNER JOIN dbo.Users u ON l.CreatedByUserId = u.UserId;
UPDATE p SET p.TenantId = u.TenantId FROM dbo.Properties p INNER JOIN dbo.Users u ON p.CreatedByUserId = u.UserId;
UPDATE d SET d.TenantId = u.TenantId FROM dbo.Deals d INNER JOIN dbo.Users u ON d.CreatedByUserId = u.UserId;

-- Re-add Deal dropped columns
IF COL_LENGTH(N'dbo.Deals', N'DownPaymentAmount') IS NULL
    ALTER TABLE dbo.Deals ADD [DownPaymentAmount] DECIMAL(18,2) NULL;

IF COL_LENGTH(N'dbo.Deals', N'BalanceAmount') IS NULL
    ALTER TABLE dbo.Deals ADD [BalanceAmount] DECIMAL(18,2) NULL;

IF COL_LENGTH(N'dbo.Deals', N'ContingenciesJson') IS NULL
    ALTER TABLE dbo.Deals ADD [ContingenciesJson] NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.Deals', N'ApprovedClauseIds') IS NULL
-- Re-add personal columns to Users, Customers, Leads and re-populate from Persons
IF COL_LENGTH(N'dbo.Users', N'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD
        [FirstName] NVARCHAR(100) NULL,
        [MiddleName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [Suffix] NVARCHAR(20) NULL,
        [Email] NVARCHAR(255) NULL;

    UPDATE u SET
        u.FirstName = p.FirstName,
        u.MiddleName = p.MiddleName,
        u.LastName = p.LastName,
        u.Suffix = p.Suffix,
        u.Email = p.Email
    FROM dbo.Users u
    INNER JOIN dbo.Persons p ON u.PersonId = p.PersonId;
END

IF COL_LENGTH(N'dbo.Customers', N'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Customers ADD
        [FirstName] NVARCHAR(100) NULL,
        [MiddleName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [Suffix] NVARCHAR(20) NULL,
        [Email] NVARCHAR(255) NULL,
        [Phone] NVARCHAR(50) NULL;

    UPDATE c SET
        c.FirstName = p.FirstName,
        c.MiddleName = p.MiddleName,
        c.LastName = p.LastName,
        c.Suffix = p.Suffix,
        c.Email = p.Email,
        c.Phone = p.Phone
    FROM dbo.Customers c
    INNER JOIN dbo.Persons p ON c.PersonId = p.PersonId;
END

IF COL_LENGTH(N'dbo.Leads', N'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Leads ADD
        [FirstName] NVARCHAR(100) NULL,
        [MiddleName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NULL,
        [Suffix] NVARCHAR(20) NULL,
        [Email] NVARCHAR(255) NULL,
        [Phone] NVARCHAR(50) NULL;

    UPDATE l SET
        l.FirstName = p.FirstName,
        l.MiddleName = p.MiddleName,
        l.LastName = p.LastName,
        l.Suffix = p.Suffix,
        l.Email = p.Email,
        l.Phone = p.Phone
    FROM dbo.Leads l
    INNER JOIN dbo.Persons p ON l.PersonId = p.PersonId;
END

COMMIT TRANSACTION;

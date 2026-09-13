-- Phase 1 Up: Create New Tables and Link Columns for 3NF Normalization

-- 1. Create Persons table
IF OBJECT_ID(N'dbo.Persons', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Persons] (
        [PersonId] INT IDENTITY(1,1) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [MiddleName] NVARCHAR(100) NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Suffix] NVARCHAR(20) NULL,
        [Email] NVARCHAR(255) NULL,
        [Phone] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED ([PersonId] ASC)
    );
END;

-- 2. Create DealContingencies table (1NF split of ContingenciesJson)
IF OBJECT_ID(N'dbo.DealContingencies', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DealContingencies] (
        [DealContingencyId] INT IDENTITY(1,1) NOT NULL,
        [DealId] INT NOT NULL,
        [ContingencyName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [DueDate] DATETIME2 NULL,
        [IsSatisfied] BIT NOT NULL DEFAULT 0,
        [SatisfiedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_DealContingencies] PRIMARY KEY CLUSTERED ([DealContingencyId] ASC),
        CONSTRAINT [FK_DealContingencies_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [dbo].[Deals] ([DealId]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_DealContingencies_DealId] ON [dbo].[DealContingencies] ([DealId]);
END;

-- 3. Create DealClauses junction table (1NF split of ApprovedClauseIds)
IF OBJECT_ID(N'dbo.DealClauses', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DealClauses] (
        [DealClauseId] INT IDENTITY(1,1) NOT NULL,
        [DealId] INT NOT NULL,
        [ClauseId] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(200) NULL,
        [ClauseText] NVARCHAR(MAX) NULL,
        [IsApproved] BIT NOT NULL DEFAULT 1,
        [ApprovedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_DealClauses] PRIMARY KEY CLUSTERED ([DealClauseId] ASC),
        CONSTRAINT [FK_DealClauses_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [dbo].[Deals] ([DealId]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_DealClauses_DealId] ON [dbo].[DealClauses] ([DealId]);
END;

-- 4. Add nullable link columns to prepare for backfill
IF COL_LENGTH(N'dbo.Users', N'PersonId') IS NULL
    ALTER TABLE [dbo].[Users] ADD [PersonId] INT NULL;

IF COL_LENGTH(N'dbo.Customers', N'PersonId') IS NULL
    ALTER TABLE [dbo].[Customers] ADD [PersonId] INT NULL;

IF COL_LENGTH(N'dbo.Leads', N'PersonId') IS NULL
    ALTER TABLE [dbo].[Leads] ADD [PersonId] INT NULL;

IF COL_LENGTH(N'dbo.Deals', N'CreatedByUserId') IS NULL
    ALTER TABLE [dbo].[Deals] ADD [CreatedByUserId] INT NULL;

-- 5. Ensure CreatedByUserId column exists on Leads, Customers, Properties
IF COL_LENGTH(N'dbo.Leads', N'CreatedByUserId') IS NULL
    ALTER TABLE [dbo].[Leads] ADD [CreatedByUserId] INT NULL;

IF COL_LENGTH(N'dbo.Customers', N'CreatedByUserId') IS NULL
    ALTER TABLE [dbo].[Customers] ADD [CreatedByUserId] INT NULL;

IF COL_LENGTH(N'dbo.Properties', N'CreatedByUserId') IS NULL
    ALTER TABLE [dbo].[Properties] ADD [CreatedByUserId] INT NULL;

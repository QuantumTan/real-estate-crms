-- Phase 2 Down: Remove FK constraints and nullable conversions

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Deals_Users_CreatedByUserId')
    ALTER TABLE dbo.Deals DROP CONSTRAINT [FK_Deals_Users_CreatedByUserId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Properties_Users_CreatedByUserId')
    ALTER TABLE dbo.Properties DROP CONSTRAINT [FK_Properties_Users_CreatedByUserId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Leads_Users_CreatedByUserId')
    ALTER TABLE dbo.Leads DROP CONSTRAINT [FK_Leads_Users_CreatedByUserId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Customers_Users_CreatedByUserId')
    ALTER TABLE dbo.Customers DROP CONSTRAINT [FK_Customers_Users_CreatedByUserId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Leads_Persons_PersonId')
    ALTER TABLE dbo.Leads DROP CONSTRAINT [FK_Leads_Persons_PersonId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Customers_Persons_PersonId')
    ALTER TABLE dbo.Customers DROP CONSTRAINT [FK_Customers_Persons_PersonId];

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Persons_PersonId')
    ALTER TABLE dbo.Users DROP CONSTRAINT [FK_Users_Persons_PersonId];

ALTER TABLE dbo.Deals ALTER COLUMN CreatedByUserId INT NULL;
ALTER TABLE dbo.Properties ALTER COLUMN CreatedByUserId INT NULL;
ALTER TABLE dbo.Leads ALTER COLUMN CreatedByUserId INT NULL;
ALTER TABLE dbo.Customers ALTER COLUMN CreatedByUserId INT NULL;

ALTER TABLE dbo.Leads ALTER COLUMN PersonId INT NULL;
ALTER TABLE dbo.Customers ALTER COLUMN PersonId INT NULL;
ALTER TABLE dbo.Users ALTER COLUMN PersonId INT NULL;

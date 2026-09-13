-- Phase 1 Down: Rollback creation of new tables and link columns

IF OBJECT_ID(N'dbo.DealClauses', N'U') IS NOT NULL
    DROP TABLE [dbo].[DealClauses];

IF OBJECT_ID(N'dbo.DealContingencies', N'U') IS NOT NULL
    DROP TABLE [dbo].[DealContingencies];

IF COL_LENGTH(N'dbo.Deals', N'CreatedByUserId') IS NOT NULL
    ALTER TABLE [dbo].[Deals] DROP COLUMN [CreatedByUserId];

IF COL_LENGTH(N'dbo.Leads', N'PersonId') IS NOT NULL
    ALTER TABLE [dbo].[Leads] DROP COLUMN [PersonId];

IF COL_LENGTH(N'dbo.Customers', N'PersonId') IS NOT NULL
    ALTER TABLE [dbo].[Customers] DROP COLUMN [PersonId];

IF COL_LENGTH(N'dbo.Users', N'PersonId') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP COLUMN [PersonId];

IF OBJECT_ID(N'dbo.Persons', N'U') IS NOT NULL
    DROP TABLE [dbo].[Persons];

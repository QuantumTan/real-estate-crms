-- Phase 4 Up: Drop redundant calculated columns, non-atomic blobs, and transitive TenantId columns

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

-- 1. Drop stored calculated and non-atomic columns on Deals
IF COL_LENGTH(N'dbo.Deals', N'DownPaymentAmount') IS NOT NULL
    ALTER TABLE dbo.Deals DROP COLUMN [DownPaymentAmount];

IF COL_LENGTH(N'dbo.Deals', N'BalanceAmount') IS NOT NULL
    ALTER TABLE dbo.Deals DROP COLUMN [BalanceAmount];

IF COL_LENGTH(N'dbo.Deals', N'ContingenciesJson') IS NOT NULL
    ALTER TABLE dbo.Deals DROP COLUMN [ContingenciesJson];

IF COL_LENGTH(N'dbo.Deals', N'ApprovedClauseIds') IS NOT NULL
    ALTER TABLE dbo.Deals DROP COLUMN [ApprovedClauseIds];

-- 2. Helper procedure to safely drop default constraint and column
DECLARE @sql NVARCHAR(MAX);

-- Drop TenantId indexes first
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_TenantId_Email' AND object_id = OBJECT_ID('dbo.Users'))
    DROP INDEX [IX_Users_TenantId_Email] ON dbo.Users;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_TenantId_LastName_FirstName' AND object_id = OBJECT_ID('dbo.Customers'))
    DROP INDEX [IX_Customers_TenantId_LastName_FirstName] ON dbo.Customers;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_TenantId' AND object_id = OBJECT_ID('dbo.Customers'))
    DROP INDEX [IX_Customers_TenantId] ON dbo.Customers;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Leads_TenantId_LastName_FirstName' AND object_id = OBJECT_ID('dbo.Leads'))
    DROP INDEX [IX_Leads_TenantId_LastName_FirstName] ON dbo.Leads;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Leads_TenantId' AND object_id = OBJECT_ID('dbo.Leads'))
    DROP INDEX [IX_Leads_TenantId] ON dbo.Leads;

-- 3. Drop TenantId column from tables where tenant is transitively derived
DECLARE @DropTable NVARCHAR(100);
DECLARE @TableList TABLE (TableName NVARCHAR(100));

INSERT INTO @TableList (TableName) VALUES
    ('LoginSessions'),
    ('Users'),
    ('Activities'),
    ('SupportTickets'),
    ('SystemSettings'),
    ('BackupLogs'),
    ('Leads'),
    ('Customers'),
    ('BuyerProfiles'),
    ('Properties'),
    ('Deals'),
    ('PropertyShowingDetails');

DECLARE @ConstraintName NVARCHAR(200);
DECLARE TableCursor CURSOR FOR SELECT TableName FROM @TableList;
OPEN TableCursor;
FETCH NEXT FROM TableCursor INTO @DropTable;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH('dbo.' + @DropTable, 'TenantId') IS NOT NULL
    BEGIN
        SET @ConstraintName = NULL;
        SELECT @ConstraintName = d.name
        FROM sys.default_constraints d
        INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
        WHERE d.parent_object_id = OBJECT_ID('dbo.' + @DropTable) AND c.name = 'TenantId';

        IF @ConstraintName IS NOT NULL
        BEGIN
            SET @sql = 'ALTER TABLE dbo.[' + @DropTable + '] DROP CONSTRAINT [' + @ConstraintName + '];';
            EXEC sp_executesql @sql;
        END;

        SET @sql = 'ALTER TABLE dbo.[' + @DropTable + '] DROP COLUMN [TenantId];';
        EXEC sp_executesql @sql;
    END;

    FETCH NEXT FROM TableCursor INTO @DropTable;
END;

CLOSE TableCursor;
DEALLOCATE TableCursor;

-- 4. Drop redundant personal columns normalized into Persons table
DECLARE @RedundantCols TABLE (TableName NVARCHAR(100), ColumnName NVARCHAR(100));
INSERT INTO @RedundantCols (TableName, ColumnName) VALUES
    ('Users', 'FirstName'),
    ('Users', 'MiddleName'),
    ('Users', 'LastName'),
    ('Users', 'Suffix'),
    ('Users', 'Email'),
    ('Customers', 'FirstName'),
    ('Customers', 'MiddleName'),
    ('Customers', 'LastName'),
    ('Customers', 'Suffix'),
    ('Customers', 'Email'),
    ('Customers', 'Phone'),
    ('Leads', 'FirstName'),
    ('Leads', 'MiddleName'),
    ('Leads', 'LastName'),
    ('Leads', 'Suffix'),
    ('Leads', 'Email'),
    ('Leads', 'Phone');

DECLARE @TargetTable NVARCHAR(100), @TargetColumn NVARCHAR(100);
DECLARE @DefConstraint NVARCHAR(200);
DECLARE ColCursor CURSOR FOR SELECT TableName, ColumnName FROM @RedundantCols;
OPEN ColCursor;
FETCH NEXT FROM ColCursor INTO @TargetTable, @TargetColumn;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH('dbo.' + @TargetTable, @TargetColumn) IS NOT NULL
    BEGIN
        SET @DefConstraint = NULL;
        SELECT @DefConstraint = d.name
        FROM sys.default_constraints d
        INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
        WHERE d.parent_object_id = OBJECT_ID('dbo.' + @TargetTable) AND c.name = @TargetColumn;

        IF @DefConstraint IS NOT NULL
        BEGIN
            SET @sql = 'ALTER TABLE dbo.[' + @TargetTable + '] DROP CONSTRAINT [' + @DefConstraint + '];';
            EXEC sp_executesql @sql;
        END;

        SET @sql = 'ALTER TABLE dbo.[' + @TargetTable + '] DROP COLUMN [' + @TargetColumn + '];';
        EXEC sp_executesql @sql;
    END;

    FETCH NEXT FROM ColCursor INTO @TargetTable, @TargetColumn;
END;

CLOSE ColCursor;
DEALLOCATE ColCursor;

COMMIT TRANSACTION;

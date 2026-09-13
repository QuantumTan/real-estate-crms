-- Phase 2 Up: Backfill Data, Verify Integrity, and Enforce Constraints

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRANSACTION;

-- 1. Backfill Persons from Users
-- Insert Users that don't yet have a PersonId
DECLARE @UserCursor CURSOR;
DECLARE @UserId INT, @UserFN NVARCHAR(100), @UserMN NVARCHAR(100), @UserLN NVARCHAR(100), @UserSuf NVARCHAR(20), @UserEmail NVARCHAR(200);
DECLARE @NewPersonId INT;

SET @UserCursor = CURSOR FOR
    SELECT UserId, ISNULL(FirstName, ''), MiddleName, ISNULL(LastName, ''), Suffix, Email
    FROM dbo.Users
    WHERE PersonId IS NULL;

OPEN @UserCursor;
FETCH NEXT FROM @UserCursor INTO @UserId, @UserFN, @UserMN, @UserLN, @UserSuf, @UserEmail;

WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.Persons (FirstName, MiddleName, LastName, Suffix, Email, Phone, CreatedAt)
    VALUES (
        CASE WHEN LEN(LTRIM(RTRIM(@UserFN))) > 0 THEN @UserFN ELSE 'User' END,
        @UserMN,
        CASE WHEN LEN(LTRIM(RTRIM(@UserLN))) > 0 THEN @UserLN ELSE CAST(@UserId AS NVARCHAR(20)) END,
        @UserSuf,
        @UserEmail,
        NULL,
        GETUTCDATE()
    );
    SET @NewPersonId = SCOPE_IDENTITY();

    UPDATE dbo.Users SET PersonId = @NewPersonId WHERE UserId = @UserId;

    FETCH NEXT FROM @UserCursor INTO @UserId, @UserFN, @UserMN, @UserLN, @UserSuf, @UserEmail;
END;

CLOSE @UserCursor;
DEALLOCATE @UserCursor;

-- 2. Backfill Persons from Customers
DECLARE @CustCursor CURSOR;
DECLARE @CustId INT, @CustFN NVARCHAR(100), @CustMN NVARCHAR(100), @CustLN NVARCHAR(100), @CustSuf NVARCHAR(20), @CustEmail NVARCHAR(255), @CustPhone NVARCHAR(50);

SET @CustCursor = CURSOR FOR
    SELECT CustomerId, ISNULL(FirstName, ''), MiddleName, ISNULL(LastName, ''), Suffix, Email, Phone
    FROM dbo.Customers
    WHERE PersonId IS NULL;

OPEN @CustCursor;
FETCH NEXT FROM @CustCursor INTO @CustId, @CustFN, @CustMN, @CustLN, @CustSuf, @CustEmail, @CustPhone;

WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.Persons (FirstName, MiddleName, LastName, Suffix, Email, Phone, CreatedAt)
    VALUES (
        CASE WHEN LEN(LTRIM(RTRIM(@CustFN))) > 0 THEN @CustFN ELSE 'Customer' END,
        @CustMN,
        CASE WHEN LEN(LTRIM(RTRIM(@CustLN))) > 0 THEN @CustLN ELSE CAST(@CustId AS NVARCHAR(20)) END,
        @CustSuf,
        @CustEmail,
        @CustPhone,
        GETUTCDATE()
    );
    SET @NewPersonId = SCOPE_IDENTITY();

    UPDATE dbo.Customers SET PersonId = @NewPersonId WHERE CustomerId = @CustId;

    FETCH NEXT FROM @CustCursor INTO @CustId, @CustFN, @CustMN, @CustLN, @CustSuf, @CustEmail, @CustPhone;
END;

CLOSE @CustCursor;
DEALLOCATE @CustCursor;

-- 3. Backfill Persons from Leads
-- If converted to Customer, link to Customer's PersonId; otherwise create new Person
DECLARE @LeadCursor CURSOR;
DECLARE @LeadId INT, @LeadFN NVARCHAR(100), @LeadMN NVARCHAR(100), @LeadLN NVARCHAR(100), @LeadSuf NVARCHAR(20), @LeadEmail NVARCHAR(255), @LeadPhone NVARCHAR(50), @ConvertedCustId INT;

SET @LeadCursor = CURSOR FOR
    SELECT LeadId, ISNULL(FirstName, ''), MiddleName, ISNULL(LastName, ''), Suffix, Email, Phone, ConvertedCustomerId
    FROM dbo.Leads
    WHERE PersonId IS NULL;

OPEN @LeadCursor;
FETCH NEXT FROM @LeadCursor INTO @LeadId, @LeadFN, @LeadMN, @LeadLN, @LeadSuf, @LeadEmail, @LeadPhone, @ConvertedCustId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @NewPersonId = NULL;

    IF @ConvertedCustId IS NOT NULL
    BEGIN
        SELECT @NewPersonId = PersonId FROM dbo.Customers WHERE CustomerId = @ConvertedCustId;
    END;

    IF @NewPersonId IS NULL
    BEGIN
        INSERT INTO dbo.Persons (FirstName, MiddleName, LastName, Suffix, Email, Phone, CreatedAt)
        VALUES (
            CASE WHEN LEN(LTRIM(RTRIM(@LeadFN))) > 0 THEN @LeadFN ELSE 'Lead' END,
            @LeadMN,
            CASE WHEN LEN(LTRIM(RTRIM(@LeadLN))) > 0 THEN @LeadLN ELSE CAST(@LeadId AS NVARCHAR(20)) END,
            @LeadSuf,
            @LeadEmail,
            @LeadPhone,
            GETUTCDATE()
        );
        SET @NewPersonId = SCOPE_IDENTITY();
    END;

    UPDATE dbo.Leads SET PersonId = @NewPersonId WHERE LeadId = @LeadId;

    FETCH NEXT FROM @LeadCursor INTO @LeadId, @LeadFN, @LeadMN, @LeadLN, @LeadSuf, @LeadEmail, @LeadPhone, @ConvertedCustId;
END;

CLOSE @LeadCursor;
DEALLOCATE @LeadCursor;

-- 4. Backfill Deals.CreatedByUserId
UPDATE d
SET d.CreatedByUserId = COALESCE(d.AgentId, c.CreatedByUserId, 1)
FROM dbo.Deals d
LEFT JOIN dbo.Customers c ON d.CustomerId = c.CustomerId
WHERE d.CreatedByUserId IS NULL;

-- 5. Ensure CreatedByUserId is populated on Leads, Customers, Properties
UPDATE dbo.Leads SET CreatedByUserId = 1 WHERE CreatedByUserId IS NULL;
UPDATE dbo.Customers SET CreatedByUserId = 1 WHERE CreatedByUserId IS NULL;
UPDATE dbo.Properties SET CreatedByUserId = 1 WHERE CreatedByUserId IS NULL;

-- 6. Backfill DealContingencies from ContingenciesJson (if JSON populated)
INSERT INTO dbo.DealContingencies (DealId, ContingencyName, Description, DueDate, IsSatisfied, SatisfiedAt, CreatedAt)
SELECT
    d.DealId,
    j.[ContingencyName],
    j.[Description],
    TRY_CAST(j.[DueDate] AS DATETIME2),
    ISNULL(TRY_CAST(j.[IsSatisfied] AS BIT), 0),
    TRY_CAST(j.[SatisfiedAt] AS DATETIME2),
    GETUTCDATE()
FROM dbo.Deals d
CROSS APPLY OPENJSON(d.ContingenciesJson)
WITH (
    ContingencyName NVARCHAR(100) '$.ContingencyName',
    Description NVARCHAR(500) '$.Description',
    DueDate NVARCHAR(50) '$.DueDate',
    IsSatisfied NVARCHAR(10) '$.IsSatisfied',
    SatisfiedAt NVARCHAR(50) '$.SatisfiedAt'
) j
WHERE d.ContingenciesJson IS NOT NULL
  AND ISJSON(d.ContingenciesJson) = 1
  AND NOT EXISTS (
      SELECT 1 FROM dbo.DealContingencies dc WHERE dc.DealId = d.DealId AND dc.ContingencyName = j.ContingencyName
  );

-- 7. Backfill DealClauses from ApprovedClauseIds (comma delimited)
INSERT INTO dbo.DealClauses (DealId, ClauseId, Title, ClauseText, IsApproved, ApprovedAt, CreatedAt)
SELECT
    d.DealId,
    LTRIM(RTRIM(s.value)) AS ClauseId,
    CONCAT('Clause ', LTRIM(RTRIM(s.value))) AS Title,
    NULL AS ClauseText,
    1 AS IsApproved,
    GETUTCDATE() AS ApprovedAt,
    GETUTCDATE() AS CreatedAt
FROM dbo.Deals d
CROSS APPLY STRING_SPLIT(d.ApprovedClauseIds, ',') s
WHERE d.ApprovedClauseIds IS NOT NULL
  AND LEN(LTRIM(RTRIM(s.value))) > 0
  AND NOT EXISTS (
      SELECT 1 FROM dbo.DealClauses dc WHERE dc.DealId = d.DealId AND dc.ClauseId = LTRIM(RTRIM(s.value))
  );

-- 8. Verification checks before enforcing NOT NULL constraints
IF EXISTS (SELECT 1 FROM dbo.Users WHERE PersonId IS NULL)
    THROW 51001, 'Data integrity error: Users found with NULL PersonId.', 1;

IF EXISTS (SELECT 1 FROM dbo.Customers WHERE PersonId IS NULL)
    THROW 51002, 'Data integrity error: Customers found with NULL PersonId.', 1;

IF EXISTS (SELECT 1 FROM dbo.Leads WHERE PersonId IS NULL)
    THROW 51003, 'Data integrity error: Leads found with NULL PersonId.', 1;

IF EXISTS (SELECT 1 FROM dbo.Deals WHERE CreatedByUserId IS NULL)
    THROW 51004, 'Data integrity error: Deals found with NULL CreatedByUserId.', 1;

-- 9. Enforce NOT NULL and add Foreign Key Constraints
ALTER TABLE dbo.Users ALTER COLUMN PersonId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Persons_PersonId')
    ALTER TABLE dbo.Users ADD CONSTRAINT [FK_Users_Persons_PersonId] FOREIGN KEY (PersonId) REFERENCES dbo.Persons (PersonId);

ALTER TABLE dbo.Customers ALTER COLUMN PersonId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Customers_Persons_PersonId')
    ALTER TABLE dbo.Customers ADD CONSTRAINT [FK_Customers_Persons_PersonId] FOREIGN KEY (PersonId) REFERENCES dbo.Persons (PersonId);

ALTER TABLE dbo.Leads ALTER COLUMN PersonId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Leads_Persons_PersonId')
    ALTER TABLE dbo.Leads ADD CONSTRAINT [FK_Leads_Persons_PersonId] FOREIGN KEY (PersonId) REFERENCES dbo.Persons (PersonId);

ALTER TABLE dbo.Customers ALTER COLUMN CreatedByUserId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Customers_Users_CreatedByUserId')
    ALTER TABLE dbo.Customers ADD CONSTRAINT [FK_Customers_Users_CreatedByUserId] FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (UserId);

ALTER TABLE dbo.Leads ALTER COLUMN CreatedByUserId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Leads_Users_CreatedByUserId')
    ALTER TABLE dbo.Leads ADD CONSTRAINT [FK_Leads_Users_CreatedByUserId] FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (UserId);

ALTER TABLE dbo.Properties ALTER COLUMN CreatedByUserId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Properties_Users_CreatedByUserId')
    ALTER TABLE dbo.Properties ADD CONSTRAINT [FK_Properties_Users_CreatedByUserId] FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (UserId);

ALTER TABLE dbo.Deals ALTER COLUMN CreatedByUserId INT NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Deals_Users_CreatedByUserId')
    ALTER TABLE dbo.Deals ADD CONSTRAINT [FK_Deals_Users_CreatedByUserId] FOREIGN KEY (CreatedByUserId) REFERENCES dbo.Users (UserId);

-- 10. Performance Indexes on all FKs used for tenant traversal
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_PersonId')
    CREATE NONCLUSTERED INDEX [IX_Users_PersonId] ON dbo.Users (PersonId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_PersonId')
    CREATE NONCLUSTERED INDEX [IX_Customers_PersonId] ON dbo.Customers (PersonId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Customers_CreatedByUserId')
    CREATE NONCLUSTERED INDEX [IX_Customers_CreatedByUserId] ON dbo.Customers (CreatedByUserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Leads_PersonId')
    CREATE NONCLUSTERED INDEX [IX_Leads_PersonId] ON dbo.Leads (PersonId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Leads_CreatedByUserId')
    CREATE NONCLUSTERED INDEX [IX_Leads_CreatedByUserId] ON dbo.Leads (CreatedByUserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Properties_CreatedByUserId')
    CREATE NONCLUSTERED INDEX [IX_Properties_CreatedByUserId] ON dbo.Properties (CreatedByUserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Deals_CreatedByUserId')
    CREATE NONCLUSTERED INDEX [IX_Deals_CreatedByUserId] ON dbo.Deals (CreatedByUserId);

COMMIT TRANSACTION;

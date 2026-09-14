IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Persons] (
    [PersonId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [MiddleName] nvarchar(100) NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Suffix] nvarchar(20) NULL,
    [Email] nvarchar(255) NULL,
    [Phone] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY ([PersonId])
);

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [RoleName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [PersonId] int NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [RoleId] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION
);

CREATE TABLE [BackupLogs] (
    [BackupId] int NOT NULL IDENTITY,
    [PerformedByUserId] int NOT NULL,
    [BackupDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [FileLocation] nvarchar(500) NULL,
    CONSTRAINT [PK_BackupLogs] PRIMARY KEY ([BackupId]),
    CONSTRAINT [FK_BackupLogs_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [LoginSessions] (
    [SessionId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LoginAt] datetime2 NOT NULL,
    [LogoutAt] datetime2 NULL,
    [IpAddress] nvarchar(50) NULL,
    CONSTRAINT [PK_LoginSessions] PRIMARY KEY ([SessionId]),
    CONSTRAINT [FK_LoginSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [SystemSettings] (
    [SettingId] int NOT NULL IDENTITY,
    [SettingKey] nvarchar(200) NOT NULL,
    [SettingValue] nvarchar(2000) NOT NULL,
    [UpdatedByUserId] int NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SettingId]),
    CONSTRAINT [FK_SystemSettings_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_BackupLogs_PerformedByUserId] ON [BackupLogs] ([PerformedByUserId]);

CREATE INDEX [IX_LoginSessions_UserId] ON [LoginSessions] ([UserId]);

CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);

CREATE TABLE [Persons] (
    [PersonId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [MiddleName] nvarchar(100) NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Suffix] nvarchar(20) NULL,
    [Email] nvarchar(255) NULL,
    [Phone] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY ([PersonId])
);

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [TenantId] int NOT NULL,
    [RoleName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [PersonId] int NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [RoleId] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION
);

CREATE TABLE [BackupLogs] (
    [BackupId] int NOT NULL IDENTITY,
    [PerformedByUserId] int NOT NULL,
    [BackupDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [FileLocation] nvarchar(500) NULL,
    CONSTRAINT [PK_BackupLogs] PRIMARY KEY ([BackupId]),
    CONSTRAINT [FK_BackupLogs_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [LoginSessions] (
    [SessionId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LoginAt] datetime2 NOT NULL,
    [LogoutAt] datetime2 NULL,
    [IpAddress] nvarchar(50) NULL,
    CONSTRAINT [PK_LoginSessions] PRIMARY KEY ([SessionId]),
    CONSTRAINT [FK_LoginSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [SystemSettings] (
    [SettingId] int NOT NULL IDENTITY,
    [SettingKey] nvarchar(200) NOT NULL,
    [SettingValue] nvarchar(2000) NOT NULL,
    [UpdatedByUserId] int NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SettingId]),
    CONSTRAINT [FK_SystemSettings_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_BackupLogs_PerformedByUserId] ON [BackupLogs] ([PerformedByUserId]);

CREATE INDEX [IX_LoginSessions_UserId] ON [LoginSessions] ([UserId]);

CREATE UNIQUE INDEX [IX_SystemSettings_SettingKey] ON [SystemSettings] ([SettingKey]);

CREATE INDEX [IX_SystemSettings_UpdatedByUserId] ON [SystemSettings] ([UpdatedByUserId]);

CREATE INDEX [IX_Users_PersonId] ON [Users] ([PersonId]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE TABLE [Customers] (
    [CustomerId] int NOT NULL IDENTITY,
    [PersonId] int NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedByUserId] int NOT NULL,
    [AssignedAgentId] int NULL,
    [AssignmentStatus] nvarchar(50) NOT NULL,
    [AssignmentReviewedByUserId] int NULL,
    [AssignmentReviewedAt] datetime2 NULL,
    [AssignmentReviewNotes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId]),
    CONSTRAINT [FK_Customers_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Customers_Users_AssignedAgentId] FOREIGN KEY ([AssignedAgentId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Customers_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [BuyerProfiles] (
    [CustomerId] int NOT NULL,
    [Budget] decimal(18,2) NOT NULL,
    [PreferredLocation] nvarchar(200) NULL,
    [PreferredPropertyType] nvarchar(100) NULL,
    CONSTRAINT [PK_BuyerProfiles] PRIMARY KEY ([CustomerId]),
    CONSTRAINT [FK_BuyerProfiles_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
);

CREATE TABLE [Leads] (
    [LeadId] int NOT NULL IDENTITY,
    [PersonId] int NOT NULL,
    [Source] nvarchar(100) NULL,
    [Notes] nvarchar(2000) NULL,
    [Stage] nvarchar(50) NOT NULL,
    [Priority] nvarchar(20) NULL,
    [ExpectedValue] decimal(18,2) NULL,
    [CreatedByUserId] int NOT NULL,
    [AssignedAgentId] int NULL,
    [AssignmentStatus] nvarchar(50) NOT NULL,
    [AssignmentReviewedByUserId] int NULL,
    [AssignmentReviewedAt] datetime2 NULL,
    [AssignmentReviewNotes] nvarchar(1000) NULL,
    [ConvertedCustomerId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Leads] PRIMARY KEY ([LeadId]),
    CONSTRAINT [FK_Leads_Customers_ConvertedCustomerId] FOREIGN KEY ([ConvertedCustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Leads_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([PersonId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Leads_Users_AssignedAgentId] FOREIGN KEY ([AssignedAgentId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Leads_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Customers_AssignedAgentId] ON [Customers] ([AssignedAgentId]);

CREATE INDEX [IX_Customers_CreatedByUserId] ON [Customers] ([CreatedByUserId]);

CREATE INDEX [IX_Customers_IsDeleted] ON [Customers] ([IsDeleted]);

CREATE INDEX [IX_Customers_PersonId] ON [Customers] ([PersonId]);

CREATE INDEX [IX_Leads_AssignedAgentId] ON [Leads] ([AssignedAgentId]);

CREATE UNIQUE INDEX [IX_Leads_ConvertedCustomerId] ON [Leads] ([ConvertedCustomerId]) WHERE [ConvertedCustomerId] IS NOT NULL;

CREATE INDEX [IX_Leads_CreatedByUserId] ON [Leads] ([CreatedByUserId]);

CREATE INDEX [IX_Leads_IsDeleted] ON [Leads] ([IsDeleted]);

CREATE INDEX [IX_Leads_PersonId] ON [Leads] ([PersonId]);

CREATE TABLE [Properties] (
    [PropertyId] int NOT NULL IDENTITY,
    [OwnerCustomerId] int NOT NULL,
    [CreatedByUserId] int NOT NULL,
    [ListedByAgentId] int NULL,
    [Address] nvarchar(500) NOT NULL,
    [PropertyType] nvarchar(100) NULL,
    [Price] decimal(18,2) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [AssignmentStatus] nvarchar(50) NOT NULL,
    [AssignmentReviewedByUserId] int NULL,
    [AssignmentReviewedAt] datetime2 NULL,
    [AssignmentReviewNotes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Properties] PRIMARY KEY ([PropertyId]),
    CONSTRAINT [FK_Properties_Customers_OwnerCustomerId] FOREIGN KEY ([OwnerCustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Properties_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Properties_Users_ListedByAgentId] FOREIGN KEY ([ListedByAgentId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Deals] (
    [DealId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [PropertyId] int NOT NULL,
    [AgentId] int NULL,
    [CreatedByUserId] int NOT NULL,
    [Value] decimal(18,2) NOT NULL,
    [CommissionRate] decimal(5,2) NOT NULL,
    [Stage] nvarchar(50) NOT NULL,
    [ExpectedCloseDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [PaymentScheme] nvarchar(50) NULL,
    [ReservationFee] decimal(18,2) NULL,
    [DownPaymentPercent] decimal(5,2) NULL,
    [CgtPayer] nvarchar(50) NULL,
    [DstPayer] nvarchar(50) NULL,
    [TransferTaxPayer] nvarchar(50) NULL,
    [RegistrationFeePayer] nvarchar(50) NULL,
    [SpecialStipulations] nvarchar(4000) NULL,
    [ContractSignedDate] datetime2 NULL,
    CONSTRAINT [PK_Deals] PRIMARY KEY ([DealId]),
    CONSTRAINT [FK_Deals_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Deals_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([PropertyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Deals_Users_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Deals_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [DealClauses] (
    [DealClauseId] int NOT NULL IDENTITY,
    [DealId] int NOT NULL,
    [ClauseId] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NULL,
    [ClauseText] nvarchar(max) NULL,
    [IsApproved] bit NOT NULL,
    [ApprovedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DealClauses] PRIMARY KEY ([DealClauseId]),
    CONSTRAINT [FK_DealClauses_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [Deals] ([DealId]) ON DELETE CASCADE
);

CREATE TABLE [DealContingencies] (
    [DealContingencyId] int NOT NULL IDENTITY,
    [DealId] int NOT NULL,
    [ContingencyName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [DueDate] datetime2 NULL,
    [IsSatisfied] bit NOT NULL,
    [SatisfiedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DealContingencies] PRIMARY KEY ([DealContingencyId]),
    CONSTRAINT [FK_DealContingencies_Deals_DealId] FOREIGN KEY ([DealId]) REFERENCES [Deals] ([DealId]) ON DELETE CASCADE
);

CREATE INDEX [IX_DealClauses_DealId] ON [DealClauses] ([DealId]);

CREATE INDEX [IX_DealContingencies_DealId] ON [DealContingencies] ([DealId]);

CREATE INDEX [IX_Deals_AgentId] ON [Deals] ([AgentId]);

CREATE INDEX [IX_Deals_CreatedByUserId] ON [Deals] ([CreatedByUserId]);

CREATE INDEX [IX_Deals_CustomerId] ON [Deals] ([CustomerId]);

CREATE INDEX [IX_Deals_PropertyId] ON [Deals] ([PropertyId]);

CREATE INDEX [IX_Properties_CreatedByUserId] ON [Properties] ([CreatedByUserId]);

CREATE INDEX [IX_Properties_ListedByAgentId] ON [Properties] ([ListedByAgentId]);

CREATE INDEX [IX_Properties_OwnerCustomerId] ON [Properties] ([OwnerCustomerId]);

CREATE TABLE [SupportTickets] (
    [TicketId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [RaisedByUserId] int NOT NULL,
    [AssignedToUserId] int NULL,
    [Description] nvarchar(2000) NOT NULL,
    [Priority] nvarchar(20) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ResolvedAt] datetime2 NULL,
    CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([TicketId]),
    CONSTRAINT [FK_SupportTickets_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
    CONSTRAINT [FK_SupportTickets_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SupportTickets_Users_RaisedByUserId] FOREIGN KEY ([RaisedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Activities] (
    [ActivityId] int NOT NULL IDENTITY,
    [Type] nvarchar(100) NOT NULL,
    [RelatedLeadId] int NULL,
    [RelatedCustomerId] int NULL,
    [LoggedByAgentId] int NOT NULL,
    [Notes] nvarchar(2000) NULL,
    [ActivityDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Activities] PRIMARY KEY ([ActivityId]),
    CONSTRAINT [FK_Activities_Customers_RelatedCustomerId] FOREIGN KEY ([RelatedCustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Activities_Leads_RelatedLeadId] FOREIGN KEY ([RelatedLeadId]) REFERENCES [Leads] ([LeadId]) ON DELETE SET NULL,
    CONSTRAINT [FK_Activities_Users_LoggedByAgentId] FOREIGN KEY ([LoggedByAgentId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [PropertyShowingDetails] (
    [ShowingDetailId] int NOT NULL IDENTITY,
    [ActivityId] int NOT NULL,
    [PropertyId] int NOT NULL,
    [ScheduledDate] datetime2 NULL,
    [FeedbackNotes] nvarchar(2000) NULL,
    CONSTRAINT [PK_PropertyShowingDetails] PRIMARY KEY ([ShowingDetailId]),
    CONSTRAINT [FK_PropertyShowingDetails_Activities_ActivityId] FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([ActivityId]) ON DELETE CASCADE,
    CONSTRAINT [FK_PropertyShowingDetails_Properties_PropertyId] FOREIGN KEY ([PropertyId]) REFERENCES [Properties] ([PropertyId]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Activities_LoggedByAgentId] ON [Activities] ([LoggedByAgentId]);

CREATE INDEX [IX_Activities_RelatedCustomerId] ON [Activities] ([RelatedCustomerId]);

CREATE INDEX [IX_Activities_RelatedLeadId] ON [Activities] ([RelatedLeadId]);

CREATE UNIQUE INDEX [IX_PropertyShowingDetails_ActivityId] ON [PropertyShowingDetails] ([ActivityId]);

CREATE INDEX [IX_PropertyShowingDetails_PropertyId] ON [PropertyShowingDetails] ([PropertyId]);

CREATE INDEX [IX_SupportTickets_AssignedToUserId] ON [SupportTickets] ([AssignedToUserId]);

CREATE INDEX [IX_SupportTickets_CustomerId] ON [SupportTickets] ([CustomerId]);

CREATE INDEX [IX_SupportTickets_RaisedByUserId] ON [SupportTickets] ([RaisedByUserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260914024312_InitialTenantSchema_3NF', N'10.0.11');

COMMIT;
GO


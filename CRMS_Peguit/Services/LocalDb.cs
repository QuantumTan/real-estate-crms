using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.winforms.Models.Services
{
    public static class LocalDb
    {
        private static bool _dealSchemaChecked = false;
        private static readonly object _lockObj = new();

        public static string ConnectionString =>
            DbConfiguration.GetLocalConnectionString();

        public static RealEstateDbContext CreateContext(int tenantId = 1)
        {
            LocalDbHelper.EnsureLocalDbRunning(ConnectionString);

            var options = new DbContextOptionsBuilder<RealEstateDbContext>()
                .UseSqlServer(ConnectionString, sql => sql.EnableRetryOnFailure())
                .Options;

            var context = new RealEstateDbContext(options, tenantId: tenantId);

            if (!_dealSchemaChecked)
            {
                lock (_lockObj)
                {
                    if (!_dealSchemaChecked)
                    {
                        EnsureDealSchema(context);
                        EnsureSupportTicketSchema(context);
                        EnsureFollowUpSchema(context);
                        _dealSchemaChecked = true;
                    }
                }
            }

            return context;
        }

        private static void EnsureDealSchema(RealEstateDbContext context)
        {
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('Deals', 'U') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH('Deals', 'PaymentScheme') IS NULL
                        BEGIN
                            ALTER TABLE Deals ADD PaymentScheme NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD ReservationFee DECIMAL(18,2) NULL;
                            ALTER TABLE Deals ADD DownPaymentPercent DECIMAL(5,2) NULL;
                            ALTER TABLE Deals ADD CgtPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD DstPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD TransferTaxPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD RegistrationFeePayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD SpecialStipulations NVARCHAR(MAX) NULL;
                            ALTER TABLE Deals ADD ContractSignedDate DATETIME2 NULL;
                        END

                        IF COL_LENGTH('Deals', 'CreatedByUserId') IS NULL
                        BEGIN
                            ALTER TABLE Deals ADD CreatedByUserId INT NULL;
                            EXEC('UPDATE d SET d.CreatedByUserId = COALESCE(d.AgentId, c.CreatedByUserId, 1) FROM Deals d LEFT JOIN Customers c ON d.CustomerId = c.CustomerId WHERE d.CreatedByUserId IS NULL');
                            ALTER TABLE Deals ALTER COLUMN CreatedByUserId INT NOT NULL;
                        END

                        UPDATE Deals SET
                            CgtPayer = ISNULL(CgtPayer, 'Seller'),
                            DstPayer = ISNULL(DstPayer, 'Buyer'),
                            TransferTaxPayer = ISNULL(TransferTaxPayer, 'Buyer'),
                            RegistrationFeePayer = ISNULL(RegistrationFeePayer, 'Buyer'),
                            PaymentScheme = ISNULL(PaymentScheme, 'Bank Financing')
                        WHERE CgtPayer IS NULL OR PaymentScheme IS NULL;
                    END

                    IF OBJECT_ID('DealContingencies', 'U') IS NULL
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
                    END

                    IF OBJECT_ID('DealClauses', 'U') IS NULL
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
                    END
                ");
            }
            catch
            {
                // Silent fallback if server offline or already created
            }
        }

        private static void EnsureSupportTicketSchema(RealEstateDbContext context)
        {
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('SupportTickets', 'U') IS NOT NULL
                    BEGIN
                        IF COL_LENGTH('SupportTickets', 'TicketNumber') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD TicketNumber NVARCHAR(30) NULL;
                            EXEC('UPDATE SupportTickets SET TicketNumber = ''TCK-'' + RIGHT(''00000'' + CAST(TicketId AS VARCHAR(10)), 5) WHERE TicketNumber IS NULL');
                            ALTER TABLE SupportTickets ALTER COLUMN TicketNumber NVARCHAR(30) NOT NULL;
                        END

                        IF COL_LENGTH('SupportTickets', 'Category') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD Category NVARCHAR(50) NOT NULL CONSTRAINT DF_SupportTickets_Category DEFAULT 'Other';
                        END

                        IF COL_LENGTH('SupportTickets', 'DueDate') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD DueDate DATETIME2 NULL;
                        END

                        IF COL_LENGTH('SupportTickets', 'FirstRespondedAt') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD FirstRespondedAt DATETIME2 NULL;
                        END

                        IF COL_LENGTH('SupportTickets', 'IsDeleted') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD IsDeleted BIT NOT NULL CONSTRAINT DF_SupportTickets_IsDeleted DEFAULT 0;
                        END

                        IF COL_LENGTH('SupportTickets', 'DeletedAt') IS NULL
                        BEGIN
                            ALTER TABLE SupportTickets ADD DeletedAt DATETIME2 NULL;
                        END
                    END

                    IF OBJECT_ID('TicketComments', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[TicketComments] (
                            [TicketCommentId] INT IDENTITY(1,1) NOT NULL,
                            [TicketId] INT NOT NULL,
                            [AuthorUserId] INT NOT NULL,
                            [CommentText] NVARCHAR(2000) NOT NULL,
                            [CommentType] NVARCHAR(50) NOT NULL DEFAULT 'Comment',
                            [IsInternal] BIT NOT NULL DEFAULT 1,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            CONSTRAINT [PK_TicketComments] PRIMARY KEY CLUSTERED ([TicketCommentId] ASC),
                            CONSTRAINT [FK_TicketComments_SupportTickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [dbo].[SupportTickets] ([TicketId]) ON DELETE CASCADE,
                            CONSTRAINT [FK_TicketComments_Users_AuthorUserId] FOREIGN KEY ([AuthorUserId]) REFERENCES [dbo].[Users] ([UserId])
                        );
                    END
                ");
            }
            catch
            {
                // Silent fallback if server offline or already updated
            }
        }

        private static void EnsureFollowUpSchema(RealEstateDbContext context)
        {
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID('TaskReminders', 'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[TaskReminders] (
                            [TaskReminderId] INT IDENTITY(1,1) NOT NULL,
                            [Title] NVARCHAR(200) NOT NULL,
                            [DueDate] DATETIME2 NOT NULL,
                            [AssignedToUserId] INT NOT NULL,
                            [RelatedCustomerId] INT NULL,
                            [RelatedLeadId] INT NULL,
                            [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                            [Type] NVARCHAR(50) NOT NULL DEFAULT 'Call',
                            [Notes] NVARCHAR(2000) NULL,
                            [Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [UpdatedAt] DATETIME2 NULL,
                            [CompletedAt] DATETIME2 NULL,
                            [IsDeleted] BIT NOT NULL DEFAULT 0,
                            [DeletedAt] DATETIME2 NULL,
                            CONSTRAINT [PK_TaskReminders] PRIMARY KEY CLUSTERED ([TaskReminderId] ASC),
                            CONSTRAINT [FK_TaskReminders_Users_AssignedToUserId] FOREIGN KEY ([AssignedToUserId]) REFERENCES [dbo].[Users] ([UserId]),
                            CONSTRAINT [FK_TaskReminders_Customers_RelatedCustomerId] FOREIGN KEY ([RelatedCustomerId]) REFERENCES [dbo].[Customers] ([CustomerId]) ON DELETE SET NULL,
                            CONSTRAINT [FK_TaskReminders_Leads_RelatedLeadId] FOREIGN KEY ([RelatedLeadId]) REFERENCES [dbo].[Leads] ([LeadId]) ON DELETE SET NULL
                        );

                        CREATE INDEX [IX_TaskReminders_AssignedToUserId] ON [dbo].[TaskReminders] ([AssignedToUserId]);
                        CREATE INDEX [IX_TaskReminders_DueDate] ON [dbo].[TaskReminders] ([DueDate]);
                        CREATE INDEX [IX_TaskReminders_IsDeleted] ON [dbo].[TaskReminders] ([IsDeleted]);
                        CREATE INDEX [IX_TaskReminders_RelatedCustomerId] ON [dbo].[TaskReminders] ([RelatedCustomerId]);
                        CREATE INDEX [IX_TaskReminders_RelatedLeadId] ON [dbo].[TaskReminders] ([RelatedLeadId]);
                        CREATE INDEX [IX_TaskReminders_Status] ON [dbo].[TaskReminders] ([Status]);
                    END
                ");
            }
            catch
            {
                // Silent fallback if server offline or already created
            }
        }
    }
}
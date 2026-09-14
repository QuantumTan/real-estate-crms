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
    }
}
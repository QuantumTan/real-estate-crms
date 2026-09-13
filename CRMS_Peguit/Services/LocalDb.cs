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
                            ALTER TABLE Deals ADD DownPaymentAmount DECIMAL(18,2) NULL;
                            ALTER TABLE Deals ADD BalanceAmount DECIMAL(18,2) NULL;
                            ALTER TABLE Deals ADD CgtPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD DstPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD TransferTaxPayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD RegistrationFeePayer NVARCHAR(50) NULL;
                            ALTER TABLE Deals ADD ContingenciesJson NVARCHAR(MAX) NULL;
                            ALTER TABLE Deals ADD ApprovedClauseIds NVARCHAR(500) NULL;
                            ALTER TABLE Deals ADD SpecialStipulations NVARCHAR(MAX) NULL;
                            ALTER TABLE Deals ADD ContractSignedDate DATETIME2 NULL;
                        END

                        UPDATE Deals SET
                            CgtPayer = ISNULL(CgtPayer, 'Seller'),
                            DstPayer = ISNULL(DstPayer, 'Buyer'),
                            TransferTaxPayer = ISNULL(TransferTaxPayer, 'Buyer'),
                            RegistrationFeePayer = ISNULL(RegistrationFeePayer, 'Buyer'),
                            PaymentScheme = ISNULL(PaymentScheme, 'Bank Financing')
                        WHERE CgtPayer IS NULL OR PaymentScheme IS NULL;
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
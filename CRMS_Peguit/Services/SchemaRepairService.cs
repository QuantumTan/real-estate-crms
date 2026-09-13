using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CRMS_Peguit.winforms.Models.Services
{
    public static class SchemaRepairService
    {
        public static void EnsureCrmPolishColumns(DbContext db)
        {
            var connection = db.Database.GetDbConnection();
            bool shouldClose = connection.State != System.Data.ConnectionState.Open;

            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                EnsureLeadColumns((SqlConnection)connection);
                EnsureUserColumns((SqlConnection)connection);
                EnsurePropertyColumns((SqlConnection)connection);
                EnsureDealColumns((SqlConnection)connection);
                EnsureAssignmentColumns((SqlConnection)connection, "Leads");
                EnsureAssignmentColumns((SqlConnection)connection, "Customers");
                EnsureAssignmentColumns((SqlConnection)connection, "Properties");
            }
            finally
            {
                if (shouldClose)
                {
                    connection.Close();
                }
            }
        }

        private static void EnsureLeadColumns(SqlConnection connection)
        {
            ExecuteIfMissing(
                connection,
                "Leads",
                "ExpectedValue",
                "ALTER TABLE [Leads] ADD [ExpectedValue] decimal(18,2) NULL;");

            ExecuteIfMissing(
                connection,
                "Leads",
                "Notes",
                "ALTER TABLE [Leads] ADD [Notes] nvarchar(2000) NULL;");

            ExecuteIfMissing(
                connection,
                "Leads",
                "Priority",
                "ALTER TABLE [Leads] ADD [Priority] nvarchar(20) NULL;");
        }

        private static void EnsureDealColumns(SqlConnection connection)
        {
            ExecuteIfMissing(connection, "Deals", "PaymentScheme", "ALTER TABLE [Deals] ADD [PaymentScheme] nvarchar(50) NULL;");
            ExecuteIfMissing(connection, "Deals", "ReservationFee", "ALTER TABLE [Deals] ADD [ReservationFee] decimal(18,2) NULL;");
            ExecuteIfMissing(connection, "Deals", "DownPaymentPercent", "ALTER TABLE [Deals] ADD [DownPaymentPercent] decimal(5,2) NULL;");
            ExecuteIfMissing(connection, "Deals", "DownPaymentAmount", "ALTER TABLE [Deals] ADD [DownPaymentAmount] decimal(18,2) NULL;");
            ExecuteIfMissing(connection, "Deals", "BalanceAmount", "ALTER TABLE [Deals] ADD [BalanceAmount] decimal(18,2) NULL;");
            ExecuteIfMissing(connection, "Deals", "CgtPayer", "ALTER TABLE [Deals] ADD [CgtPayer] nvarchar(50) NULL;");
            ExecuteIfMissing(connection, "Deals", "DstPayer", "ALTER TABLE [Deals] ADD [DstPayer] nvarchar(50) NULL;");
            ExecuteIfMissing(connection, "Deals", "TransferTaxPayer", "ALTER TABLE [Deals] ADD [TransferTaxPayer] nvarchar(50) NULL;");
            ExecuteIfMissing(connection, "Deals", "RegistrationFeePayer", "ALTER TABLE [Deals] ADD [RegistrationFeePayer] nvarchar(50) NULL;");
            ExecuteIfMissing(connection, "Deals", "ContingenciesJson", "ALTER TABLE [Deals] ADD [ContingenciesJson] nvarchar(max) NULL;");
            ExecuteIfMissing(connection, "Deals", "ApprovedClauseIds", "ALTER TABLE [Deals] ADD [ApprovedClauseIds] nvarchar(500) NULL;");
            ExecuteIfMissing(connection, "Deals", "SpecialStipulations", "ALTER TABLE [Deals] ADD [SpecialStipulations] nvarchar(max) NULL;");
            ExecuteIfMissing(connection, "Deals", "ContractSignedDate", "ALTER TABLE [Deals] ADD [ContractSignedDate] datetime2 NULL;");
        }

        private static void EnsurePropertyColumns(SqlConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Properties') 
    AND name = 'ListedByAgentId' 
    AND is_nullable = 0
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Properties_ListedByAgentId' AND object_id = OBJECT_ID('dbo.Properties'))
        DROP INDEX [IX_Properties_ListedByAgentId] ON [dbo].[Properties];

    ALTER TABLE [dbo].[Properties] ALTER COLUMN [ListedByAgentId] int NULL;

    CREATE NONCLUSTERED INDEX [IX_Properties_ListedByAgentId] ON [dbo].[Properties] ([ListedByAgentId]);
END";
            command.ExecuteNonQuery();
        }

        private static void EnsureUserColumns(SqlConnection connection)
        {
            ExecuteIfMissing(
                connection,
                "Users",
                "FirstName",
                "ALTER TABLE [Users] ADD [FirstName] nvarchar(100) NOT NULL CONSTRAINT [DF_Users_FirstName] DEFAULT ('');");

            ExecuteIfMissing(
                connection,
                "Users",
                "MiddleName",
                "ALTER TABLE [Users] ADD [MiddleName] nvarchar(100) NULL;");

            ExecuteIfMissing(
                connection,
                "Users",
                "LastName",
                "ALTER TABLE [Users] ADD [LastName] nvarchar(100) NOT NULL CONSTRAINT [DF_Users_LastName] DEFAULT ('');");

            ExecuteIfMissing(
                connection,
                "Users",
                "Suffix",
                "ALTER TABLE [Users] ADD [Suffix] nvarchar(20) NULL;");

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'FullName' AND IS_NULLABLE = 'NO')
BEGIN
    ALTER TABLE [dbo].[Users] ALTER COLUMN [FullName] nvarchar(200) NULL;
END
UPDATE [Users] SET [FirstName] = CASE WHEN CHARINDEX('@', Email) > 0 THEN LEFT(Email, CHARINDEX('@', Email) - 1) ELSE Email END WHERE ([FirstName] IS NULL OR [FirstName] = '') AND ([LastName] IS NULL OR [LastName] = '');
";
            cmd.ExecuteNonQuery();
        }

        private static void EnsureAssignmentColumns(SqlConnection connection, string tableName)
        {
            ExecuteIfMissing(
                connection,
                tableName,
                "AssignmentStatus",
                $"ALTER TABLE [{tableName}] ADD [AssignmentStatus] nvarchar(50) NOT NULL CONSTRAINT [DF_{tableName}_AssignmentStatus] DEFAULT ('approved');");

            ExecuteIfMissing(
                connection,
                tableName,
                "AssignmentReviewedByUserId",
                $"ALTER TABLE [{tableName}] ADD [AssignmentReviewedByUserId] int NULL;");

            ExecuteIfMissing(
                connection,
                tableName,
                "AssignmentReviewedAt",
                $"ALTER TABLE [{tableName}] ADD [AssignmentReviewedAt] datetime2 NULL;");

            ExecuteIfMissing(
                connection,
                tableName,
                "AssignmentReviewNotes",
                $"ALTER TABLE [{tableName}] ADD [AssignmentReviewNotes] nvarchar(1000) NULL;");

            ExecuteIfMissing(
                connection,
                tableName,
                "CreatedByUserId",
                $"ALTER TABLE [{tableName}] ADD [CreatedByUserId] int NULL;");

            using var cmd = connection.CreateCommand();
            if (tableName == "Customers")
            {
                cmd.CommandText = "UPDATE c SET c.[CreatedByUserId] = ISNULL(a.[LoggedByAgentId], ISNULL(c.[AssignedAgentId], 1)) FROM [Customers] c OUTER APPLY (SELECT TOP 1 [LoggedByAgentId] FROM [Activities] WHERE [RelatedCustomerId] = c.[CustomerId] AND [Type] = 'Customer Created') a WHERE c.[CreatedByUserId] IS NULL;";
                cmd.ExecuteNonQuery();
            }
            else if (tableName == "Leads")
            {
                cmd.CommandText = "UPDATE l SET l.[CreatedByUserId] = ISNULL(a.[LoggedByAgentId], ISNULL(l.[AssignedAgentId], 1)) FROM [Leads] l OUTER APPLY (SELECT TOP 1 [LoggedByAgentId] FROM [Activities] WHERE [RelatedLeadId] = l.[LeadId] AND [Type] = 'Lead Created') a WHERE l.[CreatedByUserId] IS NULL;";
                cmd.ExecuteNonQuery();
            }
            else if (tableName == "Properties")
            {
                cmd.CommandText = "UPDATE p SET p.[CreatedByUserId] = ISNULL(p.[ListedByAgentId], 1) FROM [Properties] p WHERE p.[CreatedByUserId] IS NULL;";
                cmd.ExecuteNonQuery();
            }
        }

        private static void ExecuteIfMissing(
            SqlConnection connection,
            string tableName,
            string columnName,
            string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                $"IF COL_LENGTH('dbo.{tableName}', '{columnName}') IS NULL BEGIN {sql} END";
            command.ExecuteNonQuery();
        }
    }
}

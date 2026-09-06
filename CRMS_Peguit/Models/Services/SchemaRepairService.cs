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
            cmd.CommandText = "UPDATE [Users] SET [FirstName] = CASE WHEN CHARINDEX('@', Email) > 0 THEN LEFT(Email, CHARINDEX('@', Email) - 1) ELSE Email END WHERE ([FirstName] IS NULL OR [FirstName] = '') AND ([LastName] IS NULL OR [LastName] = '')";
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

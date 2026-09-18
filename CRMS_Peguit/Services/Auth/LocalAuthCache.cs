using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace CRMS_Peguit.winforms.Auth
{
    // Caches the last successful online login per (company, email), so a
    // user who has logged in before can still get into the app when
    // monsterASP/the API is unreachable.
    // Keyed by CompanyId + Email so one device can hold cached logins for
    // more than one tenant without them colliding.
    // NuGet: Install-Package Microsoft.Data.Sqlite
    public class LocalAuthCache
    {
        private readonly string _connectionString;

        public LocalAuthCache()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CRMS_Peguit", "local_cache.db"
            );
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            _connectionString = $"Data Source={dbPath}";
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            // Check if existing table has CompanyId column (old schema)
            var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('CachedLogin') WHERE name = 'CompanyId';";
            var oldColExists = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0) > 0;
            if (oldColExists)
            {
                // Drop legacy table so it can be recreated with Email alone as PK
                var dropCmd = conn.CreateCommand();
                dropCmd.CommandText = "DROP TABLE IF EXISTS CachedLogin;";
                dropCmd.ExecuteNonQuery();
            }

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS CachedLogin (
                    Email TEXT PRIMARY KEY,
                    TenantId INTEGER NOT NULL,
                    UserId INTEGER NOT NULL,
                    FullName TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    RoleName TEXT NOT NULL,
                    LastSyncedAt TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        // Called after every successful ONLINE login, so the cache stays fresh
        public void SaveSuccessfulLogin(int tenantId, int userId, string fullName, string email, string passwordHash, string roleName)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO CachedLogin (Email, TenantId, UserId, FullName, PasswordHash, RoleName, LastSyncedAt)
                VALUES ($email, $tenantId, $userId, $fullName, $hash, $role, $syncedAt)
                ON CONFLICT(Email) DO UPDATE SET
                    TenantId = excluded.TenantId,
                    UserId = excluded.UserId,
                    FullName = excluded.FullName,
                    PasswordHash = excluded.PasswordHash,
                    RoleName = excluded.RoleName,
                    LastSyncedAt = excluded.LastSyncedAt;";
            cmd.Parameters.AddWithValue("$email", email);
            cmd.Parameters.AddWithValue("$tenantId", tenantId);
            cmd.Parameters.AddWithValue("$userId", userId);
            cmd.Parameters.AddWithValue("$fullName", fullName);
            cmd.Parameters.AddWithValue("$hash", passwordHash);
            cmd.Parameters.AddWithValue("$role", roleName);
            cmd.Parameters.AddWithValue("$syncedAt", DateTime.UtcNow.ToString("O"));
            cmd.ExecuteNonQuery();
        }

        public CachedLoginRecord? TryGetCachedLogin(string email)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TenantId, UserId, FullName, PasswordHash, RoleName, LastSyncedAt
                                 FROM CachedLogin WHERE Email = $email;";
            cmd.Parameters.AddWithValue("$email", email);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new CachedLoginRecord(
                TenantId: reader.GetInt32(0),
                UserId: reader.GetInt32(1),
                FullName: reader.GetString(2),
                Email: email,
                PasswordHash: reader.GetString(3),
                RoleName: reader.GetString(4),
                LastSyncedAt: DateTime.Parse(reader.GetString(5))
            );
        }
    }

    public record CachedLoginRecord(
        int TenantId,
        int UserId,
        string FullName,
        string Email,
        string PasswordHash,
        string RoleName,
        DateTime LastSyncedAt
    );
}
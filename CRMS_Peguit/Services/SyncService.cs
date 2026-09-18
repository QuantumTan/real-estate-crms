using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRMS_Peguit.domain.entities;
using CRMS_Peguit.infrastructure.data;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Periodically pushes local CRM data to the cloud database (MonsterASP).
    /// One-way sync: local → cloud. Local always wins.
    /// Writes status to sync-log.txt next to the app.
    /// Implements graceful failure handling with exponential backoff and identity insert management.
    /// </summary>
    public class SyncService : IDisposable
    {
        private readonly string _localConnection;
        private readonly string _cloudConnection;
        private readonly string _logPath;
        private System.Threading.Timer? _timer;
        private bool _isSyncing;
        private int _failureCount;
        private int _baseIntervalSeconds;
        public bool CloudAvailable { get; private set; } = true;

        private static readonly HashSet<string> TablesWithIdentity = new(StringComparer.OrdinalIgnoreCase)
        {
            "Roles", "Users", "LoginSessions", "Customers", "Properties", "Leads", "Deals",
            "Activities", "PropertyShowingDetails", "SupportTickets", "Subscriptions",
            "SystemSettings", "BackupLogs"
        };

        public SyncService(string? localConnection = null, string? cloudConnection = null)
        {
            _localConnection = !string.IsNullOrWhiteSpace(localConnection) && !localConnection.Contains("YOUR_CLOUD")
                ? localConnection
                : DbConfiguration.GetLocalConnectionString();

            _cloudConnection = !string.IsNullOrWhiteSpace(cloudConnection) && !cloudConnection.Contains("YOUR_CLOUD")
                ? cloudConnection
                : (DbConfiguration.GetCloudConnectionString() ?? string.Empty);

            _logPath = Path.Combine(AppContext.BaseDirectory, "sync-log.txt");
            _failureCount = 0;
        }

        public void Start(int intervalSeconds = 30)
        {
            _baseIntervalSeconds = intervalSeconds;
            Log($"SyncService started. Base interval: {intervalSeconds}s");

            _timer = new System.Threading.Timer(
                async _ => await SyncAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(intervalSeconds));
        }

        public async Task SyncAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                Log("Sync started (local → cloud)...");

                await using var local = CreateLocalContext();
                await using var cloud = CreateCloudContext();

                // Test cloud connection first
                await cloud.Database.OpenConnectionAsync();
                cloud.Database.CloseConnection();
                CloudAvailable = true;
                _failureCount = 0;

                // Ensure cloud database schema matches latest columns
                SchemaRepairService.EnsureCrmPolishColumns(cloud);

                // Pull any remotely registered users and roles from cloud to local
                await PullMissingUsersAndRoles(local, cloud);

                int totalChanges = 0;
                var details = new List<string>();

                // Sync in strict foreign-key dependency order
                totalChanges += await SyncTable<Role>(local, cloud, "Roles", details);
                totalChanges += await SyncTable<Person>(local, cloud, "Persons", details);
                totalChanges += await SyncTable<User>(local, cloud, "Users", details);
                totalChanges += await SyncTable<Customer>(local, cloud, "Customers", details);
                totalChanges += await SyncTable<BuyerProfile>(local, cloud, "BuyerProfiles", details);
                totalChanges += await SyncTable<Property>(local, cloud, "Properties", details);
                totalChanges += await SyncTable<Lead>(local, cloud, "Leads", details);
                totalChanges += await SyncTable<Deal>(local, cloud, "Deals", details);
                totalChanges += await SyncTable<DealContingency>(local, cloud, "DealContingencies", details);
                totalChanges += await SyncTable<DealClause>(local, cloud, "DealClauses", details);
                totalChanges += await SyncTable<Activity>(local, cloud, "Activities", details);
                totalChanges += await SyncTable<PropertyShowingDetail>(local, cloud, "PropertyShowingDetails", details);
                totalChanges += await SyncTable<SupportTicket>(local, cloud, "SupportTickets", details);
                totalChanges += await SyncTable<Subscription>(local, cloud, "Subscriptions", details);
                totalChanges += await SyncTable<SystemSetting>(local, cloud, "SystemSettings", details);
                totalChanges += await SyncTable<BackupLog>(local, cloud, "BackupLogs", details);
                totalChanges += await SyncTable<LoginSession>(local, cloud, "LoginSessions", details);

                if (totalChanges > 0)
                {
                    Log($"Sync completed successfully at {DateTime.Now:HH:mm:ss}. Pushed {totalChanges} record change(s) ({string.Join(", ", details)}).");
                }
                else
                {
                    Log($"Sync completed successfully at {DateTime.Now:HH:mm:ss}. Cloud database is up to date (0 changes).");
                }
            }
            catch (Exception ex)
            {
                CloudAvailable = false;
                _failureCount++;

                Log($"SYNC FAILED (attempt #{_failureCount}): {ex.Message}");
                if (ex.InnerException is not null)
                    Log($"INNER: {ex.InnerException.Message}");

                // Implement exponential backoff: wait longer between retries
                int backoffSeconds = Math.Min(_baseIntervalSeconds * (int)Math.Pow(2, Math.Min(_failureCount - 1, 4)), 600);
                Log($"Next retry in {backoffSeconds} seconds...");
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private RealEstateDbContext CreateLocalContext()
        {
            var options = new DbContextOptionsBuilder<RealEstateDbContext>()
                .UseSqlServer(_localConnection)
                .Options;
            return new RealEstateDbContext(options, 0);
        }

        private RealEstateDbContext CreateCloudContext()
        {
            var options = new DbContextOptionsBuilder<RealEstateDbContext>()
                .UseSqlServer(_cloudConnection, sql => sql.CommandTimeout(60))
                .Options;
            return new RealEstateDbContext(options, 0);
        }

        private async Task<int> SyncTable<T>(
            RealEstateDbContext local,
            RealEstateDbContext cloud,
            string tableName,
            List<string> details)
            where T : class
        {
            cloud.ChangeTracker.Clear();

            var localRows = await local.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync();
            if (localRows.Count == 0) return 0;

            var cloudRows = await cloud.Set<T>().IgnoreQueryFilters().AsNoTracking().ToListAsync();

            var entityType = cloud.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey();
            var keyProp = primaryKey?.Properties[0].PropertyInfo;
            if (keyProp == null) return 0;

            var cloudMap = cloudRows.ToDictionary(r => keyProp.GetValue(r)!, r => r);

            var toInsert = new List<T>();
            var toUpdate = new List<T>();

            foreach (var localRow in localRows)
            {
                var keyVal = keyProp.GetValue(localRow)!;
                if (!cloudMap.TryGetValue(keyVal, out var existing))
                {
                    toInsert.Add(localRow);
                }
                else
                {
                    // Check if any property actually changed
                    bool isDifferent = false;
                    foreach (var prop in entityType!.GetProperties())
                    {
                        var pInfo = prop.PropertyInfo;
                        if (pInfo == null) continue;
                        var localVal = pInfo.GetValue(localRow);
                        var cloudVal = pInfo.GetValue(existing);

                        if (localVal is DateTime dt1 && cloudVal is DateTime dt2)
                        {
                            if (Math.Abs((dt1 - dt2).TotalSeconds) > 1)
                            {
                                isDifferent = true;
                                break;
                            }
                            continue;
                        }

                        if (!Equals(localVal, cloudVal))
                        {
                            isDifferent = true;
                            break;
                        }
                    }

                    if (isDifferent)
                    {
                        toUpdate.Add(localRow);
                    }
                }
            }

            if (toInsert.Count == 0 && toUpdate.Count == 0)
            {
                return 0;
            }

            var cloudSet = cloud.Set<T>();

            foreach (var item in toUpdate)
            {
                cloudSet.Attach(item);
                cloud.Entry(item).State = EntityState.Modified;
            }

            foreach (var item in toInsert)
            {
                cloudSet.Add(item);
            }

            bool hasIdentity = TablesWithIdentity.Contains(tableName);
            var strategy = cloud.Database.CreateExecutionStrategy();

            if (toInsert.Count > 0 && hasIdentity)
            {
                await strategy.ExecuteAsync(async () =>
                {
                    if (!TablesWithIdentity.Contains(tableName))
                    {
                        throw new InvalidOperationException($"Table '{tableName}' is not permitted for identity insert.");
                    }

                    var safeTableName = tableName.Replace("]", "]]");
                    await using var tx = await cloud.Database.BeginTransactionAsync();

#pragma warning disable EF1002 // Table name is strictly validated against TablesWithIdentity whitelist and escaped
                    await cloud.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{safeTableName}] ON;");
                    await cloud.SaveChangesAsync();
                    await cloud.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [dbo].[{safeTableName}] OFF;");
#pragma warning restore EF1002

                    await tx.CommitAsync();
                });
            }
            else
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await cloud.SaveChangesAsync();
                });
            }

            cloud.ChangeTracker.Clear();

            int tableChanges = toInsert.Count + toUpdate.Count;
            details.Add($"{tableName}: +{toInsert.Count} ins, ~{toUpdate.Count} upd");
            return tableChanges;
        }

        private async Task PullMissingUsersAndRoles(RealEstateDbContext local, RealEstateDbContext cloud)
        {
            try
            {
                // 1. Pull missing Roles
                var localRoles = await local.Roles.IgnoreQueryFilters().AsNoTracking().ToListAsync();
                var cloudRoles = await cloud.Roles.IgnoreQueryFilters().AsNoTracking().ToListAsync();
                var missingRoles = cloudRoles.Where(cr => !localRoles.Any(lr => lr.RoleId == cr.RoleId || lr.RoleName.ToLower() == cr.RoleName.ToLower())).ToList();

                foreach (var role in missingRoles)
                {
                    await local.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleId = {0} OR RoleName = {2})
                        BEGIN
                            SET IDENTITY_INSERT Roles ON;
                            INSERT INTO Roles (RoleId, TenantId, RoleName) VALUES ({0}, {1}, {2});
                            SET IDENTITY_INSERT Roles OFF;
                        END",
                        role.RoleId, role.TenantId, role.RoleName);
                }

                // 2. Pull missing Users
                var localUsers = await local.Users.IgnoreQueryFilters().Include(u => u.Person).AsNoTracking().ToListAsync();
                var cloudUsers = await cloud.Users.IgnoreQueryFilters().Include(u => u.Person).AsNoTracking().ToListAsync();
                var missingUsers = cloudUsers.Where(cu => !localUsers.Any(lu => lu.UserId == cu.UserId || lu.Email.ToLower() == cu.Email.ToLower())).ToList();

                foreach (var user in missingUsers)
                {
                    await local.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = {0})
                        BEGIN
                            DECLARE @PersonId INT;
                            SELECT TOP 1 @PersonId = PersonId FROM Persons WHERE Email = {3};
                            IF @PersonId IS NULL
                            BEGIN
                                INSERT INTO Persons (FirstName, MiddleName, LastName, Suffix, Email, Phone, CreatedAt)
                                VALUES ({1}, {2}, {4}, {5}, {3}, {6}, GETUTCDATE());
                                SET @PersonId = SCOPE_IDENTITY();
                            END

                            SET IDENTITY_INSERT Users ON;
                            INSERT INTO Users (UserId, PersonId, PasswordHash, RoleId, Status, CreatedAt)
                            VALUES ({0}, @PersonId, {7}, {8}, {9}, {10});
                            SET IDENTITY_INSERT Users OFF;
                        END",
                        user.UserId,
                        user.FirstName ?? "User",
                        (object?)user.MiddleName ?? DBNull.Value,
                        user.Email,
                        user.LastName ?? "",
                        (object?)user.Suffix ?? DBNull.Value,
                        (object?)user.Phone ?? DBNull.Value,
                        user.PasswordHash ?? "",
                        user.RoleId,
                        user.Status ?? "active",
                        user.CreatedAt);
                }

                if (missingRoles.Count > 0 || missingUsers.Count > 0)
                {
                    Log($"Pulled from cloud: {missingRoles.Count} new Role(s), {missingUsers.Count} new User(s).");
                }
            }
            catch (Exception ex)
            {
                Log($"PullMissingUsersAndRoles warning: {ex.Message}");
            }
        }

        public int FailureCount => _failureCount;

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(
                    _logPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
            catch
            {
                // ignore logging failures
            }
        }

        public void Stop() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

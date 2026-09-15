using CRMS_Peguit.infrastructure.Seeding;
using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms
{
    internal static class Program
    {
        private static SyncService? _syncService;

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ApplicationConfiguration.Initialize();

            var localConnection = DbConfiguration.GetLocalConnectionString();
            var cloudConnection = DbConfiguration.GetCloudConnectionString();

            // Automatically ensure SQL Server LocalDB instance is actively running before database access
            LocalDbHelper.EnsureLocalDbRunning(localConnection);

            // Propagate connection string to environment so all components share the resolved value
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CRMS_CONNECTION")))
            {
                Environment.SetEnvironmentVariable("CRMS_CONNECTION", localConnection);
            }
            if (!string.IsNullOrWhiteSpace(cloudConnection) &&
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CRMS_CLOUD_CONNECTION")))
            {
                Environment.SetEnvironmentVariable("CRMS_CLOUD_CONNECTION", cloudConnection);
            }

            if (args.Contains("--verify-assets"))
            {
                var logo = AppBrand.Logo;
                var icon = AppBrand.AppIcon;
                Console.WriteLine($"[VERIFY] Logo loaded: {logo != null}, Size: {logo?.Width}x{logo?.Height}, Icon loaded: {icon != null}");
                return;
            }

            if (args.Contains("--sync-once"))
            {
                if (!string.IsNullOrWhiteSpace(cloudConnection))
                {
                    using var sync = new SyncService(localConnection, cloudConnection);
                    sync.SyncAsync().GetAwaiter().GetResult();
                }
                return;
            }

            if (args.Contains("--init-db"))
            {
                using var startupDb = LocalDb.CreateContext();
                startupDb.Database.EnsureCreated();
                DbSeeder.SeedTestUsersAsync(startupDb, 1).GetAwaiter().GetResult();
                SchemaRepairService.EnsureCrmPolishColumns(startupDb);
                Console.WriteLine("CRMS_Local database initialized and seeded successfully.");
                return;
            }

            // ==================================================
            // ONE-TIME SCHEMA INITIALIZATION
            // ==================================================
            try
            {
                using var startupDb = LocalDb.CreateContext();
                startupDb.Database.EnsureCreated();
                DbSeeder.SeedTestUsersAsync(startupDb, 1).GetAwaiter().GetResult();
                SchemaRepairService.EnsureCrmPolishColumns(startupDb);
            }
            catch (Exception ex)
            {
                // Non-critical startup schema check
                System.Diagnostics.Debug.WriteLine($"Startup DB init error: {ex.Message}");
            }

            // ==================================================
            // START BACKGROUND SYNC SERVICE (IF CLOUD CONFIGURED)
            // ==================================================
            if (!string.IsNullOrWhiteSpace(cloudConnection))
            {
                _syncService = new SyncService(localConnection, cloudConnection);
                _syncService.Start(30);
            }

            // ==================================================
            // START LOGIN FORM
            // ==================================================

            using var loginForm = new LoginForm();

            Application.Run(loginForm);

            // ==================================================
            // CLEAN UP
            // ==================================================

            _syncService?.Dispose();
        }
    }
}
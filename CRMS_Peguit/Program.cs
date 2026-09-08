using CRMS_Peguit.winforms.Models.Services;

namespace CRMS_Peguit.winforms
{
    internal static class Program
    {
        private static SyncService? _syncService;

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            var localConnection = DbConfiguration.GetLocalConnectionString();
            var cloudConnection = DbConfiguration.GetCloudConnectionString();

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

            if (args.Contains("--sync-once"))
            {
                if (!string.IsNullOrWhiteSpace(cloudConnection))
                {
                    using var sync = new SyncService(localConnection, cloudConnection);
                    sync.SyncAsync().GetAwaiter().GetResult();
                }
                return;
            }

            // ==================================================
            // ONE-TIME SCHEMA INITIALIZATION
            // ==================================================
            try
            {
                using var startupDb = LocalDb.CreateContext();
                SchemaRepairService.EnsureCrmPolishColumns(startupDb);
            }
            catch
            {
                // Non-critical startup schema check
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
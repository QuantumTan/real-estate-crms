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

            if (args.Contains("--sync-once"))
            {
                var localConn = Environment.GetEnvironmentVariable("CRMS_CONNECTION") ??
                    "Server=(localdb)\\mssqllocaldb;Database=CRMS_Local;Trusted_Connection=True;TrustServerCertificate=True;";
                var cloudConn = Environment.GetEnvironmentVariable("CRMS_CLOUD_CONNECTION") ??
                    "Server=db66713.public.databaseasp.net;Database=db66713;User Id=db66713;Password=2Ni%Sz_9?J8m;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
                using var sync = new SyncService(localConn, cloudConn);
                sync.SyncAsync().GetAwaiter().GetResult();
                return;
            }

            // ==================================================
            // LOCAL DATABASE
            // ==================================================

            var localConnection =
                Environment.GetEnvironmentVariable("CRMS_CONNECTION") ??
                "Server=(localdb)\\mssqllocaldb;Database=CRMS_Local;Trusted_Connection=True;TrustServerCertificate=True;";

            // ==================================================
            // CLOUD DATABASE (MonsterASP)
            // ==================================================

            var cloudConnection =
                Environment.GetEnvironmentVariable("CRMS_CLOUD_CONNECTION") ??
                "Server=db66713.public.databaseasp.net;Database=db66713;User Id=db66713;Password=2Ni%Sz_9?J8m;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            // ==================================================
            // START SYNC SERVICE
            // ==================================================

            _syncService = new SyncService(
                localConnection,
                cloudConnection
            );

            _syncService.Start(30);

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
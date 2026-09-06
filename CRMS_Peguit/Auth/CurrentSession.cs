using CRMS_Peguit.Models.Backend;
using NEXA.Model;
using System;

namespace CRMS_Peguit.winforms.Auth
{
    public static class CurrentSession
    {
        // ==============================================
        // CURRENT USER INFORMATION
        // ==============================================

        public static int UserId
        {
            get;
            private set;
        }

        public static int TenantId
        {
            get;
            private set;
        }

        public static string? JwtToken
        {
            get;
            private set;
        }

        public static User? CurrentUser
        {
            get;
            private set;
        }

        public static bool IsOffline
        {
            get;
            private set;
        }

        // ==============================================
        // START SESSION
        // ==============================================

        public static void Start(
            int userId,
            int tenantId,
            string fullName,
            string email,
            string roleName,
            string? jwtToken,
            bool isOffline)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(
                    "A valid UserId is required.",
                    nameof(userId));
            }

            if (tenantId <= 0)
            {
                throw new ArgumentException(
                    "A valid TenantId is required.",
                    nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException(
                    "Full name is required.",
                    nameof(fullName));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(
                    "Email is required.",
                    nameof(email));
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(
                    "Role is required.",
                    nameof(roleName));
            }

            UserId = userId;

            TenantId = tenantId;

            JwtToken = jwtToken;

            IsOffline = isOffline;

            var role = roleName
                .Trim()
                .ToLowerInvariant();

            CurrentUser = role switch
            {
                "superadmin" or "super admin" or "super_admin" =>
                    new SuperAdmin(
                        fullName,
                        email
                    ),

                "admin" =>
                    new Admin(
                        fullName,
                        email
                    ),

                "manager" =>
                    new Manager(
                        fullName,
                        email
                    ),

                "agent" =>
                    new SalesStaff(
                        fullName,
                        email
                    ),

                _ =>
                    throw new InvalidOperationException(
                        $"Unknown role '{roleName}' - cannot build a session user."
                    )
            };
        }

        // ==============================================
        // CHECK MODULE ACCESS
        // ==============================================

        public static bool CanAccess(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return false;
            }

            return CurrentUser?
                .GetAccessibleModules()
                .Contains(moduleName)
                ?? false;
        }

        // ==============================================
        // SIGN OUT
        // ==============================================

        public static void SignOut()
        {
            UserId = 0;

            TenantId = 0;

            JwtToken = null;

            CurrentUser = null;

            IsOffline = false;
        }
    }
}

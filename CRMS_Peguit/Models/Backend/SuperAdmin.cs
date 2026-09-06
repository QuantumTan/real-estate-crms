using NEXA.Model;
using System.Collections.Generic;

namespace CRMS_Peguit.Models.Backend
{
    public class SuperAdmin : User
    {
        public SuperAdmin(
            string fullName,
            string email)
            : base(
                fullName,
                email,
                UserRole.SuperAdmin)
        {
        }

        public override List<string> GetAccessibleModules()
        {
            return new List<string>
            {
                "Administrators",
                "Roles",
                "SystemAccess",
                "SystemData",
                "Backup",
                "Settings",
                "Policies",
                "Subscription",

                "Dashboard",
                "Reports",
                "Customers",
                "Leads",
                "Properties",
                "Deals",
                "Activities",
                "SupportTickets"
            };
        }

        public override string GetDashboardType()
        {
            return "SuperAdminDashboard";
        }
    }
}

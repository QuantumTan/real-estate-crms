using System.Collections.Generic;

namespace CRMS_Peguit.winforms.Models.Roles
{
    // CHILD CLASS - Manager

    public class Manager : User
    {
        public Manager(
            string fullName,
            string email)
            : base(
                fullName,
                email,
                UserRole.Manager)
        {
        }

        public override List<string> GetAccessibleModules()
        {
            return new List<string>
            {
                "Dashboard",
                "Leads",
                "Customers",
                "Properties",
                "Deals",
                "Campaigns",
                "Activities",
                "Approvals",
                "SalesStaff",
                "Reports",
                "SupportTickets",
                "TeamPerformance",
                "CustomerAssignments",
                "PropertyAssignments",
                "StaffAssignments"
            };
        }

        public override string GetDashboardType()
        {
            return "ManagerDashboard";
        }
    }
}

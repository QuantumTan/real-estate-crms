using NEXA.Model;
using System.Collections.Generic;

namespace CRMS_Peguit.Models.Backend
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
                "Activities",
                "TasksReminders",
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

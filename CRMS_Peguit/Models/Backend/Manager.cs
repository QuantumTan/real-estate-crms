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

                // View Reports
                "Reports",

                // Oversight & Manage Support Tickets
                "SupportTickets",

                // Monitor Team Performance
                "TeamPerformance",

                // Review Customer, Property & Staff Assignments
                "Customers",
                "Properties",
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

using NEXA.Model;
using System.Collections.Generic;

namespace CRMS_Peguit.Models.Backend
{
    // CHILD CLASS - SalesStaff

    public class SalesStaff : User
    {
        public SalesStaff(
            string fullName,
            string email)
            : base(
                fullName,
                email,
                UserRole.SalesStaff)
        {
        }

        public override List<string> GetAccessibleModules()
        {
            return new List<string>
            {
                "Dashboard",
                "Reports",
                "Customers",
                "Leads",
                "Properties",
                "Deals",
                "Activities",
                "TasksReminders",
                "SupportTickets"
            };
        }

        public override string GetDashboardType()
        {
            return "SalesStaffDashboard";
        }
    }
}

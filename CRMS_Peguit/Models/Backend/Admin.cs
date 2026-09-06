using NEXA.Model;
using System.Collections.Generic;

namespace CRMS_Peguit.Models.Backend
{
    // CHILD CLASS - Admin

    public class Admin : User
    {
        public Admin(
            string fullName,
            string email)
            : base(
                fullName,
                email,
                UserRole.Admin)
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
                "Managers",
                "SalesStaff",
                "Reports",
                "SupportTickets"
            };
        }

        public override string GetDashboardType()
        {
            return "AdminDashboard";
        }
    }
}

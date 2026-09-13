using System.Collections.Generic;

namespace CRMS_Peguit.winforms.Models.Roles
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
                "Campaigns",
                "Reports",
                "SupportTickets",
                "Managers",
                "SalesStaff"
            };
        }

        public override string GetDashboardType()
        {
            return "AdminDashboard";
        }
    }
}

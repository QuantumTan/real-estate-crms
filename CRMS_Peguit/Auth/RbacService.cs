using NEXA.Model;

namespace CRMS_Peguit.winforms.Auth
{
    public static class RbacService
    {
        public static bool IsSuperAdmin =>
            CurrentSession.CurrentUser?.Role == UserRole.SuperAdmin;

        public static bool IsAdmin =>
            CurrentSession.CurrentUser?.Role == UserRole.Admin;

        public static bool IsManager =>
            CurrentSession.CurrentUser?.Role == UserRole.Manager;

        public static bool IsAgent =>
            CurrentSession.CurrentUser?.Role == UserRole.SalesStaff;

        public static bool CanApproveAssignments =>
            IsSuperAdmin || IsAdmin || IsManager;

        public static bool CanManageAllRecords =>
            IsSuperAdmin || IsAdmin || IsManager;

        public static bool CanUseModule(string moduleName) =>
            CurrentSession.CanAccess(moduleName);

        public static bool CanEditAssignedRecord(int? assignedAgentId)
        {
            if (CanManageAllRecords)
            {
                return true;
            }

            return IsAgent &&
                   (assignedAgentId is null || assignedAgentId == CurrentSession.UserId);
        }

        public static bool CanArchiveAssignedRecord(int? assignedAgentId) =>
            CanEditAssignedRecord(assignedAgentId);

        public static bool ShouldAutoAssignCreatedRecord =>
            IsAgent;
    }
}

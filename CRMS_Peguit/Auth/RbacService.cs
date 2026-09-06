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

        // R24. Only Manager or Admin may set or change ownership.
        public static bool CanAssignRecords =>
            IsManager || IsAdmin;

        public static bool CanApproveAssignments =>
            IsManager;

        public static bool CanEditAssignedRecord(int? assignedAgentId)
        {
            // R26. Manager and Admin bypass ownership restrictions entirely (oversight override)
            if (IsManager || IsAdmin || IsSuperAdmin)
                return true;

            // R25. Ownership governs Agent access — Agent may modify ONLY records where they are current owner.
            // Unassigned records (null assignedAgentId) are read-only for Agents until assigned by Manager.
            if (IsAgent)
                return assignedAgentId.HasValue && assignedAgentId.Value == CurrentSession.UserId;

            return false;
        }

        public static bool CanArchiveAssignedRecord(int? assignedAgentId) =>
            CanEditAssignedRecord(assignedAgentId);

        // R23. Default state is Unassigned — a record created by an Agent starts Unassigned.
        // It is NEVER auto-assigned to its creator.
        public static bool ShouldAutoAssignCreatedRecord =>
            false;
    }
}

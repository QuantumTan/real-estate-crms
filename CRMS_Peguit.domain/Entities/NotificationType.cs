namespace CRMS_Peguit.domain.entities
{
    public enum NotificationType
    {
        // Agent notifications
        LeadAssigned = 1,
        LeadStageChanged = 2,
        CustomerAssigned = 3,
        PropertyAssigned = 4,
        PropertyStatusChanged = 5,
        DealStageChanged = 6,
        FollowUpDueSoon = 7,
        FollowUpOverdue = 8,
        TicketAssigned = 9,
        TicketStatusChanged = 10,

        // Manager notifications
        LeadUnassigned = 20,
        CustomerUnassigned = 21,
        DealClosed = 22,
        TicketCreated = 23,

        // Admin notifications
        SubscriptionExpiring = 30,
        SubscriptionExpired = 31,
        BackupFailed = 32,

        // Super Admin notifications
        AdminAccountCreated = 40
    }
}

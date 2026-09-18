# NEXA CRM — Follow-Ups Module: Rules & Implementation Traceability

This document provides a complete traceability matrix of every architectural constraint, access control, and business rule enforced in the **Follow-Ups Module** for project defense and code audits.

---

## 1. Access Model & Role Gating (Agent-Only)

| Rule / Requirement | Enforcement Rationale | Implementation Location |
| :--- | :--- | :--- |
| **Strict Agent-Only Module Access** | Per NEXA's finalized use case diagram, ONLY the Agent role has a *"Manage Activities & Follow Ups"* use case. Manager, Admin, and Super Admin have NO use case or oversight screens. | [`Manager.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Models/Roles/Manager.cs) (removed `TasksReminders`), [`SalesStaff.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Models/Roles/SalesStaff.cs), [`MainForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/MainForm.cs#L632) |
| **Sidebar Navigation Gating** | The Follow-Ups sidebar button is visible only when the active session is both authorized for `TasksReminders` and authenticated as an Agent (`RbacService.IsAgent`). Hidden entirely for Manager, Admin, and Super Admin. | [`MainForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/MainForm.cs#L632) (`btnFollowUps.Visible = CurrentSession.CanAccess("TasksReminders") && RbacService.IsAgent;`) |
| **Route / Deep Navigation Gating** | Direct programmatic navigation via `NavigateTo("followups")` verifies Agent permissions before activating or switching the view. | [`MainForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/MainForm.cs#L698) |
| **Data Scope Isolation** | An Agent can only see their own personal tasks. Cross-agent follow-up browsing or team compliance grids do not exist. Queries are hard-scoped to `CurrentSession.UserId`. | [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L36) (`r.AssignedToUserId == currentUserId`) |

---

## 2. Data Ownership & Relationship Rules

| Rule / Requirement | Enforcement Rationale | Implementation Location |
| :--- | :--- | :--- |
| **Customer XOR Lead Exclusivity** | A Follow-Up relates to exactly one of Customer or Lead, never both. Linking both or neither is rejected at domain, controller, and UI levels. | [`TaskReminder.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.domain/Entities/TaskReminder.cs#L38-L47) (`LinkToCustomer`, `LinkToLead`), [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L280-L318), [`FollowUpInputForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpInputForm.cs#L180-L198) |
| **Agent Assignment Ownership** | New follow-ups cannot be assigned to another agent or left in a pool. They are automatically and strictly bound to the creating Agent (`CurrentSession.UserId`). Dropdowns only show Customers/Leads currently assigned to this Agent. | [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L104-L124), [`FollowUpInputForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpInputForm.cs#L100-L148) |
| **Reassignment Cascade** | If a Customer or Lead is reassigned by a Manager to a different Agent, all open (non-deleted, Pending/Overdue) Follow-Ups tied to that record automatically transfer to the new Agent. No orphaned tasks remain with previous assignees. | [`CustomerController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/CustomerController.cs#L136-L141) & [L240-L265](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/CustomerController.cs#L240-L265) (`TransferOpenFollowUps`), [`LeadController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/LeadController.cs#L138-L143) & [L272-L298](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/LeadController.cs#L272-L298) (`TransferOpenFollowUps`) |

---

## 3. Status Lifecycle & Workflow Rules

| Rule / Requirement | Enforcement Rationale | Implementation Location |
| :--- | :--- | :--- |
| **Computed Overdue Status** | Users cannot manually select "Overdue". The system automatically evaluates `DueDate < UtcNow` for `Pending` items and marks them `Overdue`. | [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L38-L55) (`GetAll`), [L120](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L120) (`Add`), [L150](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L150) (`Update`) |
| **Reschedule / Snooze Reset** | Rescheduling or snoozing a task pushes the `DueDate` forward into the future and immediately restores status from `Overdue` back to `Pending`. | [`TaskReminder.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.domain/Entities/TaskReminder.cs#L49-L61), [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L196-L224) (`Snooze`, `Reschedule`) |
| **Optional Activity Logging on Completion** | Completing a Follow-Up optionally generates a historical `Activity` log on the customer/lead timeline (matching HubSpot/Salesforce task completion patterns). This keeps `TaskReminder` (forward-looking task) and `Activity` (historical log) distinct. | [`CompleteFollowUpDialog.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/CompleteFollowUpDialog.cs#L88-L135), [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L162-L192) (`MarkComplete`) |
| **Soft Delete Enforcement** | Follow-Ups are never hard-deleted from the database. Soft delete sets `IsDeleted = true` and `DeletedAt = DateTime.UtcNow`. The global query filter excludes soft-deleted items automatically. | [`TaskReminder.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.domain/Entities/TaskReminder.cs#L29-L30), [`RealEstateDbContext.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.infrastructure/Data/RealEstateDbContext.cs#L422), [`FollowUpController.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Controllers/FollowUpController.cs#L226-L239) |

---

## 4. Industry-Standard Feature Implementation

| Feature | Description | Implementation Location |
| :--- | :--- | :--- |
| **Follow-Up Type / Channel** | Distinguishes channel (`Call`, `Email`, `Meeting`, `Text`) with visual icons (`📞`, `✉️`, `👥`, `💬`). | [`TaskReminder.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.domain/Entities/TaskReminder.cs#L20), [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs#L368-L382) |
| **Specific Date & Time** | Tasks are scheduled with both a calendar date and an hour/minute timestamp (`DateTime DueDate`). | [`FollowUpInputForm.designer.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpInputForm.designer.cs#L170-L195), [`FollowUpInputForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpInputForm.cs#L182-L188) |
| **Title vs. Context Notes** | Separate short subject title (`Title`, max 200 chars) from rich context preparation notes (`Notes`, max 2000 chars). | [`TaskReminder.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.domain/Entities/TaskReminder.cs#L14-L21), [`RealEstateDbContext.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit.infrastructure/Data/RealEstateDbContext.cs#L415-L419) |
| **Snooze Quick Action** | One-click context menu flyout to push tasks forward (+1 Day, +3 Days, +1 Week) without having to manually pick dates. | [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs#L422-L452) |
| **Overdue / Today / Upcoming Grouping** | Real-world task management grouping using KPI summary cards and segmented filter pills. | [`FollowUpsView.designer.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.designer.cs#L85-L165), [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs#L268-L295) |
| **Priority Levels** | Low, Medium, High priorities styled using theme-consistent alert colors. | [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs#L384-L394) |
| **ActionsColumn "⋮" Menu** | Consistent single-button row action menu (Mark Complete, Snooze, Reschedule, Edit, Archive) using `ActionsColumn`. | [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs#L408-L476) |
| **Azure Design System Theming** | Zero hard-coded colors. All colors and borders derived through static `Theme` and `UiRadiusHelper` / `UiGridHelper`. | Throughout [`FollowUpsView.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpsView.cs), [`FollowUpInputForm.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/FollowUpInputForm.cs), [`CompleteFollowUpDialog.cs`](file:///c:/Users/ACER/source/repos/CRMS_Peguit/CRMS_Peguit/Views/FollowUps/CompleteFollowUpDialog.cs) |

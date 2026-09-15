using System.Drawing;
using CRMS_Peguit.Models;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Central design system for the NEXA CRM application,
    /// derived directly from AzureShades, AzureTints, and AzureTones.
    /// </summary>
    public static class Theme
    {
        // Core Palette (driven by AzureShades, AzureTints, AzureTones)
        public static Color Primary = AzureTints.SkylineBlue;              // 10% white (37, 103, 156)
        public static Color PrimaryDark = AzureShades.PressedAzure;         // 10% black (12, 77, 130)
        public static Color PrimaryLight = AzureTints.WhisperTint;          // 90% white (231, 238, 244)
        public static Color PrimaryBadgeText = AzureTones.SecondaryAzure;   // 10% gray (24, 90, 143)
        public static Color Surface = AzureTints.PureWhite;                // 100% white (255, 255, 255)
        public static Color Background = AzureTints.BackgroundWash;        // 95% white (243, 247, 250)
        public static Color TextPrimary = AzureShades.DeepInterface;        // 40% black (8, 52, 87)
        public static Color TextSecondary = AzureTones.NeutralAzure;        // 50% gray (70, 107, 136)
        public static Color Border = AzureTints.GhostBlue;                 // 80% white (207, 221, 233)
        public static Color Success = Color.FromArgb(22, 101, 52);
        public static Color Danger = Color.FromArgb(153, 27, 27);

        // Sidebar & Dark Hero Palette (Modern Dark Slate #0F172A)
        public static Color SidebarBackground = Color.FromArgb(15, 23, 42);   // Slate 900 (#0F172A)
        public static Color SidebarSelected = Color.FromArgb(30, 41, 59);     // Slate 800 (#1E293B)
        public static Color SidebarHover = Color.FromArgb(30, 41, 59);        // Slate 800 (#1E293B)
        public static Color SidebarText = Color.FromArgb(148, 163, 184);      // Slate 400 (#94A3B8)
        public static Color SidebarTextActive = Color.White;
        public static Color SidebarTextMuted = Color.FromArgb(100, 116, 139); // Slate 500 (#64748B)
        public static Color SidebarProfileCard = Color.FromArgb(30, 41, 59);  // Slate 800 (#1E293B)
        public static Color SidebarAccent = Color.FromArgb(56, 189, 248);     // Sky 400 (#38BDF8)

        // Top Header (driven by AzureTints)
        public static Color HeaderBackground = AzureTints.PureWhite;
        public static Color HeaderBorder = Color.FromArgb(226, 232, 240);     // Slate 200 (#E2E8F0)

        // Accessible UI Borders (≥ 3:1 against white/surface)
        public static Color BorderAccessible = Color.FromArgb(203, 213, 225); // Slate 300 (#CBD5E1)

        // Visible Focus Ring for keyboard navigation (Fitts's / WCAG 2.4.7)
        public static Color FocusBorder = Color.FromArgb(14, 165, 233);

        // Minimalist Status Palette (Strictly No Badges/Pills)
        public static Color StatusSuccess = Color.FromArgb(5, 150, 105);      // Emerald (#059669) - Converted, Active, Available, Closed, Resolved
        public static Color StatusPending = Color.FromArgb(217, 119, 6);      // Amber (#D97706) - Contacted, Pending Review, Offer, Contract, In Progress
        public static Color StatusAlert = Color.FromArgb(220, 38, 38);        // Rose (#DC2626) - Inactive, Overdue, Lost, Urgent, Critical, High
        public static Color StatusNeutral = Color.FromArgb(71, 85, 105);      // Slate (#475569) - New, Unassigned, Low, Medium, Prospect

        // Legacy Status Badge Palette (retained for backward compatibility)
        public static Color StatusActiveBg = Color.FromArgb(220, 252, 231);
        public static Color StatusActiveText = Color.FromArgb(22, 101, 52);
        public static Color StatusFollowUpBg = Color.FromArgb(254, 243, 199);
        public static Color StatusFollowUpText = Color.FromArgb(180, 83, 9);
        public static Color StatusInactiveBg = Color.FromArgb(254, 226, 226);
        public static Color StatusInactiveText = Color.FromArgb(153, 27, 27);

        // OS High-Contrast Mode Awareness
        public static bool IsHighContrast => System.Windows.Forms.SystemInformation.HighContrast;

        public static Color GetEffectiveTextColor(Color defaultColor)
        {
            return IsHighContrast ? System.Drawing.SystemColors.WindowText : defaultColor;
        }

        public static Color GetEffectiveSurfaceColor(Color defaultColor)
        {
            return IsHighContrast ? System.Drawing.SystemColors.Window : defaultColor;
        }
    }
}
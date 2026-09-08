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

        // Sidebar & Dark Hero Palette (driven by AzureShades, AzureTints, AzureTones)
        public static Color SidebarBackground = AzureShades.HighEmphasisBlue;  // 60% black (5, 34, 58)
        public static Color SidebarSelected = AzureShades.OverlayBlue;         // 30% black (9, 60, 102)
        public static Color SidebarHover = AzureShades.DeepInterface;          // 40% black (8, 52, 87)
        public static Color SidebarText = AzureTints.GhostBlue;                // 80% white (207, 221, 233)
        public static Color SidebarTextMuted = AzureTones.InactiveBlueIcon;    // 60% gray (82, 111, 135)
        public static Color SidebarProfileCard = AzureShades.ContrastAzure;    // 50% black (6, 43, 72)

        // Top Header (driven by AzureTints)
        public static Color HeaderBackground = AzureTints.PureWhite;
        public static Color HeaderBorder = AzureTints.GhostBlue;

        // Status Badge Palette
        public static Color StatusActiveBg = Color.FromArgb(220, 252, 231);
        public static Color StatusActiveText = Color.FromArgb(22, 101, 52);
        public static Color StatusFollowUpBg = Color.FromArgb(254, 243, 199);
        public static Color StatusFollowUpText = Color.FromArgb(180, 83, 9);
        public static Color StatusInactiveBg = Color.FromArgb(254, 226, 226);
        public static Color StatusInactiveText = Color.FromArgb(153, 27, 27);
    }
}
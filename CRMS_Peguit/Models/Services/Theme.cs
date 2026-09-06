using CRMS_Peguit.Models;

namespace CRMS_Peguit.winforms.Models.Services
{
    public static class Theme
    {
        // Core Palette
        public static Color Primary = Color.FromArgb(15, 91, 158); // #0F5B9E
        public static Color PrimaryDark = Color.FromArgb(10, 69, 120);
        public static Color PrimaryLight = Color.FromArgb(238, 246, 255); // #EEF6FF
        public static Color PrimaryBadgeText = Color.FromArgb(29, 108, 176);
        public static Color Surface = Color.White;
        public static Color Background = Color.FromArgb(244, 247, 251); // #F4F7FB
        public static Color TextPrimary = Color.FromArgb(15, 23, 42); // #0F172A
        public static Color TextSecondary = Color.FromArgb(100, 116, 139); // #64748B
        public static Color Border = Color.FromArgb(226, 232, 240); // #E2E8F0
        public static Color Success = Color.FromArgb(22, 101, 52);
        public static Color Danger = Color.FromArgb(153, 27, 27);

        // Sidebar & Dark Hero Palette
        public static Color SidebarBackground = Color.FromArgb(6, 29, 51); // #061D33
        public static Color SidebarSelected = Color.FromArgb(19, 56, 99); // #133863
        public static Color SidebarHover = Color.FromArgb(12, 42, 74); // #0C2A4A
        public static Color SidebarText = Color.FromArgb(203, 213, 225); // #CBD5E1
        public static Color SidebarTextMuted = Color.FromArgb(88, 118, 147); // #587693
        public static Color SidebarProfileCard = Color.FromArgb(12, 40, 68); // #0C2844

        // Top Header
        public static Color HeaderBackground = Color.White;
        public static Color HeaderBorder = Color.FromArgb(226, 232, 240);

        // Status Badge Palette
        public static Color StatusActiveBg = Color.FromArgb(220, 252, 231); // #DCFCE7
        public static Color StatusActiveText = Color.FromArgb(22, 101, 52); // #166534
        public static Color StatusFollowUpBg = Color.FromArgb(254, 243, 199); // #FEF3C7
        public static Color StatusFollowUpText = Color.FromArgb(180, 83, 9); // #B45309
        public static Color StatusInactiveBg = Color.FromArgb(254, 226, 226); // #FEE2E2
        public static Color StatusInactiveText = Color.FromArgb(153, 27, 27); // #991B1B
    }
}
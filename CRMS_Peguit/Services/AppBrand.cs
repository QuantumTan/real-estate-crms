using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CRMS_Peguit.winforms.Models.Services
{
    /// <summary>
    /// Central provider for application branding assets, including the brand logo and window icon.
    /// </summary>
    public static class AppBrand
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private static Image? _logoImage;
        private static Icon? _appIcon;
        private static bool _initialized;

        /// <summary>
        /// Gets the cached brand logo image.
        /// </summary>
        public static Image? Logo
        {
            get
            {
                EnsureLoaded();
                return _logoImage;
            }
        }

        /// <summary>
        /// Gets the application icon derived from the brand logo.
        /// </summary>
        public static Icon? AppIcon
        {
            get
            {
                EnsureLoaded();
                return _appIcon;
            }
        }

        public static void ApplyAppIcon(Form form)
        {
            try
            {
                if (AppIcon != null)
                {
                    form.Icon = AppIcon;
                }
            }
            catch
            {
                // Silently ignore if OS icon creation is not supported
            }
        }

        private static void EnsureLoaded()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                _logoImage = LoadLogoImage();
                if (_logoImage is Bitmap bmp)
                {
                    _appIcon = CreateIconFromBitmap(bmp);
                }
            }
            catch
            {
                // Fallback to null if loading fails
            }
        }

        private static Image? LoadLogoImage()
        {
            // 1. Try loading from Embedded Resources
            var asm = Assembly.GetExecutingAssembly();
            var resourceNames = asm.GetManifestResourceNames();
            var targetResource = resourceNames.FirstOrDefault(n => n.EndsWith("logo.png", StringComparison.OrdinalIgnoreCase))
                                ?? resourceNames.FirstOrDefault(n => n.IndexOf("pokecutweb", StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(targetResource))
            {
                using var stream = asm.GetManifestResourceStream(targetResource);
                if (stream != null)
                {
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    return Image.FromStream(ms);
                }
            }

            // 2. Try loading from output directory / Assets folder
            string[] possiblePaths =
            {
                Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png"),
                Path.Combine(AppContext.BaseDirectory, "Assets", "pokecutweb_1789365462537 - Copy.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.png"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pokecutweb_1789365462537 - Copy.png"),
                @"C:\Users\ACER\Downloads\pokecutweb_1789365462537 - Copy.png"
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(path);
                        using var ms = new MemoryStream(bytes);
                        return Image.FromStream(ms);
                    }
                    catch
                    {
                        // continue to next candidate
                    }
                }
            }

            return null;
        }

        private static Icon? CreateIconFromBitmap(Bitmap bitmap)
        {
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                using var iconBmp = new Bitmap(48, 48);
                using (var g = Graphics.FromImage(iconBmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    float scale = Math.Min(48f / bitmap.Width, 48f / bitmap.Height);
                    int nw = (int)(bitmap.Width * scale);
                    int nh = (int)(bitmap.Height * scale);
                    int nx = (48 - nw) / 2;
                    int ny = (48 - nh) / 2;
                    g.DrawImage(bitmap, nx, ny, nw, nh);
                }

                hIcon = iconBmp.GetHicon();
                using var tempIcon = Icon.FromHandle(hIcon);
                return (Icon)tempIcon.Clone();
            }
            catch
            {
                return null;
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                {
                    try
                    {
                        DestroyIcon(hIcon);
                    }
                    catch { }
                }
            }
        }
    }
}

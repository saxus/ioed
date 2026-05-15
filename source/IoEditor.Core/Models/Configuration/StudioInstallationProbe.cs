using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace IoEditor.Models.Configuration
{
    /// <summary>
    /// Detects default LEGO Studio install locations and validates a user-configured root folder.
    /// </summary>
    internal static class StudioInstallationProbe
    {
        private static readonly string[] RequiredDataRelativePaths =
        {
            Path.Combine("data", "StudioPartDefinition2.txt"),
            Path.Combine("data", "StudioColorDefinition.txt")
        };

        /// <summary>
        /// If <paramref name="options"/>.StudioFolder is empty and the provider returns a valid path, sets it.
        /// </summary>
        public static bool TryApplySuggestedRootIfEmpty(StudioOptions options, IStudioDefaultPathProvider? provider)
        {
            if (!string.IsNullOrWhiteSpace(options.StudioFolder))
            {
                return false;
            }

            var suggested = provider?.GetDefaultStudioRoot();
            if (string.IsNullOrEmpty(suggested) || !IsValidStudioRoot(suggested, out _))
            {
                return false;
            }

            options.StudioFolder = suggested;
            return true;
        }

        public static bool IsValidStudioRoot(string directory)
            => IsValidStudioRoot(directory, out _);

        public static bool IsValidStudioRoot(string directory, [NotNullWhen(false)] out string? error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                error = "Directory does not exist.";
                return false;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var exe = Path.Combine(directory, "studio.exe");
                if (!File.Exists(exe))
                {
                    error = "This folder does not contain studio.exe.";
                    return false;
                }
            }

            foreach (var rel in RequiredDataRelativePaths)
            {
                var full = Path.Combine(directory, rel);
                if (!File.Exists(full))
                {
                    error = $"Missing required file: {rel}";
                    return false;
                }
            }

            return true;
        }
    }
}

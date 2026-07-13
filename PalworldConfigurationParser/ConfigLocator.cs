using System.Runtime.InteropServices;

namespace PalworldConfigurationParser;

public static class ConfigLocator
{
    public static string GetDefaultLocation()
    {
        string osFolder;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            osFolder = "WindowsServer";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Check if running under Wine
            osFolder = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINEPREFIX")) ? "WindowsServer" : "LinuxServer";
        }
        else
        {
            throw new NotSupportedException("Unsupported operating system");
        }

        var path = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "..",
            "Pal",
            "Saved",
            "Config",
            osFolder,
            "PalWorldSettings.ini"
        ));

        return path;
    }
}
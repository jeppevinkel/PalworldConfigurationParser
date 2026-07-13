namespace PalworldConfigurationParser.Models;

public sealed record SettingValue(
    string Name,
    SettingTypes SettingType,
    bool Sensitive = false,
    params string[] EnvVars);

// Usage:
// new SettingValue("Port", SettingTypes.Integer, EnvVars: ["MYAPP_PORT", "OTHERTOOL_PORT"])
// First env var found wins, so put your preferred names first.
using System.Globalization;
using System.Text.RegularExpressions;
using PalworldConfigurationParser.Models;

namespace PalworldConfigurationParser;

public static partial class SettingsUpdater
{
    private delegate bool TryParse<T>(string input, out T result);

    public static void ApplyEnvironmentOverrides(PalworldSettingsFile settings, IEnumerable<SettingValue> supportedSettings)
    {
        int changed = 0, unchanged = 0, notSet = 0;

        foreach (var setting in supportedSettings)
        {
            var (envVar, rawValue) = ResolveEnvVar(setting.EnvVars);
            if (rawValue is null)
            {
                notSet++;
                continue;
            }

            bool wasChanged = setting.SettingType switch
            {
                SettingTypes.String =>
                    Apply<string>(settings, setting, envVar!, rawValue,
                        static (string s, out string v) => { v = s; return !s.Contains('"'); },
                        settings.GetString, settings.SetString),

                SettingTypes.BrowserDisplay =>
                    Apply<string>(settings, setting, envVar!, rawValue,
                        static (string s, out string v) =>
                        {
                            v = PalworldSettingsFile.EncodeForServerBrowser(s);
                            return true;
                        },
                        settings.GetString, settings.SetString,
                        describe: static v =>
                            $"\"{PalworldSettingsFile.PredictBrowserDisplay(v)}\""),

                SettingTypes.Integer =>
                    Apply<int>(settings, setting, envVar!, rawValue,
                        static (string s, out int v) =>
                            int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v),
                        settings.GetInt, settings.SetInt),

                SettingTypes.Float =>
                    Apply<double>(settings, setting, envVar!, rawValue,
                        static (string s, out double v) =>
                        {
                            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                                return false;
                            // Mirror SetFloat's F6 serialization so unchanged-detection is idempotent.
                            v = double.Parse(v.ToString("F6", CultureInfo.InvariantCulture),
                                CultureInfo.InvariantCulture);
                            return true;
                        },
                        settings.GetFloat, settings.SetFloat),

                SettingTypes.Boolean =>
                    Apply<bool>(settings, setting, envVar!, rawValue,
                        TryParseFlexibleBool, settings.GetBool, settings.SetBool),

                SettingTypes.AlphaDash =>
                    Apply<string>(settings, setting, envVar!, rawValue,
                        static (string s, out string v) =>
                        {
                            v = s;
                            return MatchAlphaDashRegex().IsMatch(s);
                        },
                        settings.GetString, settings.SetString),

                SettingTypes.PlatformList =>
                    Apply<string>(settings, setting, envVar!, rawValue,
                        static (string s, out string v) => PalworldSettingsFile.TryParsePlatformList(s, out v),
                        name => settings.GetRaw(name) ?? "",   // null (key absent) => always counts as changed
                        settings.SetRaw),

                _ => throw new NotSupportedException(
                    $"Setting '{setting.Name}' has unsupported type {setting.SettingType}.")
            };

            if (wasChanged) changed++; else unchanged++;
        }

        Console.WriteLine($"Environment config overrides: {changed} changed, {unchanged} unchanged, {notSet} not set.");
    }

    private static (string? EnvVar, string? Value) ResolveEnvVar(IReadOnlyList<string> envVars)
    {
        foreach (var name in envVars)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(value))
                return (name, value);
        }
        return (null, null);
    }

    private static bool Apply<T>(
        PalworldSettingsFile settings,
        SettingValue setting,
        string envVar,
        string rawValue,
        TryParse<T> parse,
        Func<string, T> get,
        Action<string, T> set,
        Func<T, string>? describe = null)
    {
        if (!parse(rawValue, out var newValue))
        {
            var shown = setting.Sensitive ? "<redacted>" : $"\"{rawValue}\"";
            throw new FormatException(
                $"Setting '{setting.Name}': cannot parse {shown} from environment variable '{envVar}' as {typeof(T).Name}.");
        }

        var oldValue = get(setting.Name);
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return false;

        set(setting.Name, newValue);

        if (setting.Sensitive)
        {
            Console.WriteLine($"Updated '{setting.Name}' via {envVar} (value redacted).");
        }
        else
        {
            describe ??= static v => $"\"{v}\"";
            Console.WriteLine($"Updated '{setting.Name}': {describe(oldValue)} -> {describe(newValue)} (via {envVar}).");
        }

        return true;
    }

    private static bool TryParseFlexibleBool(string input, out bool value)
    {
        switch (input.Trim())
        {
            case "1": value = true; return true;
            case "0": value = false; return true;
            default: return bool.TryParse(input, out value); // "true"/"false", any casing
        }
    }

    // No official spec exists for password characters. Community consensus
    // (hosting provider docs + pelican-eggs/Palworld-Config-Parser-Tool) is alphanumeric,
    // dash, and underscore only — special chars/spaces break /AdminPassword
    // chat auth and RCON. Length limit of 30 reported by some hosts.
    [GeneratedRegex("^[a-zA-Z0-9_-]{1,30}$")]
    private static partial Regex MatchAlphaDashRegex();
}
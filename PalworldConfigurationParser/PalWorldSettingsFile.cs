using System.Globalization;
using System.Text;

namespace PalworldConfigurationParser;

public class PalworldSettingsFile
{
    private readonly List<string> _lines = new();          // original file lines
    private int _optionSettingsLineIndex = -1;
    private readonly List<KeyValuePair<string, string>> _settings = new(); // preserves order

    private const char PipeLookalike = '\u2223';
    private static readonly string[] KnownPlatforms = ["Steam", "Xbox", "PS5", "Mac"];

    public static PalworldSettingsFile Load(string path)
    {
        var file = new PalworldSettingsFile();
        file._lines.AddRange(File.ReadAllLines(path));

        for (int i = 0; i < file._lines.Count; i++)
        {
            var line = file._lines[i].TrimStart();
            if (line.StartsWith("OptionSettings=", StringComparison.OrdinalIgnoreCase))
            {
                file._optionSettingsLineIndex = i;
                var value = line["OptionSettings=".Length..].Trim();
                file.ParseStruct(value);
                break;
            }
        }

        if (file._optionSettingsLineIndex < 0)
            throw new FormatException("OptionSettings entry not found.");

        return file;
    }

    private void ParseStruct(string raw)
    {
        if (!raw.StartsWith('(') || !raw.EndsWith(')'))
            throw new FormatException("OptionSettings value is not wrapped in parentheses.");

        var inner = raw[1..^1];

        foreach (var pair in SplitTopLevel(inner))
        {
            int eq = pair.IndexOf('=');
            if (eq < 0) continue; // shouldn't happen, but be lenient
            _settings.Add(new(pair[..eq].Trim(), pair[(eq + 1)..].Trim()));
        }
    }

    // Splits on commas, but ignores commas inside quotes or nested parentheses.
    private static IEnumerable<string> SplitTopLevel(string s)
    {
        var sb = new StringBuilder();
        int depth = 0;
        bool inQuotes = false;

        foreach (char c in s)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (!inQuotes && c == '(') depth++;
            else if (!inQuotes && c == ')') depth--;

            if (c == ',' && depth == 0 && !inQuotes)
            {
                yield return sb.ToString();
                sb.Clear();
            }
            else sb.Append(c);
        }
        if (sb.Length > 0) yield return sb.ToString();
    }

    // ---------- raw access ----------

    public string? GetRaw(string key) =>
        _settings.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;

    public void SetRaw(string key, string rawValue)
    {
        for (int i = 0; i < _settings.Count; i++)
        {
            if (_settings[i].Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                _settings[i] = new(_settings[i].Key, rawValue);
                return;
            }
        }
        _settings.Add(new(key, rawValue)); // new key: append
    }

    // ---------- typed helpers ----------

    public string GetString(string key)
    {
        var v = GetRaw(key) ?? "";
        return v.Length >= 2 && v.StartsWith('"') && v.EndsWith('"') ? v[1..^1] : v;
    }

    public bool GetBool(string key) =>
        string.Equals(GetRaw(key), "True", StringComparison.OrdinalIgnoreCase);

    public int GetInt(string key) =>
        int.Parse(GetRaw(key) ?? "0", CultureInfo.InvariantCulture);

    public double GetFloat(string key) =>
        double.Parse(GetRaw(key) ?? "0", CultureInfo.InvariantCulture);

    public void SetString(string key, string value)
    {
        if (value.Contains('"'))
        {
            throw new ArgumentException("Value cannot contain double quotes");
        }
        SetRaw(key, $"\"{value}\"");
    }

    public void SetBool(string key, bool value) => SetRaw(key, value ? "True" : "False");
    public void SetInt(string key, int value) => SetRaw(key, value.ToString(CultureInfo.InvariantCulture));
    public void SetFloat(string key, double value) => SetRaw(key, value.ToString("F6", CultureInfo.InvariantCulture));

    /// <summary>
    /// Encodes text for fields rendered in the in-game server browser
    /// (ServerName, ServerDescription). The client displays every ASCII '|' as '"'.
    /// </summary>
    public static string EncodeForServerBrowser(string display) =>
        display
            .Replace('|', PipeLookalike)  // literal pipes -> lookalike
            .Replace('"', '|');           // desired quotes -> pipe

    /// <summary>
    /// Predicts what the in-game server browser will display for a stored value.
    /// Note: the REST API returns the raw stored string, NOT this.
    /// </summary>
    public static string PredictBrowserDisplay(string stored) =>
        stored
            .Replace('|', '"')
            .Replace(PipeLookalike, '|');

    /// <summary>
    /// Parses a platform list from flexible input — with/without parentheses,
    /// optional quotes, any casing, any order — and normalizes to the canonical
    /// ini form: (Steam,Xbox,PS5,Mac). Requires at least one valid platform.
    /// </summary>
    public static bool TryParsePlatformList(string input, out string normalized)
    {
        normalized = string.Empty;

        var span = input.Trim();
        if (span.StartsWith('(') && span.EndsWith(')'))
            span = span[1..^1];

        var requested = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in span.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var name = token.Trim('"');
            if (!KnownPlatforms.Contains(name, StringComparer.OrdinalIgnoreCase))
                return false; // unknown platform, e.g. typo like "Playstation"

            requested.Add(name);
        }

        // "you MUST leave at least one platform for the server to work"
        if (requested.Count == 0)
            return false;

        // Canonical casing and canonical order, so comparison against the
        // stored value is a plain string equality check.
        var canonical = KnownPlatforms.Where(requested.Contains);
        normalized = $"({string.Join(",", canonical)})";
        return true;
    }

    // ---------- writing ----------

    public void Save(string path)
    {
        var joined = string.Join(",", _settings.Select(kv => $"{kv.Key}={kv.Value}"));
        _lines[_optionSettingsLineIndex] = $"OptionSettings=({joined})";
        File.WriteAllLines(path, _lines);
    }
}

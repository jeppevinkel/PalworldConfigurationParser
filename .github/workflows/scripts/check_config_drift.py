#!/usr/bin/env python3
"""
check_config_drift.py

Compares the SettingValue definitions in Program.cs against every key found
in Palworld's DefaultPalWorldSettings.ini (downloaded via SteamCMD).

Reports:
  +  In config, NOT in code  ->  candidate for implementation
                                 (shown with its default value and inferred type)
  -  In code,   NOT in config ->  possibly renamed / removed by Pocketpair

Exit codes:
  0  Clean - no drift detected
  1  Drift detected (or a setup error)
"""

import os
import re
import sys
from pathlib import Path

# ---------------------------------------------------------------------------
# Paths - overridden by environment variables set in the workflow
# ---------------------------------------------------------------------------
PROGRAM_CS  = Path(os.environ.get("PROGRAM_CS_PATH",  "PalworldConfigurationParser/Program.cs"))
CONFIG_FILE = Path(os.environ.get("CONFIG_FILE_PATH", "gameserver/DefaultPalWorldSettings.ini"))

# Maximum raw-value width to display in the console table before truncating.
_CONSOLE_VAL_WIDTH = 50

# ---------------------------------------------------------------------------
# Regex - intentionally identical to the one in the README-update script.
# If you ever modify one, modify both; or extract to a shared utils module.
# ---------------------------------------------------------------------------
SETTING_RE = re.compile(
    r'new\s+SettingValue\(\s*"([^"]+)"\s*,\s*SettingTypes\.(\w+)'
    r'(?:\s*,\s*(?:true|false))?'       # optional IsSecret bool param
    r'\s*,\s*EnvVars:\s*\[([^\]]+)\]'  # EnvVars: ["A", "B", ...]
    r'\s*\)',
    re.DOTALL,
)


# ---------------------------------------------------------------------------
# Parsers
# ---------------------------------------------------------------------------

def parse_cs_settings(source: str) -> set[str]:
    """Return every setting name declared as a SettingValue in Program.cs."""
    return {name for name, _, _ in SETTING_RE.findall(source)}


def parse_config_settings(config_text: str) -> dict[str, str]:
    """
    Parse the OptionSettings=(...) block and return a {key: raw_default_value} dict.

    A hand-written scanner is used instead of a simple split/regex so that
    commas *inside* nested structures are never mistaken for pair separators:

      Quoted strings:        ServerName="Default Palworld Server"
      Parenthesised lists:   CrossplayPlatforms=(Steam,Xbox,PS5,Mac)
      Bare values:           DayTimeSpeedRate=1.000000,  bIsPvP=False
      Empty values:          DenyTechnologyList=,
    """
    block_match = re.search(r'OptionSettings=\((.+)\)', config_text, re.DOTALL)
    if not block_match:
        print("ERROR: OptionSettings=(...) block not found in config file.", file=sys.stderr)
        sys.exit(1)

    block = block_match.group(1)
    settings: dict[str, str] = {}
    i = 0

    while i < len(block):
        # -- Find the next key (identifier immediately followed by '=') -------
        key_match = re.match(r'([A-Za-z_][A-Za-z0-9_]*)=', block[i:])
        if not key_match:
            i += 1
            continue

        key = key_match.group(1)
        i += len(key_match.group(0))   # advance past 'Key='
        value_start = i

        # -- Parse the value --------------------------------------------------
        if i >= len(block) or block[i] == ',':
            # Empty value - e.g. DenyTechnologyList=,
            value = ""

        elif block[i] == '"':
            # Quoted string - scan for the closing quote.
            i += 1
            while i < len(block) and block[i] != '"':
                i += 1
            if i < len(block):
                i += 1  # step past the closing quote
            value = block[value_start:i]

        elif block[i] == '(':
            # Nested parenthesised list - track depth to find the matching ')'.
            depth = 0
            while i < len(block):
                if block[i] == '(':
                    depth += 1
                elif block[i] == ')':
                    depth -= 1
                    if depth == 0:
                        i += 1
                        break
                i += 1
            value = block[value_start:i]

        else:
            # Bare value - read until the next comma (don't consume it).
            while i < len(block) and block[i] != ',':
                i += 1
            value = block[value_start:i]

        settings[key] = value

        # Skip the comma separator between pairs
        if i < len(block) and block[i] == ',':
            i += 1

    return settings


# ---------------------------------------------------------------------------
# Type inference
# ---------------------------------------------------------------------------

def infer_type(raw_value: str) -> str:
    """
    Best-effort guess at the appropriate SettingTypes enum value, based solely
    on the raw default-value string extracted from the config file.

    Intentionally conservative for the string-like family:
      String, BrowserDisplay, and AlphaDash are indistinguishable from the
      config alone, so they are all labelled 'String?' to prompt manual review.
    """
    v = raw_value.strip()

    if v.lower() in ("true", "false"):
        return "Boolean"
    if re.fullmatch(r'-?\d+\.\d+', v):
        return "Float"
    if re.fullmatch(r'-?\d+', v):
        return "Integer"
    if v.startswith('(') and v.endswith(')'):
        return "PlatformList"

    # Quoted string ("..."), bare word (None / Item / Text / ...), or empty
    return "String?"


# ---------------------------------------------------------------------------
# GitHub Actions helpers
# ---------------------------------------------------------------------------

def append_summary(text: str) -> None:
    """Append markdown to the GitHub Actions step summary, if available."""
    path = os.environ.get("GITHUB_STEP_SUMMARY")
    if path:
        with open(path, "a", encoding="utf-8") as fh:
            fh.write(text)


def set_output(key: str, value: str) -> None:
    """Write a key=value pair to GITHUB_OUTPUT for use in later steps."""
    path = os.environ.get("GITHUB_OUTPUT")
    if path:
        with open(path, "a", encoding="utf-8") as fh:
            fh.write(f"{key}={value}\n")


# ---------------------------------------------------------------------------
# Reporting
# ---------------------------------------------------------------------------

def _truncate(s: str, max_len: int) -> str:
    """Shorten s to max_len characters, appending '...' if truncated."""
    return s if len(s) <= max_len else s[: max_len - 3] + "..."


def build_console_table(unhandled: list[tuple[str, str]]) -> str:
    """
    Render the unhandled settings as an aligned plain-text table for the
    runner log, e.g.:

      Setting                                          Default Value                        Inferred Type
      ---------------------------------------------    ------------------------------------  -------------
      + bAdditionalDropItemWhenPlayerKillingInPvP...  "PlayerDropItem"                      String?
      + GuildRejoinCooldownMinutes                  0                                     Integer
      + PhysicsActiveDropItemMaxNum                 -1                                    Integer
    """
    if not unhandled:
        return ""

    name_w = max(len(name) for name, _ in unhandled)
    val_w  = _CONSOLE_VAL_WIDTH

    lines = [
        # "+ " prefix (2 chars) is part of the data rows; the header pads to match.
        f"  {'Setting':<{name_w + 2}}  {'Default Value':<{val_w}}  Inferred Type",
        f"  {'-' * (name_w + 2)}  {'-' * val_w}  {'-' * 13}",
    ]

    for name, raw in unhandled:
        display_val = _truncate(raw if raw else "(empty)", val_w)
        lines.append(f"  + {name:<{name_w}}  {display_val:<{val_w}}  {infer_type(raw)}")

    return "\n".join(lines)


def build_summary(
    cs_count:  int,
    cfg_count: int,
    unhandled: list[tuple[str, str]],   # (setting_name, raw_default_value)
    stale:     list[str],
) -> str:
    """Build the full GitHub Step Summary as a markdown string."""
    lines: list[str] = []

    lines.append("## Palworld Config Drift Report\n\n")
    lines.append("| | Count |\n|---|---|\n")
    lines.append(f"| Settings in `Program.cs` | **{cs_count}** |\n")
    lines.append(f"| Settings in `DefaultPalWorldSettings.ini` | **{cfg_count}** |\n")
    lines.append(f"| + Unhandled (config -> code) | **{len(unhandled)}** |\n")
    lines.append(f"| - Stale (code -> config)     | **{len(stale)}** |\n\n")

    if not unhandled and not stale:
        lines.append("### No discrepancies found!\n\n")
        lines.append(
            "Every setting in `DefaultPalWorldSettings.ini` is handled in `Program.cs`, "
            "and every setting in `Program.cs` exists in the config file.\n"
        )
        return "".join(lines)

    if unhandled:
        lines.append(
            f"### + In config but **not** in code - {len(unhandled)} setting(s)\n\n"
            "These settings exist in the game's default config but have no `SettingValue` "
            "in `Program.cs`. "
            "The **Default Value** and **Inferred Type** columns are provided to help "
            "decide which `SettingTypes` to use when implementing.\n\n"
            "> **Note:** `String?` means the value is string-like in the config but could "
            "map to `String`, `BrowserDisplay`, or `AlphaDash` - manual review is needed "
            "to distinguish them.\n\n"
        )
        lines.append("| Setting | Default Value | Inferred Type |\n|---|---|---|\n")
        for name, raw in unhandled:
            display_val = f"`{raw}`" if raw else "*(empty)*"
            lines.append(f"| `{name}` | {display_val} | `{infer_type(raw)}` |\n")
        lines.append("\n")

    if stale:
        lines.append(
            f"### - In code but **not** in config - {len(stale)} setting(s)\n\n"
            "These settings are declared in `Program.cs` but were not found in "
            "`DefaultPalWorldSettings.ini`. They may have been renamed or removed "
            "by Pocketpair in a recent update:\n\n"
        )
        lines.append("| Setting |\n|---|\n")
        lines.extend(f"| `{s}` |\n" for s in stale)
        lines.append("\n")

    return "".join(lines)


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def main() -> int:
    # -- Validate that both files exist before doing anything -----------------
    errors: list[str] = []
    if not PROGRAM_CS.exists():
        errors.append(f"`{PROGRAM_CS}` not found. Check PROGRAM_CS_PATH.")
    if not CONFIG_FILE.exists():
        errors.append(f"`{CONFIG_FILE}` not found. Check CONFIG_FILE_PATH.")

    if errors:
        msg = "\n".join(f"- {e}" for e in errors)
        print(f"ERROR:\n{msg}", file=sys.stderr)
        append_summary("## Config Drift Check - Setup Error\n\n" + msg + "\n")
        return 1

    # -- Parse both sources ---------------------------------------------------
    cs_settings     = parse_cs_settings(PROGRAM_CS.read_text(encoding="utf-8"))
    config_settings = parse_config_settings(CONFIG_FILE.read_text(encoding="utf-8"))

    # -- Compute the two-way diff ---------------------------------------------
    # Unhandled: in config but not in code - include the raw default value.
    unhandled: list[tuple[str, str]] = sorted(
        (
            (name, config_settings[name])
            for name in config_settings
            if name not in cs_settings
        ),
        key=lambda t: t[0],
    )

    # Stale: in code but not in config.
    stale: list[str] = sorted(cs_settings - config_settings.keys())

    has_drift = bool(unhandled or stale)

    # -- Console / runner log output ------------------------------------------
    print(f"Settings in Program.cs               : {len(cs_settings)}")
    print(f"Settings in DefaultPalWorldSettings  : {len(config_settings)}")
    print()

    if unhandled:
        print(f"[WARN] {len(unhandled)} setting(s) in config but NOT in code (need implementation):")
        print()
        print(build_console_table(unhandled))
    else:
        print("[OK]   All config settings are handled in code.")

    print()

    if stale:
        print(f"[WARN] {len(stale)} setting(s) in code but NOT in config (possibly renamed/removed):")
        for s in stale:
            print(f"         -  {s}")
    else:
        print("[OK]   All code settings are present in the config.")

    # -- Step outputs ---------------------------------------------------------
    set_output("has_drift",       "true" if has_drift else "false")
    set_output("unhandled_count", str(len(unhandled)))
    set_output("stale_count",     str(len(stale)))

    # -- GitHub Step Summary --------------------------------------------------
    append_summary(
        build_summary(len(cs_settings), len(config_settings), unhandled, stale)
    )

    return 1 if has_drift else 0


if __name__ == "__main__":
    sys.exit(main())
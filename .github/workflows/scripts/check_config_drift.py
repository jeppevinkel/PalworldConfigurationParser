#!/usr/bin/env python3
"""
check_config_drift.py

Compares the SettingValue definitions in Program.cs against every key found
in Palworld's DefaultPalWorldSettings.ini (downloaded via SteamCMD).

Reports:
  +  In config, NOT in code  ->  candidate for implementation
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
# Paths — overridden by environment variables set in the workflow
# ---------------------------------------------------------------------------
PROGRAM_CS  = Path(os.environ.get("PROGRAM_CS_PATH",  "PalworldConfigurationParser/Program.cs"))
CONFIG_FILE = Path(os.environ.get("CONFIG_FILE_PATH", "gameserver/DefaultPalWorldSettings.ini"))

# ---------------------------------------------------------------------------
# Regex — intentionally identical to the one in the README-update script.
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


def parse_config_settings(config_text: str) -> set[str]:
    """
    Extracts every key name from the OptionSettings=(...) block.

    The block is a single long line of comma-separated Key=Value pairs.
    A key is a C-style identifier immediately followed by '='.

    Values are numbers, bare words (True/False/None), quoted strings, or
    nested lists - none of which contain an 'identifier=' sub-pattern, so
    false positives are not a concern.
    """
    match = re.search(r'OptionSettings=\((.+)\)', config_text, re.DOTALL)
    if not match:
        print("ERROR: OptionSettings=(...) block not found in config file.", file=sys.stderr)
        sys.exit(1)

    return set(re.findall(r'\b([A-Za-z_][A-Za-z0-9_]*)=', match.group(1)))


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

def build_summary(
    cs_count:     int,
    cfg_count:    int,
    unhandled:    list[str],
    stale:        list[str],
) -> str:
    lines: list[str] = []

    lines.append("## Palworld Config Drift Report\n\n")
    lines.append("| | Count |\n|---|---|\n")
    lines.append(f"| Settings in `Program.cs` | **{cs_count}** |\n")
    lines.append(f"| Settings in `DefaultPalWorldSettings.ini` | **{cfg_count}** |\n")
    lines.append(f"| + Unhandled (config -> code) | **{len(unhandled)}** |\n")
    lines.append(f"| - Stale (code -> config) | **{len(stale)}** |\n\n")

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
            "in `Program.cs`. Consider adding support for them:\n\n"
        )
        lines.append("| Setting |\n|---|\n")
        lines.extend(f"| `{s}` |\n" for s in unhandled)
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

    # -- Compute the two-way diff ----------------------------------------------
    unhandled = sorted(config_settings - cs_settings)  # need to add to code
    stale     = sorted(cs_settings - config_settings)  # possibly removed from game

    has_drift = bool(unhandled or stale)

    # -- Console / runner log output -------------------------------------------
    print(f"Settings in Program.cs               : {len(cs_settings)}")
    print(f"Settings in DefaultPalWorldSettings  : {len(config_settings)}")
    print()

    if unhandled:
        print(f"[WARN] {len(unhandled)} setting(s) in config but NOT in code (need implementation):")
        for s in unhandled:
            print(f"         +  {s}")
    else:
        print("[OK]   All config settings are handled in code.")

    print()

    if stale:
        print(f"[WARN] {len(stale)} setting(s) in code but NOT in config (possibly renamed/removed):")
        for s in stale:
            print(f"         -  {s}")
    else:
        print("[OK]   All code settings are present in the config.")

    # -- Expose drift flag as a step output for the workflow -------------------
    set_output("has_drift", "true" if has_drift else "false")
    set_output("unhandled_count", str(len(unhandled)))
    set_output("stale_count",     str(len(stale)))

    # -- Write the GitHub Step Summary -----------------------------------------
    append_summary(
        build_summary(len(cs_settings), len(config_settings), unhandled, stale)
    )

    return 1 if has_drift else 0


if __name__ == "__main__":
    sys.exit(main())
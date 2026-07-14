#!/usr/bin/env python3
"""
Regenerates the environment variable settings table in README.md
from the SettingValue definitions in Program.cs.
"""

import re
import sys
from collections import defaultdict
from pathlib import Path

# -- Paths --------------------------------------------------------------------
PROGRAM_CS = Path("PalworldConfigurationParser/Program.cs")
README_MD  = Path("README.md")

START_MARKER = "<!-- ENV-SETTINGS-START -->"
END_MARKER   = "<!-- ENV-SETTINGS-END -->"

# -- Section config (order matters) -------------------------------------------
# (SettingTypes enum value, heading, optional description block)
TYPE_CONFIG = [
    (
        "BrowserDisplay",
        "### BrowserDisplay",
        (
            'These are like strings, except they have special logic that allows them to support `"` and `|` in the values.  \n'
            "This allows a unique display on the server list as these values are not normally allowed due to the syntax of the config options."
        ),
    ),
    ("String", "### Text", None),
    (
        "AlphaDash",
        "### Passwords",
        (
            "Passwords are restricted to alphanumeric characters, dashes and underscores\n"
            "(1–30 characters). Special characters and spaces are rejected, as they can break\n"
            "in-game chat auth and RCON."
        ),
    ),
    ("Integer", "### Integers", None),
    ("Float",   "### Floats",   None),
    (
        "Boolean",
        "### Booleans",
        "Accepts `true|1` / `false|0` (case-insensitive).",
    ),
    (
        "PlatformList",
        "### Platform list",
        (
            "Accepts a comma-separated list of platforms (`Steam`, `Xbox`, `PS5`, `Mac`),\n"
            "with or without parentheses or quotes, in any casing/order. At least one valid\n"
            "platform is required."
        ),
    ),
]

# -- Parser --------------------------------------------------------------------
SETTING_RE = re.compile(
    r'new\s+SettingValue\(\s*"([^"]+)"\s*,\s*SettingTypes\.(\w+)'
    r'(?:\s*,\s*(?:true|false))?'       # optional IsSecret / similar bool param
    r'\s*,\s*EnvVars:\s*\[([^\]]+)\]'  # EnvVars: ["A", "B", ...]
    r'\s*\)',
    re.DOTALL,
)


def parse_settings(source: str) -> dict[str, list[tuple[str, list[str]]]]:
    groups: dict[str, list[tuple[str, list[str]]]] = defaultdict(list)
    for name, setting_type, env_vars_raw in SETTING_RE.findall(source):
        env_vars = [v.strip().strip('"') for v in env_vars_raw.split(",") if v.strip()]
        groups[setting_type].append((name, env_vars))
    return groups


# -- Generator -----------------------------------------------------------------
def generate_section(groups: dict[str, list[tuple[str, list[str]]]]) -> str:
    # Warn about types in the code that aren't in TYPE_CONFIG
    known = {t for t, *_ in TYPE_CONFIG}
    for unknown_type in set(groups) - known:
        print(
            f"WARNING: SettingTypes.{unknown_type} has no entry in TYPE_CONFIG "
            "and will be omitted from the README.",
            file=sys.stderr,
        )

    parts: list[str] = []
    for type_key, heading, description in TYPE_CONFIG:
        if type_key not in groups:
            continue

        parts.append(heading)
        parts.append("")
        if description:
            parts.append(description)
            parts.append("")
        parts.append("| Environment variable | Setting |")
        parts.append("|---|---|")
        for name, env_vars in groups[type_key]:
            # Only the canonical / primary env var is shown in the table
            parts.append(f"| `{env_vars[0]}` | {name} |")
        parts.append("")  # blank line between sections

    return "\n".join(parts)


# -- README updater ------------------------------------------------------------
def update_readme(readme: str, section: str) -> str:
    start_idx = readme.find(START_MARKER)
    end_idx   = readme.find(END_MARKER)

    if start_idx == -1:
        raise ValueError(f"Start marker not found: {START_MARKER!r}")
    if end_idx == -1:
        raise ValueError(f"End marker not found: {END_MARKER!r}")
    if end_idx <= start_idx:
        raise ValueError("End marker must come after start marker")

    return (
        readme[: start_idx + len(START_MARKER)]
        + "\n"
        + section
        + readme[end_idx:]
    )


# -- Entry point ---------------------------------------------------------------
def main() -> None:
    if not PROGRAM_CS.exists():
        print(f"ERROR: {PROGRAM_CS} not found.", file=sys.stderr)
        sys.exit(1)

    source = PROGRAM_CS.read_text(encoding="utf-8")
    groups = parse_settings(source)

    if not groups:
        print(
            "ERROR: No SettingValue entries parsed. "
            "Check PROGRAM_CS path and regex.",
            file=sys.stderr,
        )
        sys.exit(1)

    section    = generate_section(groups)
    readme     = README_MD.read_text(encoding="utf-8")
    new_readme = update_readme(readme, section)

    if new_readme == readme:
        print("README.md is already up to date — nothing to commit.")
        return

    README_MD.write_text(new_readme, encoding="utf-8")
    print("README.md updated successfully.")


if __name__ == "__main__":
    main()
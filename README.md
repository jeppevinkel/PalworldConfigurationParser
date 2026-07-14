# Palworld Configuration Parser

A small .NET command-line tool that updates a Palworld dedicated server's
`PalWorldSettings.ini` from environment variables. It's designed for
containerized / scripted server deployments, where you'd rather configure the
server through environment variables than hand-edit the ini file.

The tool parses the `OptionSettings=(...)` line, applies any overrides it finds
in the environment, and writes the file back — preserving the original ordering
of settings and any other lines in the file. Values are validated and type-checked
before being written, so a malformed value fails loudly instead of corrupting the
config.

This tool is inspired by
[pelican-eggs/Palworld-Config-Parser-Tool](https://github.com/pelican-eggs/Palworld-Config-Parser-Tool)
and supports most of the same environment variables.

## Usage

The project targets **.NET 10**.

```bash
# Build
dotnet build

# Run against the default file (./PalWorldSettings.ini)
dotnet run --project PalworldConfigurationParser

# Or point it at a specific settings file
dotnet run --project PalworldConfigurationParser -- /path/to/PalWorldSettings.ini

# Show help
dotnet run --project PalworldConfigurationParser -- --help
```

If no path is given, the tool defaults to `PalWorldSettings.ini` in the current
working directory. Set whichever environment variables you want to override
before running it — every recognized variable that is set will be applied, and
the rest are left untouched. On completion it prints a summary of how many
settings were changed, left unchanged, or not set.

Example:

```bash
export SERVER_NAME="My Palworld Server"
export MAX_PLAYERS=32
export EXP_RATE=2.0
export IS_PVP=true

dotnet run --project PalworldConfigurationParser -- /palworld/PalWorldSettings.ini
```

## Supported environment variables

Each setting below can be overridden by the listed environment variable. Some
settings also accept alternative variable names for compatibility, but only the
primary one is shown here.

<!-- ENV-SETTINGS-START -->
### BrowserDisplay

These are like strings, except they have special logic that allows them to support `"` and `|` in the values.  
This allows a unique display on the server list as these values are not normally allowed due to the syntax of the config options.

| Environment variable | Setting |
|---|---|
| `SERVER_NAME` | ServerName |
| `SERVER_DESCRIPTION` | ServerDescription |

### Text

| Environment variable | Setting |
|---|---|
| `DIFFICULTY` | Difficulty |
| `DEATH_PENALTY` | DeathPenalty |
| `PUBLIC_IP` | PublicIP |
| `BAN_LIST_URL` | BanListURL |
| `REGION` | Region |
| `LOG_FORMAT_TYPE` | LogFormatType |
| `RANDOMIZER_TYPE` | RandomizerType |
| `RANDOMIZER_SEED` | RandomizerSeed |

### Passwords

Passwords are restricted to alphanumeric characters, dashes and underscores
(1–30 characters). Special characters and spaces are rejected, as they can break
in-game chat auth and RCON.

| Environment variable | Setting |
|---|---|
| `SERVER_PASSWORD` | ServerPassword |
| `ADMIN_PASSWORD` | AdminPassword |

### Integers

| Environment variable | Setting |
|---|---|
| `DROP_ITEM_MAX_NUM` | DropItemMaxNum |
| `DROP_ITEM_MAX_NUM_UNKO` | DropItemMaxNum_UNKO |
| `BASE_CAMP_MAX_NUM` | BaseCampMaxNum |
| `BASE_CAMP_WORKER_MAX_NUM` | BaseCampWorkerMaxNum |
| `GUILD_PLAYER_MAX_NUM` | GuildPlayerMaxNum |
| `BASE_CAMP_MAX_NUM_IN_GUILD` | BaseCampMaxNumInGuild |
| `COOP_PLAYER_MAX_NUM` | CoopPlayerMaxNum |
| `SERVER_PLAYER_MAX_NUM` | ServerPlayerMaxNum |
| `PUBLIC_PORT` | PublicPort |
| `RCON_PORT` | RCONPort |
| `REST_API_PORT` | RESTAPIPort |
| `SUPPLY_DROP_SPAN` | SupplyDropSpan |
| `CHAT_POST_LIMIT_PER_MINUTE` | ChatPostLimitPerMinute |
| `MAX_BUILDING_LIMIT_NUM` | MaxBuildingLimitNum |

### Floats

| Environment variable | Setting |
|---|---|
| `DAY_TIME_SPEED_RATE` | DayTimeSpeedRate |
| `NIGHT_TIME_SPEED_RATE` | NightTimeSpeedRate |
| `EXP_RATE` | ExpRate |
| `PAL_CAPTURE_RATE` | PalCaptureRate |
| `PAL_SPAWN_NUM_RATE` | PalSpawnNumRate |
| `PAL_DAMAGE_RATE_ATTACK` | PalDamageRateAttack |
| `PAL_DAMAGE_RATE_DEFENSE` | PalDamageRateDefense |
| `PLAYER_DAMAGE_RATE_ATTACK` | PlayerDamageRateAttack |
| `PLAYER_DAMAGE_RATE_DEFENSE` | PlayerDamageRateDefense |
| `PLAYER_STOMACH_DECREACE_RATE` | PlayerStomachDecreaceRate |
| `PLAYER_STAMINA_DECREACE_RATE` | PlayerStaminaDecreaceRate |
| `PLAYER_AUTO_HP_REGEN_RATE` | PlayerAutoHPRegeneRate |
| `PLAYER_AUTO_HP_REGEN_RATE_IN_SLEEP` | PlayerAutoHpRegeneRateInSleep |
| `PAL_STAMINA_DECREACE_RATE` | PalStaminaDecreaceRate |
| `PAL_STOMACH_DECREACE_RATE` | PalStomachDecreaceRate |
| `PAL_AUTO_HP_REGEN_RATE` | PalAutoHPRegeneRate |
| `PAL_AUTO_HP_REGEN_RATE_IN_SLEEP` | PalAutoHpRegeneRateInSleep |
| `BUILD_OBJECT_DAMAGE_RATE` | BuildObjectDamageRate |
| `BUILD_OBJECT_DETERIORATION_DAMAGE_RATE` | BuildObjectDeteriorationDamageRate |
| `BUILD_OBJECT_HP_RATE` | BuildObjectHpRate |
| `COLLECTION_DROP_RATE` | CollectionDropRate |
| `COLLECTION_OBJECT_HP_RATE` | CollectionObjectHpRate |
| `COLLECTION_OBJECT_RESPAWN_SPEED_RATE` | CollectionObjectRespawnSpeedRate |
| `ENEMY_DROP_ITEM_RATE` | EnemyDropItemRate |
| `DROP_ITEM_ALIVE_MAX_HOURS` | DropItemAliveMaxHours |
| `AUTO_RESET_GUILD_TIME_NO_ONLINE_PLAYERS` | AutoResetGuildTimeNoOnlinePlayers |
| `PAL_EGG_DEFAULT_HATCHING_TIME` | PalEggDefaultHatchingTime |
| `WORK_SPEED_RATE` | WorkSpeedRate |
| `ITEM_WEIGHT_RATE` | ItemWeightRate |
| `SERVER_REPLICATE_PAWN_CULL_DISTANCE` | ServerReplicatePawnCullDistance |
| `EQUIPMENT_DURABILITY_DAMAGE_RATE` | EquipmentDurabilityDamageRate |
| `ITEM_CONTAINER_FORCE_MARK_DIRTY_INTERVAL` | ItemContainerForceMarkDirtyInterval |
| `ITEM_CORRUPTION_MULTIPLIER` | ItemCorruptionMultiplier |
| `AUTO_SAVE_SPAN` | AutoSaveSpan |
| `VOICE_CHAT_MAX_VOLUME_DISTANCE` | VoiceChatMaxVolumeDistance |
| `VOICE_CHAT_ZERO_VOLUME_DISTANCE` | VoiceChatZeroVolumeDistance |

### Booleans

Accepts `true|1` / `false|0` (case-insensitive).

| Environment variable | Setting |
|---|---|
| `ENABLE_PLAYER_TO_PLAYER_DAMAGE` | bEnablePlayerToPlayerDamage |
| `ENABLE_FRIENDLY_FIRE` | bEnableFriendlyFire |
| `ENABLE_INVADER_ENEMY` | bEnableInvaderEnemy |
| `ACTIVE_UNKO` | bActiveUNKO |
| `ENABLE_AIM_ASSIST_PAD` | bEnableAimAssistPad |
| `ENABLE_AIM_ASSIST_KEYBOARD` | bEnableAimAssistKeyboard |
| `AUTO_RESET_GUILD_NO_ONLINE_PLAYERS` | bAutoResetGuildNoOnlinePlayers |
| `IS_MULTIPLAY` | bIsMultiplay |
| `IS_PVP` | bIsPvP |
| `CAN_PICKUP_OTHER_GUILD_DEATH_PENALTY_DROP` | bCanPickupOtherGuildDeathPenaltyDrop |
| `ENABLE_NON_LOGIN_PENALTY` | bEnableNonLoginPenalty |
| `ENABLE_FAST_TRAVEL` | bEnableFastTravel |
| `IS_START_LOCATION_SELECT_BY_MAP` | bIsStartLocationSelectByMap |
| `EXIST_PLAYER_AFTER_LOGOUT` | bExistPlayerAfterLogout |
| `ENABLE_DEFENSE_OTHER_GUILD_PLAYER` | bEnableDefenseOtherGuildPlayer |
| `RCON_ENABLED` | RCONEnabled |
| `USE_AUTH` | bUseAuth |
| `SHOW_PLAYER_LIST` | bShowPlayerList |
| `REST_API_ENABLED` | RESTAPIEnabled |
| `IS_USE_BACKUP_SAVE_DATA` | bIsUseBackupSaveData |
| `INVISIBLE_OTHER_GUILD_BASE_CAMP_AREA_FX` | bInvisibleOtherGuildBaseCampAreaFX |
| `HARDCORE` | bHardcore |
| `PAL_LOST` | bPalLost |
| `BUILD_AREA_LIMIT` | bBuildAreaLimit |
| `ENABLE_PREDATOR_BOSS_PAL` | EnablePredatorBossPal |
| `IS_RANDOMIZER_PAL_LEVEL_RANDOM` | bIsRandomizerPalLevelRandom |
| `ALLOW_GLOBAL_PALBOX_EXPORT` | bAllowGlobalPalboxExport |
| `ALLOW_GLOBAL_PALBOX_IMPORT` | bAllowGlobalPalboxImport |
| `CHARACTER_RECREATE_IN_HARDCORE` | bCharacterRecreateInHardcore |
| `ENABLE_VOICE_CHAT` | bEnableVoiceChat |

### Platform list

Accepts a comma-separated list of platforms (`Steam`, `Xbox`, `PS5`, `Mac`),
with or without parentheses or quotes, in any casing/order. At least one valid
platform is required.

| Environment variable | Setting |
|---|---|
| `CROSSPLAY_PLATFORMS` | CrossplayPlatforms |
<!-- ENV-SETTINGS-END -->

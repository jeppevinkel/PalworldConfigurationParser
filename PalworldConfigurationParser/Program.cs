using System.Reflection;
using PalworldConfigurationParser;
using PalworldConfigurationParser.Models;

List<SettingValue> supportedSettings = [
    // String Settings
    new SettingValue("ServerName", SettingTypes.BrowserDisplay, EnvVars: ["SERVER_NAME"]),
    new SettingValue("ServerDescription", SettingTypes.BrowserDisplay, EnvVars: ["SERVER_DESCRIPTION"]),
    
    // String Settings
    new SettingValue("Difficulty", SettingTypes.String, EnvVars: ["DIFFICULTY"]),
    new SettingValue("DeathPenalty", SettingTypes.String, EnvVars: ["DEATH_PENALTY"]),
    new SettingValue("PublicIP", SettingTypes.String, EnvVars: ["PUBLIC_IP"]),
    new SettingValue("BanListURL", SettingTypes.String, EnvVars: ["BAN_LIST_URL"]),
    new SettingValue("Region", SettingTypes.String, EnvVars: ["REGION"]),
    new SettingValue("LogFormatType", SettingTypes.String, EnvVars: ["LOG_FORMAT_TYPE"]),
    new SettingValue("RandomizerType", SettingTypes.String, EnvVars: ["RANDOMIZER_TYPE"]),
    new SettingValue("RandomizerSeed", SettingTypes.String, EnvVars: ["RANDOMIZER_SEED"]),
    
    // AlphaDash Settings
    new SettingValue("ServerPassword", SettingTypes.AlphaDash, true, EnvVars: ["SERVER_PASSWORD"]),
    new SettingValue("AdminPassword", SettingTypes.AlphaDash, true, EnvVars: ["ADMIN_PASSWORD"]),
    
    // Integer Settings
    new SettingValue("DropItemMaxNum", SettingTypes.Integer, EnvVars: ["DROP_ITEM_MAX_NUM"]),
    new SettingValue("DropItemMaxNum_UNKO", SettingTypes.Integer, EnvVars: ["DROP_ITEM_MAX_NUM_UNKO"]),
    new SettingValue("BaseCampMaxNum", SettingTypes.Integer, EnvVars: ["BASE_CAMP_MAX_NUM"]),
    new SettingValue("BaseCampWorkerMaxNum", SettingTypes.Integer, EnvVars: ["BASE_CAMP_WORKER_MAX_NUM"]),
    new SettingValue("GuildPlayerMaxNum", SettingTypes.Integer, EnvVars: ["GUILD_PLAYER_MAX_NUM"]),
    new SettingValue("BaseCampMaxNumInGuild", SettingTypes.Integer, EnvVars: ["BASE_CAMP_MAX_NUM_IN_GUILD"]),
    new SettingValue("CoopPlayerMaxNum", SettingTypes.Integer, EnvVars: ["COOP_PLAYER_MAX_NUM"]),
    new SettingValue("ServerPlayerMaxNum", SettingTypes.Integer, EnvVars: ["SERVER_PLAYER_MAX_NUM", "MAX_PLAYERS"]),
    new SettingValue("PublicPort", SettingTypes.Integer, EnvVars: ["PUBLIC_PORT", "SERVER_PORT"]),
    new SettingValue("RCONPort", SettingTypes.Integer, EnvVars: ["RCON_PORT"]),
    new SettingValue("RESTAPIPort", SettingTypes.Integer, EnvVars: ["REST_API_PORT"]),
    new SettingValue("SupplyDropSpan", SettingTypes.Integer, EnvVars: ["SUPPLY_DROP_SPAN"]),
    new SettingValue("ChatPostLimitPerMinute", SettingTypes.Integer, EnvVars: ["CHAT_POST_LIMIT_PER_MINUTE", "CHAT_POST_LIMIT"]),
    new SettingValue("AutoSaveSpan", SettingTypes.Integer, EnvVars: ["AUTO_SAVE_SPAN"]),
    new SettingValue("MaxBuildingLimitNum", SettingTypes.Integer, EnvVars: ["MAX_BUILDING_LIMIT_NUM"]),
    
    // Float Settings
    new SettingValue("DayTimeSpeedRate", SettingTypes.Float, EnvVars: ["DAY_TIME_SPEED_RATE"]),
    new SettingValue("NightTimeSpeedRate", SettingTypes.Float, EnvVars: ["NIGHT_TIME_SPEED_RATE"]),
    new SettingValue("ExpRate", SettingTypes.Float, EnvVars: ["EXP_RATE"]),
    new SettingValue("PalCaptureRate", SettingTypes.Float, EnvVars: ["PAL_CAPTURE_RATE"]),
    new SettingValue("PalSpawnNumRate", SettingTypes.Float, EnvVars: ["PAL_SPAWN_NUM_RATE"]),
    new SettingValue("PalDamageRateAttack", SettingTypes.Float, EnvVars: ["PAL_DAMAGE_RATE_ATTACK"]),
    new SettingValue("PalDamageRateDefense", SettingTypes.Float, EnvVars: ["PAL_DAMAGE_RATE_DEFENSE"]),
    new SettingValue("PlayerDamageRateAttack", SettingTypes.Float, EnvVars: ["PLAYER_DAMAGE_RATE_ATTACK"]),
    new SettingValue("PlayerDamageRateDefense", SettingTypes.Float, EnvVars: ["PLAYER_DAMAGE_RATE_DEFENSE"]),
    new SettingValue("PlayerStomachDecreaceRate", SettingTypes.Float, EnvVars: ["PLAYER_STOMACH_DECREACE_RATE"]),
    new SettingValue("PlayerStaminaDecreaceRate", SettingTypes.Float, EnvVars: ["PLAYER_STAMINA_DECREACE_RATE"]),
    new SettingValue("PlayerAutoHPRegeneRate", SettingTypes.Float, EnvVars: ["PLAYER_AUTO_HP_REGEN_RATE", "PLAYER_AUTO_HP_REGENE_RATE"]),
    new SettingValue("PlayerAutoHpRegeneRateInSleep", SettingTypes.Float, EnvVars: ["PLAYER_AUTO_HP_REGEN_RATE_IN_SLEEP", "PLAYER_AUTO_HP_REGENE_RATE_IN_SLEEP"]),
    new SettingValue("PalStaminaDecreaceRate", SettingTypes.Float, EnvVars: ["PAL_STAMINA_DECREACE_RATE"]),
    new SettingValue("PalStomachDecreaceRate", SettingTypes.Float, EnvVars: ["PAL_STOMACH_DECREACE_RATE"]),
    new SettingValue("PalAutoHPRegeneRate", SettingTypes.Float, EnvVars: ["PAL_AUTO_HP_REGEN_RATE", "PAL_AUTO_HP_REGENE_RATE"]),
    new SettingValue("PalAutoHpRegeneRateInSleep", SettingTypes.Float, EnvVars: ["PAL_AUTO_HP_REGEN_RATE_IN_SLEEP", "PAL_AUTO_HP_REGENE_RATE_IN_SLEEP"]),
    new SettingValue("BuildObjectDamageRate", SettingTypes.Float, EnvVars: ["BUILD_OBJECT_DAMAGE_RATE"]),
    new SettingValue("BuildObjectDeteriorationDamageRate", SettingTypes.Float, EnvVars: ["BUILD_OBJECT_DETERIORATION_DAMAGE_RATE"]),
    new SettingValue("BuildObjectHpRate", SettingTypes.Float, EnvVars: ["BUILD_OBJECT_HP_RATE"]),
    new SettingValue("CollectionDropRate", SettingTypes.Float, EnvVars: ["COLLECTION_DROP_RATE"]),
    new SettingValue("CollectionObjectHpRate", SettingTypes.Float, EnvVars: ["COLLECTION_OBJECT_HP_RATE"]),
    new SettingValue("CollectionObjectRespawnSpeedRate", SettingTypes.Float, EnvVars: ["COLLECTION_OBJECT_RESPAWN_SPEED_RATE"]),
    new SettingValue("EnemyDropItemRate", SettingTypes.Float, EnvVars: ["ENEMY_DROP_ITEM_RATE"]),
    new SettingValue("DropItemAliveMaxHours", SettingTypes.Float, EnvVars: ["DROP_ITEM_ALIVE_MAX_HOURS"]),
    new SettingValue("AutoResetGuildTimeNoOnlinePlayers", SettingTypes.Float, EnvVars: ["AUTO_RESET_GUILD_TIME_NO_ONLINE_PLAYERS"]),
    new SettingValue("PalEggDefaultHatchingTime", SettingTypes.Float, EnvVars: ["PAL_EGG_DEFAULT_HATCHING_TIME"]),
    new SettingValue("WorkSpeedRate", SettingTypes.Float, EnvVars: ["WORK_SPEED_RATE"]),
    new SettingValue("ItemWeightRate", SettingTypes.Float, EnvVars: ["ITEM_WEIGHT_RATE"]),
    new SettingValue("ServerReplicatePawnCullDistance", SettingTypes.Float, EnvVars: ["SERVER_REPLICATE_PAWN_CULL_DISTANCE"]),
    new SettingValue("EquipmentDurabilityDamageRate", SettingTypes.Float, EnvVars: ["EQUIPMENT_DURABILITY_DAMAGE_RATE"]),
    new SettingValue("ItemContainerForceMarkDirtyInterval", SettingTypes.Float, EnvVars: ["ITEM_CONTAINER_FORCE_MARK_DIRTY_INTERVAL"]),
    new SettingValue("ItemCorruptionMultiplier", SettingTypes.Float, EnvVars: ["ITEM_CORRUPTION_MULTIPLIER"]),
    
    // Boolean Settings
    new SettingValue("bEnablePlayerToPlayerDamage", SettingTypes.Boolean, EnvVars: ["ENABLE_PLAYER_TO_PLAYER_DAMAGE"]),
    new SettingValue("bEnableFriendlyFire", SettingTypes.Boolean, EnvVars: ["ENABLE_FRIENDLY_FIRE"]),
    new SettingValue("bEnableInvaderEnemy", SettingTypes.Boolean, EnvVars: ["ENABLE_INVADER_ENEMY", "ENABLE_ENEMY"]),
    new SettingValue("bActiveUNKO", SettingTypes.Boolean, EnvVars: ["ACTIVE_UNKO"]),
    new SettingValue("bEnableAimAssistPad", SettingTypes.Boolean, EnvVars: ["ENABLE_AIM_ASSIST_PAD"]),
    new SettingValue("bEnableAimAssistKeyboard", SettingTypes.Boolean, EnvVars: ["ENABLE_AIM_ASSIST_KEYBOARD"]),
    new SettingValue("bAutoResetGuildNoOnlinePlayers", SettingTypes.Boolean, EnvVars: ["AUTO_RESET_GUILD_NO_ONLINE_PLAYERS"]),
    new SettingValue("bIsMultiplay", SettingTypes.Boolean, EnvVars: ["IS_MULTIPLAY"]),
    new SettingValue("bIsPvP", SettingTypes.Boolean, EnvVars: ["IS_PVP"]),
    new SettingValue("bCanPickupOtherGuildDeathPenaltyDrop", SettingTypes.Boolean, EnvVars: ["CAN_PICKUP_OTHER_GUILD_DEATH_PENALTY_DROP"]),
    new SettingValue("bEnableNonLoginPenalty", SettingTypes.Boolean, EnvVars: ["ENABLE_NON_LOGIN_PENALTY"]),
    new SettingValue("bEnableFastTravel", SettingTypes.Boolean, EnvVars: ["ENABLE_FAST_TRAVEL"]),
    new SettingValue("bIsStartLocationSelectByMap", SettingTypes.Boolean, EnvVars: ["IS_START_LOCATION_SELECT_BY_MAP"]),
    new SettingValue("bExistPlayerAfterLogout", SettingTypes.Boolean, EnvVars: ["EXIST_PLAYER_AFTER_LOGOUT"]),
    new SettingValue("bEnableDefenseOtherGuildPlayer", SettingTypes.Boolean, EnvVars: ["ENABLE_DEFENSE_OTHER_GUILD_PLAYER"]),
    new SettingValue("RCONEnabled", SettingTypes.Boolean, EnvVars: ["RCON_ENABLED", "RCON_ENABLE"]),
    new SettingValue("bUseAuth", SettingTypes.Boolean, EnvVars: ["USE_AUTH"]),
    new SettingValue("bShowPlayerList", SettingTypes.Boolean, EnvVars: ["SHOW_PLAYER_LIST"]),
    new SettingValue("RESTAPIEnabled", SettingTypes.Boolean, EnvVars: ["REST_API_ENABLED"]),
    new SettingValue("bIsUseBackupSaveData", SettingTypes.Boolean, EnvVars: ["IS_USE_BACKUP_SAVE_DATA", "USE_BACKUP_SAVE_DATA"]),
    new SettingValue("bInvisibleOtherGuildBaseCampAreaFX", SettingTypes.Boolean, EnvVars: ["INVISIBLE_OTHER_GUILD_BASE_CAMP_AREA_FX", "INVISIBLE_OTHER_GUILD_BASE"]),
    new SettingValue("bHardcore", SettingTypes.Boolean, EnvVars: ["HARDCORE"]),
    new SettingValue("bPalLost", SettingTypes.Boolean, EnvVars: ["PAL_LOST"]),
    new SettingValue("bBuildAreaLimit", SettingTypes.Boolean, EnvVars: ["BUILD_AREA_LIMIT"]),
    new SettingValue("EnablePredatorBossPal", SettingTypes.Boolean, EnvVars: ["ENABLE_PREDATOR_BOSS_PAL"]),
    new SettingValue("bIsRandomizerPalLevelRandom", SettingTypes.Boolean, EnvVars: ["IS_RANDOMIZER_PAL_LEVEL_RANDOM"]),
    new SettingValue("bAllowGlobalPalboxExport", SettingTypes.Boolean, EnvVars: ["ALLOW_GLOBAL_PALBOX_EXPORT"]),
    new SettingValue("bAllowGlobalPalboxImport", SettingTypes.Boolean, EnvVars: ["ALLOW_GLOBAL_PALBOX_IMPORT"]),
    new SettingValue("bCharacterRecreateInHardcore", SettingTypes.Boolean, EnvVars: ["CHARACTER_RECREATE_IN_HARDCORE"]),
    
    // PlatformList Settings
    new SettingValue("CrossplayPlatforms", SettingTypes.PlatformList, EnvVars: ["CROSSPLAY_PLATFORMS"]),
];

var version = Assembly.GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion ?? "Unknown";

Console.WriteLine($"PalworldConfigurationParser Version: {version}\n");

string path;

if (args.Length > 0)
{
    if (args[0] == "?" || args[0] == "-h" || args[0] == "--help")
    {
        Console.WriteLine("Usage: PalworldConfigurationParser [path_to_settings_file]");
        return;
    }

    path = string.Join(" ", args);
}
else
{
    path = ConfigLocator.GetDefaultLocation();
}

var settings = PalworldSettingsFile.Load(path);

SettingsUpdater.ApplyEnvironmentOverrides(settings, supportedSettings);

settings.Save(path);
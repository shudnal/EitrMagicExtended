using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace EitrMagicExtended
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class EitrMagicExtended : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.EitrMagicExtended";
        public const string pluginName = "Eitr Magic Extended";
        public const string pluginVersion = "1.0.2";

        private readonly Harmony harmony = new Harmony(pluginID);

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        public static ConfigEntry<bool> configLocked;

        public static ConfigEntry<bool> loggingEnabled;

        public static ConfigEntry<bool> linearRegeneration;
        public static ConfigEntry<float> linearRegenerationMultiplier;
        public static ConfigEntry<float> linearRegenerationThreshold;

        public static ConfigEntry<bool> extraEitrRegeneration;
        public static ConfigEntry<float> extraEitrRegenerationPercent;
        public static ConfigEntry<int> extraEitrRegenerationPoints;
        public static ConfigEntry<bool> extraEitrRegenerationOnlyFood;

        public static ConfigEntry<bool> baseEitr;
        public static ConfigEntry<float> elementalMagicBaseEitrIncrease;
        public static ConfigEntry<float> bloodMagicBaseEitrIncrease;
        public static ConfigEntry<float> baseEitrRegen;
        public static ConfigEntry<float> baseEitrRegenDelay;

        public static ConfigEntry<bool> baseEitrNonLinear;
        public static ConfigEntry<float> baseEitrElementalMagicPower;
        public static ConfigEntry<float> baseEitrElementalMagicCoefficient;
        public static ConfigEntry<float> baseEitrBloodMagicPower;
        public static ConfigEntry<float> baseEitrBloodMagicCoefficient;

        public static ConfigEntry<bool> preventZeroDamageShieldSpam;
        public static ConfigEntry<bool> preventLookVectorConsoleSpam;
        public static ConfigEntry<bool> changeShieldColorByHealth;
        public static ConfigEntry<Color> shieldTargetColorZeroHealth;
        public static ConfigEntry<bool> addShieldStaffSecondaryAttack;
        public static ConfigEntry<StaffShieldPatches.HitType> shieldFullProtectionFrom;

        public static ConfigEntry<bool> hideEitrValue;

        public static EitrMagicExtended instance;

        private void Awake()
        {
            harmony.PatchAll();
            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            Game.isModded = true;
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            instance = null;
        }

        private void ConfigInit()
        {
            config("1 - General", "NexusID", 2961, "Nexus mod ID for updates", false);

            configLocked = config("1 - General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only.");
            loggingEnabled = config("1 - General", "Logging enabled", false, "Enable logging. [Not Synced with Server]", false);

            linearRegeneration = config("2 - Linear regeneration change", "Enabled", true, "Enable linear change of eitr regeneration rate. Overall time to regenerate eitr to 100% is still almost the same.");
            linearRegenerationMultiplier = config("2 - Linear regeneration change", "Multiplier", 3f, "Multiplier of regeneration rate when eitr is 0." +
                                                                                                "\nIf value is above 1. Eitr will regenerate faster at lower values and proportionally slower at higher values." +
                                                                                                "\nIf value is below 1. Eitr will regenerate slower at lower values and proportionally higher at higher values.");
            linearRegenerationThreshold = config("2 - Linear regeneration change", "Regeneration threshold", 0.5f, "Inflection point of eitr regeneration rate. Eitr regeneration rate is normal only in that point." +
                                                                                                                    "\nIn that point regeneration rate changes its sign." +
                                                                                                                    "\nIf set value is outside of 0-1 range eitr will regenerate normally");

            extraEitrRegeneration = config("3 - Extra eitr regeneration", "Enabled", true, "Enable increased eitr regeneration by X% per every Y point of additional eitr");
            extraEitrRegenerationPercent = config("3 - Extra eitr regeneration", "Percent", 1f, "Eitr regeneration increased by X%");
            extraEitrRegenerationPoints = config("3 - Extra eitr regeneration", "Points", 5, "Eitr regeneration increased per every Y points of additional eitr");
            extraEitrRegenerationOnlyFood = config("3 - Extra eitr regeneration", "Eitr from food only", true, "Only eitr gain from food is counted for increased regeneration");

            baseEitr = config("4 - Base eitr", "Enabled", true, "Base eitr will be increased proportionally to skills where set value reached at skill level 100.");
            elementalMagicBaseEitrIncrease = config("4 - Base eitr", "Elemental Magic", 40f, "Base eitr will be increased by set value when Elemental Magic skill is level 100");
            bloodMagicBaseEitrIncrease = config("4 - Base eitr", "Blood Magic", 40f, "Base eitr will be increased by set value when Blood Magic skill is level 100");
            baseEitrRegen = config("4 - Base eitr", "Base Regeneration rate", 2f, "Basic eitr regeneration rate per second.");
            baseEitrRegenDelay = config("4 - Base eitr", "Base Regeneration delay", 2f, "Delay amount before eitr regeneration starts.");

            baseEitrNonLinear = config("4 - Base eitr - Non linear", "Enabled", false, "Base eitr will increase using function X * (skill ^ Y) \"X multiplied by skill raised to the power of Y\"");
            baseEitrElementalMagicCoefficient = config("4 - Base eitr - Non linear", "Elemental Magic Coefficient", 3f, "X in formula");
            baseEitrElementalMagicPower = config("4 - Base eitr - Non linear", "Elemental Magic Power", 0.5f, "Y in formula");
            baseEitrBloodMagicCoefficient = config("4 - Base eitr - Non linear", "Blood Magic Coefficient", 3f, "X in formula");
            baseEitrBloodMagicPower = config("4 - Base eitr - Non linear", "Blood Magic Power", 0.5f, "Y in formula");

            preventZeroDamageShieldSpam = config("5 - Shield", "Prevent zero damage hit effect", true, "If you take 0 damage shield not play hit effect. Could prevent occasional hit effect spam.");
            preventLookVectorConsoleSpam = config("5 - Shield", "Prevent Look Rotation Viewing Vector Is Zero console message", true, "If shield takes damage from an indirect hit this message will no longer be shown in log file or console.");
            changeShieldColorByHealth = config("5 - Shield", "Change shield color depending on capacity", true, "Gradually changes shield color tint to different color depending on its current capacity.");
            shieldTargetColorZeroHealth = config("5 - Shield", "Change shield color at zero health to", new Color(1f, 0f, 1f, 0.5f), "Shield will have this color at 0 health.");
            addShieldStaffSecondaryAttack = config("5 - Shield", "Add shield staff secondary attack to break shield", true, "Shield staff can now break current shield with secondary attack. No skill point obtained this way.");
            shieldFullProtectionFrom = config("5 - Shield", "Shield grants full protection from", StaffShieldPatches.HitType.None, "Shield will absorb all damage from given hit type and completely prevent health damage.");

            hideEitrValue = config("6 - Misc", "Hide eitr bar text", false, "Hide eitr text value on eitr bar");
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        public static void LogInfo(object message)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(message);
        }

        public static void LogWarning(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogWarning(data);
        }

        public string GetStringConfig(string fieldName)
        {
            return (GetType().GetField(fieldName).GetValue(this) as ConfigEntry<string>).Value;
        }

        public float GetFloatConfig(string fieldName)
        {
            return (GetType().GetField(fieldName).GetValue(this) as ConfigEntry<float>).Value;
        }

        [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateEitr))]
        public class Hud_UpdateEitr_HideEitrValue
        {
            public static void Prefix(Hud __instance) => __instance.m_eitrText?.gameObject.SetActive(!hideEitrValue.Value);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateStats), typeof(float))]
        public static class Player_UpdateStats_EitrRegenMultiplier
        {
            public static float s_eitrRegenTimeMultiplier = 1f;

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var multiplierField = typeof(Player_UpdateStats_EitrRegenMultiplier).GetField(nameof(s_eitrRegenTimeMultiplier));

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    // Looking for m_eiterRegen after (1f - m_eitr / maxEitr) to add multiplier to m_eiterRegen next to it
                    if (codes[i].opcode == OpCodes.Mul && 
                        codes[i - 1].opcode == OpCodes.Ldfld && ((FieldInfo)codes[i - 1].operand).Name == "m_eiterRegen")
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldsfld, multiplierField));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
                        break;
                    }
                }

                return codes;
            }

            [HarmonyPriority(Priority.VeryLow)]
            public static void Prefix(Player __instance)
            {
                s_eitrRegenTimeMultiplier = 1f;
                __instance.m_eitrRegenDelay = baseEitrRegenDelay.Value;
                __instance.m_eiterRegen = baseEitrRegen.Value;

                if (__instance.InIntro() || __instance.IsTeleporting())
                    return;

                if (extraEitrRegeneration.Value && extraEitrRegenerationPercent.Value > 0f && extraEitrRegenerationPoints.Value > 0)
                    __instance.m_eiterRegen *= 1f + ExtraEitr.GetMultiplier(__instance);

                if (linearRegeneration.Value && 0f < linearRegenerationThreshold.Value && linearRegenerationThreshold.Value < 1f && linearRegenerationMultiplier.Value > 0f && __instance.GetMaxEitr() != 0f)
                {
                    if (__instance.GetEitrPercentage() < linearRegenerationThreshold.Value)
                    {
                        float t = Mathf.Clamp01(__instance.GetEitr() / (__instance.GetMaxEitr() * linearRegenerationThreshold.Value));
                        s_eitrRegenTimeMultiplier = Mathf.Lerp(linearRegenerationMultiplier.Value, s_eitrRegenTimeMultiplier, t);
                    }
                    else if (__instance.GetEitrPercentage() > linearRegenerationThreshold.Value)
                    {
                        float t = Mathf.Clamp01((__instance.GetMaxEitr() - __instance.GetEitr()) / (__instance.GetMaxEitr() * (1f - linearRegenerationThreshold.Value)));
                        s_eitrRegenTimeMultiplier = Mathf.Lerp(1 / linearRegenerationMultiplier.Value, s_eitrRegenTimeMultiplier, t);
                    }
                }
            }
        }
    }
}

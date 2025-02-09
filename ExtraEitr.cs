using static EitrMagicExtended.EitrMagicExtended;
using HarmonyLib;
using System;
using static ItemDrop;

namespace EitrMagicExtended
{
    internal static class ExtraEitr
    {
        public static float GetMultiplier(Player player)
        {
            float maxEitr = player.GetMaxEitr();
            if (extraEitrRegenerationOnlyFood.Value)
                player.GetTotalFoodValue(out _, out _, out maxEitr);

            return GetEitrRegenerationValueFromEitrPoints(maxEitr - GetAdditionalBaseEitr(player));
        }

        private static float GetEitrRegenerationValueFromEitrPoints(float points)
        {
            return (extraEitrRegenerationPercent.Value / 100f) * (points) / extraEitrRegenerationPoints.Value;
        }

        private static bool IsFoodItemForExtraEitrRegeneration(ItemData item, out float foodEitr)
        {
            foodEitr = 0f;

            if (!extraEitrRegeneration.Value || Player.m_localPlayer == null)
                return false;

            if (item.m_shared.m_itemType == ItemData.ItemType.Consumable)
                foodEitr = item.m_shared.m_foodEitr;

            return foodEitr > 0 || item.m_shared.m_appendToolTip != null && IsFoodItemForExtraEitrRegeneration(item.m_shared.m_appendToolTip.m_itemData, out foodEitr);
        }

        private static float GetAdditionalBaseEitr(Player player)
        {
            if (!baseEitr.Value)
                return 0f;

            return player.GetSkillFactor(Skills.SkillType.ElementalMagic) * elementalMagicBaseEitrIncrease.Value +
                   player.GetSkillFactor(Skills.SkillType.BloodMagic) * bloodMagicBaseEitrIncrease.Value;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        public static class Player_GetTotalFoodValue_BaseEitrIncrease
        {
            [HarmonyPriority(Priority.VeryLow)]
            public static void Postfix(Player __instance, ref float eitr)
            {
                if (!baseEitr.Value)
                    return;

                if (__instance != Player.m_localPlayer)
                    return;

                eitr += GetAdditionalBaseEitr(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemData), nameof(ItemData.GetTooltip), typeof(ItemData), typeof(int), typeof(bool), typeof(float), typeof(int))]
        public static class ItemDrop_ItemData_GetTooltip_EitrRegenTooltipForFoodRegen
        {
            private static string[] tooltipTokens = new string[] { "$se_staminaregen", "$item_food_regen", "$item_food_duration", "$item_food_eitr" };

            [HarmonyPriority(Priority.First)]
            [HarmonyAfter("shudnal.StaminaExtended")]
            [HarmonyBefore("shudnal.MyLittleUI")]
            private static void Postfix(ItemData item, ref string __result)
            {
                if (!IsFoodItemForExtraEitrRegeneration(item, out float foodEitr))
                    return;

                int index = -1;
                foreach (string tailString in tooltipTokens)
                {
                    index = __result.IndexOf(tailString, StringComparison.InvariantCulture);
                    if (index != -1)
                        break;
                }

                if (index == -1)
                    return;

                string tooltip = string.Format("\n$se_eitrregen: <color=#9090ffff>{0:P1}</color> ($item_current:<color=yellow>{1:P1}</color>)",
                                                GetEitrRegenerationValueFromEitrPoints(foodEitr),
                                                GetMultiplier(Player.m_localPlayer));

                int i = __result.IndexOf("\n", index, StringComparison.InvariantCulture);
                if (i != -1)
                    __result.Insert(i, tooltip);
                else
                    __result += tooltip;
            }
        }

        [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.AddActiveEffects))]
        public static class TextsDialog_AddActiveEffects_SeasonTooltipWhenBuffDisabled
        {
            private static void Postfix(TextsDialog __instance)
            {
                if (Player.m_localPlayer == null)
                    return;

                float multiplier = GetMultiplier(Player.m_localPlayer);
                if (multiplier < 0.01f)
                    return;

                __instance.m_texts[0].m_text += Localization.instance.Localize($"\n$se_eitrregen ({(extraEitrRegenerationOnlyFood.Value ? "$item_food" : "$hud_misc")}): <color=orange>{multiplier:P1}</color>");
            }
        }
    }
}

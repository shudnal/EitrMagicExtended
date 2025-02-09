using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EitrMagicExtended.EitrMagicExtended;

namespace EitrMagicExtended
{
    public static class StaffShieldPatches
    {
        private readonly static Dictionary<SE_Shield, MeshRenderer> s_shieldSpheres = new Dictionary<SE_Shield, MeshRenderer>();
        private readonly static MaterialPropertyBlock s_matBlock = new MaterialPropertyBlock();
        private const string itemDataStaffShieldName = "$item_staffshield";
        private const string itemDropStaffShieldName = "StaffShield";

        [Flags]
        public enum HitType
        {
            None = 0,
            Undefined = 1,
            EnemyHit = 2,
            PlayerHit = 4,
            Fall = 8,
            Drowning = 0x10,
            Burning = 0x20,
            Freezing = 0x40,
            Poisoned = 0x80,
            Water = 0x100,
            Smoke = 0x200,
            EdgeOfWorld = 0x400,
            Impact = 0x800,
            Cart = 0x1000,
            Tree = 0x2000,
            Self = 0x4000,
            Structural = 0x8000,
            Turret = 0x10000,
            Boat = 0x20000,
            Stalagtite = 0x40000,
            Catapult = 0x80000,
            CinderFire = 0x100000,
            AshlandsOcean = 0x200000,
            All = 0x3FFFFF
        }

        private static bool IsFullProtected(HitData.HitType hitType)
        {
            return shieldFullProtectionFrom.Value != HitType.None && shieldFullProtectionFrom.Value.HasFlag(GetHitType(hitType));
        }

        private static HitType GetHitType(HitData.HitType hitType)
        {
            return hitType switch
            {
                HitData.HitType.Undefined => HitType.Undefined,
                HitData.HitType.EnemyHit => HitType.EnemyHit,
                HitData.HitType.PlayerHit => HitType.PlayerHit,
                HitData.HitType.Fall => HitType.Fall,
                HitData.HitType.Drowning => HitType.Drowning,
                HitData.HitType.Burning => HitType.Burning,
                HitData.HitType.Freezing => HitType.Freezing,
                HitData.HitType.Poisoned => HitType.Poisoned,
                HitData.HitType.Water => HitType.Water,
                HitData.HitType.Smoke => HitType.Smoke,
                HitData.HitType.EdgeOfWorld => HitType.EdgeOfWorld,
                HitData.HitType.Impact => HitType.Impact,
                HitData.HitType.Cart => HitType.Cart,
                HitData.HitType.Tree => HitType.Tree,
                HitData.HitType.Self => HitType.Self,
                HitData.HitType.Structural => HitType.Structural,
                HitData.HitType.Turret => HitType.Turret,
                HitData.HitType.Boat => HitType.Boat,
                HitData.HitType.Stalagtite => HitType.Stalagtite,
                HitData.HitType.Catapult => HitType.Catapult,
                HitData.HitType.CinderFire => HitType.CinderFire,
                HitData.HitType.AshlandsOcean => HitType.AshlandsOcean,
                _ => HitType.All,
            };
        }

        [HarmonyPatch(typeof(SE_Shield), nameof(SE_Shield.OnDamaged))]
        public static class SE_Shield_OnDamaged_PreventEffectSpam
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(ref HitData hit)
            {
                if (preventZeroDamageShieldSpam.Value && hit.GetTotalDamage() == 0)
                    return false;

                if (preventLookVectorConsoleSpam.Value && hit.m_dir.sqrMagnitude < 0.1f)
                    hit.m_dir = Vector3.down;

                return true;
            }

            private static void Postfix(SE_Shield __instance, ref HitData hit)
            {
                if (changeShieldColorByHealth.Value)
                {
                    if (__instance.m_totalAbsorbDamage == 0f || __instance.m_startEffectInstances.Length == 0 || __instance.m_startEffectInstances[0] == null)
                        return;

                    if (!s_shieldSpheres.TryGetValue(__instance, out MeshRenderer sphere))
                    {
                        sphere = __instance.m_startEffectInstances[0].transform.Find("Sphere").GetComponent<MeshRenderer>();
                        s_shieldSpheres[__instance] = sphere;
                    }

                    sphere.GetPropertyBlock(s_matBlock);
                    s_matBlock.SetColor(ShaderProps._Color, Color.Lerp(sphere.sharedMaterial.color, shieldTargetColorZeroHealth.Value, __instance.m_damage / __instance.m_totalAbsorbDamage));
                    sphere.SetPropertyBlock(s_matBlock);
                }

                if (IsFullProtected(hit.m_hitType) && hit.GetTotalDamage() > 0f)
                {
                    DamageText.instance.ShowText(__instance.m_character.GetDamageModifier(hit.m_damage.GetMajorityDamageType()), __instance.m_character.GetTopPoint(), hit.GetTotalDamage(), true);
                    hit.m_damage.Modify(0f);
                }
            }
        }

        [HarmonyPatch(typeof(SE_Shield), nameof(SE_Shield.IsDone))]
        public static class SE_Shield_IsDone_ClearCachedSphere
        {
            private static void Postfix(SE_Shield __instance, bool __result)
            {
                if (__result)
                    s_shieldSpheres.Remove(__instance);
            }
        }

        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.OnDestroy))]
        public static class ZoneSystem_OnDestroy_ClearSpheresCache
        {
            private static void Postfix() => s_shieldSpheres.Clear();
        }

        public static void PatchStaffShield(ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared.m_name != itemDataStaffShieldName)
                return;

            Attack attack = item.m_shared.m_secondaryAttack;

            if (attack == null || attack.m_attackAnimation == "staff_trollsummon")
                return;

            attack.GetType().GetFields().Do(f => f.SetValue(attack, f.GetValue(item.m_shared.m_attack)));
            attack.m_attackEitr *= 0.5f;
            attack.m_attackHealthPercentage *= 0.5f;
            attack.m_attackAnimation = "staff_trollsummon";
            attack.m_startEffect = new EffectList();
        }

        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        public static class ZoneSystem_Start_PatchStaffShield
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix()
            {
                if (addShieldStaffSecondaryAttack.Value)
                    PatchStaffShield(ObjectDB.instance.GetItemPrefab(itemDropStaffShieldName).GetComponent<ItemDrop>()?.m_itemData);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Load))]
        public static class Player_Load_PatchStaffShield
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(Player __instance)
            {
                if (addShieldStaffSecondaryAttack.Value)
                    __instance.GetInventory().m_inventory.Do(PatchStaffShield);
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class Humanoid_EquipItem_PatchStaffShield
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemDrop.ItemData item, bool __result)
            {
                if (__result && addShieldStaffSecondaryAttack.Value)
                    PatchStaffShield(item);
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
        public static class Humanoid_StartAttack_Staff
        {
            public static ItemDrop.ItemData staffshield;
            public static Attack attack;

            private static bool Prefix(Humanoid __instance, bool secondaryAttack)
            {
                if (secondaryAttack && __instance == Player.m_localPlayer && __instance.GetCurrentWeapon() is ItemDrop.ItemData weapon && weapon.m_shared.m_name == itemDataStaffShieldName)
                    return weapon.m_shared.m_attackStatusEffect == null || Player.m_localPlayer.GetSEMan().HaveStatusEffect(weapon.m_shared.m_attackStatusEffect.NameHash());

                return true;
            }

            private static void Postfix(Humanoid __instance, bool __result)
            {
                if (__result && __instance == Player.m_localPlayer && __instance.m_currentAttackIsSecondary && __instance.GetCurrentWeapon() is ItemDrop.ItemData weapon && weapon.m_shared.m_name == itemDataStaffShieldName)
                {
                    staffshield = weapon;
                    attack = __instance.m_currentAttack;
                }
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        public static class Attack_FireProjectileBurst_Staff
        {
            private static void Prefix(Attack __instance, ref bool __state)
            {
                if (__state = __instance == Humanoid_StartAttack_Staff.attack && __instance.m_weapon == Humanoid_StartAttack_Staff.staffshield)
                    Humanoid_StartAttack_Staff.staffshield.m_shared.m_attackStatusEffectChance = 0;
            }

            private static void Postfix(bool __state)
            {
                if (__state)
                {
                    Humanoid_StartAttack_Staff.staffshield.m_shared.m_attackStatusEffectChance = 1;
                    if (Humanoid_StartAttack_Staff.staffshield.m_shared.m_attackStatusEffect is SE_Shield shield)
                    {
                        shield.m_breakEffects.Create(Player.m_localPlayer.GetCenterPoint(), Player.m_localPlayer.transform.rotation, Player.m_localPlayer.transform, Player.m_localPlayer.GetRadius() * 2f);
                        Player.m_localPlayer.GetSEMan().RemoveStatusEffect(shield);
                    }
                }
            }
        }
    }
}

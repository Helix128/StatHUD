using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Actors;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Combat;
using Assets.Scripts.Inventory.Stats;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Managers;
using Assets.Scripts.Menu.Shop;
using Assets.Scripts.Utility;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppSystem.Threading;
using StatsHUD;
using UnityEngine;

public class EnemyModule
{
    public static Dictionary<Enemy, float> enemyDamages = new Dictionary<Enemy, float>();
    public static bool isDirty = false;
    public static float lastRecalcTime = 0;

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.InitEnemy))]
    public class EnemySpawnPatch
    {
        public static void Postfix(Enemy __instance)
        {
            float baseDamage = __instance.GetBasePlayerDamage();

            bool added = enemyDamages.TryAdd(__instance, baseDamage);
            if (added)
            {
                isDirty = true;
            }
        }
    }

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.Despawn))]
    public class EnemyDeathPatch
    {
        public static void Prefix(Enemy __instance)
        {
            enemyDamages.Remove(__instance);
            isDirty = true;
        }
    }

    public static float MinDamage;
    public static float MaxDamage;

    public static void RecalculateDamage()
    {
        if (enemyDamages.Count == 0)
        {
            MinDamage = 0;
            MaxDamage = 0;
            isDirty = false;
            return;
        }

        CombatScaling.GetDamageMultiplierAddition(out float baseAdd, out float swarmAdd, out float stageAdd);
        float factor = 1 + baseAdd + swarmAdd + stageAdd;

        var allBaseDamages = enemyDamages.Values;

        float minBase = allBaseDamages.Min();
        float maxBase = allBaseDamages.Max();

        MinDamage = minBase * factor;
        MaxDamage = maxBase * factor;

        isDirty = false;
    }

    public const float RECALC_THROTTLE = 0.5f;
    public static void Update()
    {
        if (isDirty && (Time.time - lastRecalcTime > RECALC_THROTTLE))
        {
            RecalculateDamage();
            lastRecalcTime = Time.time;
        }
    }
    public static void Reset()
    {
        enemyDamages.Clear();
        MinDamage = 0;
        MaxDamage = 0;
        isDirty = false;
    }
}

public static class EnemyExtensions
{
    public static float GetBasePlayerDamage(this Enemy enemy)
    {
        DamageContainer dc = DamageUtility.GetPlayerDamage(EnemyStats.GetDamage(enemy), 0, Vector3.zero, enemy, "", DcFlags.None);

        dc.damageEffect = EDamageEffect.None;

        return dc.damage;
    }

    public static float GetCurrentDamage(this Enemy enemy)
    {
        float damage = enemy.GetBasePlayerDamage();

        CombatScaling.GetDamageMultiplierAddition(out float baseAdd, out float swarmAdd, out float stageAdd);
        float factor = 1 + baseAdd + swarmAdd + stageAdd;

        damage *= factor;
        return damage;
    }
}
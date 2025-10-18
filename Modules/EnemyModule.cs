using System;
using System.Collections.Generic;
using Assets.Scripts.Actors;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Combat;
using Assets.Scripts.Inventory.Stats;
using Assets.Scripts.Inventory__Items__Pickups.Stats;
using Assets.Scripts.Managers;
using Assets.Scripts.Menu.Shop;
using Assets.Scripts.Utility;
using HarmonyLib;
using StatsHUD;
using UnityEngine;

public class EnemyModule
{
    public static List<Enemy> enemies = new List<Enemy>();

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.InitEnemy))]
    public class EnemySpawnPatch
    {
        public static void Postfix(Enemy __instance)
        {
            enemies.Add(__instance);
            RecalculateDamage();
        }
    }

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.Despawn))]
    public class EnemyDeathPatch
    {
        public static void Prefix(Enemy __instance)
        {
            enemies.Remove(__instance);
            RecalculateDamage();
        }
    }

    public static float MinDamage;
    public static float MaxDamage;
    public static void RecalculateDamage()
    {
        MinDamage = float.MaxValue;
        MaxDamage = float.MinValue;

        if (enemies.Count == 0)
        {
            MinDamage = 0;
            MaxDamage = 0;
            return;
        }

        foreach (var enemy in enemies)
        {
            float damage = enemy.GetCurrentDamage();
            if (damage < MinDamage)
            {
                MinDamage = damage;
            }
            else if (damage > MaxDamage)
            {
                MaxDamage = damage;
            }
        }
    }

    public static void Reset()
    {
        enemies.Clear();
        MinDamage = 0;
        MaxDamage = 0;
    }
}

public static class EnemyExtensions
{
    public static float GetCurrentDamage(this Enemy enemy)
    {
        DamageContainer dc = DamageUtility.GetPlayerDamage(EnemyStats.GetDamage(enemy), 0, Vector3.zero, enemy, "", DcFlags.None);
        float damage = dc.damage;
        dc.damageEffect = EDamageEffect.None;

        CombatScaling.GetDamageMultiplierAddition(out float baseAdd, out float swarmAdd, out float stageAdd);
        float factor = 1 + baseAdd + swarmAdd + stageAdd;
        damage *= factor;
        return damage;
    }
}

using System;
using System.Collections.Generic;
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
        if (enemies.Count == 0)
        {
            MinDamage = 0;
            MaxDamage = 0;
            return;
        }

        CombatScaling.GetDamageMultiplierAddition(out float baseAdd, out float swarmAdd, out float stageAdd);
        float factor = 1 + baseAdd + swarmAdd + stageAdd;

        float firstDamage = enemies[0].GetBasePlayerDamage() * factor;

        float min = firstDamage;
        float max = firstDamage;
        
        for (int i = 1; i < enemies.Count; i++)
        {
            float damage = enemies[i].GetBasePlayerDamage() * factor;
            if (damage < min)
            {
                min = damage;
            }
            if (damage > max)
            {
                max = damage;
            }
        }

        MinDamage = min;
        MaxDamage = max;
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
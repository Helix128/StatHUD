
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Actors;
using Assets.Scripts.Actors.Enemies;
using HarmonyLib;
using StatsHUD;
using UnityEngine;

public class DPSModule
{
    public static float DPS = -1;
    public static float lastRefresh = -1;
    public const float DMG_INSTANCE_LIFETIME = 1.0f;
    public static List<DamageInstance> damageInstances = new List<DamageInstance>();
    public struct DamageInstance
    {
        public float damage;
        public float time;
    }

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.DamageFromPlayerWeapon))]
    public class EnemyHurtPatch
    {
        public static void Postfix(DamageContainer dc)
        {
            AddDamageInstance(dc);
        }
    }

    public static void AddDamageInstance(DamageContainer dc)
    {   
        DamageInstance instance = new DamageInstance
        {
            damage = dc.damage,
            time = TimeModule.GetTime()
        };
        damageInstances.Add(instance);
    }

    public static float GetDPS()
    {
        if (damageInstances.Count == 0)
        {
            DPS = 0;
            return DPS;
        }

        if (TimeModule.GetTime() - lastRefresh < 0.1f)
        {
            return DPS;
        }

        float totalDamage = 0;
        float now = TimeModule.GetTime();

        damageInstances.RemoveAll(di => now - di.time > DMG_INSTANCE_LIFETIME);
        for (int i = 0; i < damageInstances.Count; i++)
        {
            totalDamage += damageInstances[i].damage;
        }

        DPS = totalDamage;
        lastRefresh = now;
        return DPS;
    }

    public static void Reset()
    {
        DPS = -1;
        lastRefresh = -1;
        damageInstances.Clear();
    }


    
}
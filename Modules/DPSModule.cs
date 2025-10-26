using System.Collections.Generic;
using Assets.Scripts.Actors;
using Assets.Scripts.Actors.Enemies;
using HarmonyLib;
using StatsHUD;
using UnityEngine;

public class DPSModule
{

    public const float DPS_WINDOW_SECONDS = 3.0f;

    public const float SMOOTHING_FACTOR = 25.0f;

    public const float RECALC_THROTTLE = 0.1f;
    public const float DISPLAY_THROTTLE = 0.033f;

    private static float rawDPS = 0;
    private static float displayedDPS = 0;

    private static float lastRecalcTime = -1;
    private static float lastDisplayTime = -1;
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

    public static void Update()
    {
        float now = TimeModule.GetTime();

        if (TimeModule.isPaused)
        {
            lastRecalcTime = -1;
            lastDisplayTime = -1;
            return;
        }   
        if (now - lastRecalcTime > RECALC_THROTTLE)
        {
            RecalculateRawDPS(now);
            lastRecalcTime = now;
        }

        if (now - lastDisplayTime > DISPLAY_THROTTLE)
        {
            lastDisplayTime = now;
            displayedDPS = Mathf.Lerp(displayedDPS, rawDPS, Time.deltaTime * SMOOTHING_FACTOR);

            if (displayedDPS < 0.01f)
            {
                displayedDPS = 0;
            }
        }

    }

    private static void RecalculateRawDPS(float now)
    {
        if (damageInstances.Count == 0)
        {
            rawDPS = 0;
            return;
        }

        float totalDamage = 0;

        for (int i = damageInstances.Count - 1; i >= 0; i--)
        {
            if (now - damageInstances[i].time > DPS_WINDOW_SECONDS)
            {
                damageInstances.RemoveAt(i);
            }
            else
            {
                totalDamage += damageInstances[i].damage;
            }
        }

        rawDPS = totalDamage / DPS_WINDOW_SECONDS;
    }

    public static float GetDPS()
    {
        return displayedDPS;
    }

    public static void Reset()
    {
        rawDPS = 0;
        displayedDPS = 0;
        lastRecalcTime = -1;
        damageInstances.Clear();
    }
}
using System;
using System.Collections.Generic;
using Assets.Scripts.Actors.Player;
using Inventory__Items__Pickups;
using UnityEngine;

public class PlayerModule
{
    public static float lastHistoryRefresh = 0;
    public const float HISTORY_THROTTLE = 0.1f;
    public const float HISTORY_LIFETIME = 1.0f;

    public static float MaxXp;
    public static float Xp;
    public static float XpSec;
    public static float XpPct;

    public static float Gold;
    public static float GoldSec;

    public struct HistoryEntry
    {
        public float Time;
        public float Value;
    }
    public static List<HistoryEntry> xpHistory = new List<HistoryEntry>();
    public static List<HistoryEntry> goldHistory = new List<HistoryEntry>();

    public static MyPlayer GetPlayer()
    {
        return MyPlayer.Instance;
    }

    public static void RecalculateStats()
    {
        MyPlayer player = GetPlayer();
        float now = TimeModule.GetTime();

        if (player == null || player.inventory == null || player.inventory.playerXp == null)
        {
            Reset();
            return;
        }

        float lastXp = Xp;
        float lastGold = Gold;

        Xp = player.inventory.playerXp.GetXpInt();
        Gold = player.inventory.gold;
        MaxXp = XpUtility.XpTotalNextLevel(player.inventory.playerXp.xp);
        XpPct = (MaxXp > 0) ? (Xp / MaxXp) * 100.0f : 0;

        if (Xp > lastXp)
        {
            xpHistory.Add(new HistoryEntry { Time = now, Value = Xp - lastXp });
        }
        if (Gold > lastGold)
        {
            goldHistory.Add(new HistoryEntry { Time = now, Value = Gold - lastGold });
        }

        if (now - lastHistoryRefresh < HISTORY_THROTTLE)
        {
            return; 
        }
        
        lastHistoryRefresh = now;

        for (int i = xpHistory.Count - 1; i >= 0; i--)
        {
            if (now - xpHistory[i].Time > HISTORY_LIFETIME)
            {
                xpHistory.RemoveAt(i);
            }
        }
        
        for (int i = goldHistory.Count - 1; i >= 0; i--)
        {
            if (now - goldHistory[i].Time > HISTORY_LIFETIME)
            {
                goldHistory.RemoveAt(i);
            }
        }
        XpSec = 0;
        for (int i = 0; i < xpHistory.Count; i++)
        {
            XpSec += xpHistory[i].Value;
        }
        XpSec /= HISTORY_LIFETIME;

        GoldSec = 0;
        for (int i = 0; i < goldHistory.Count; i++)
        {
            GoldSec += goldHistory[i].Value;
        }
        GoldSec /= HISTORY_LIFETIME;
    }

    public static void Update()
    {
        RecalculateStats();
    }
    public static float GetXp()
    {
        return Xp;
    }

    public static float GetMaxXp()
    {
        return MaxXp;
    }

    public static float GetXpSec()
    {
        return XpSec;
    }

    public static float GetXpPct()
    {
        return XpPct;
    }

    public static float GetGold()
    {
        return Gold;
    }

    public static float GetGoldSec()
    {
        return GoldSec;
    }

    public static void Reset()
    {
        lastHistoryRefresh = 0; 
        MaxXp = 0;
        Xp = 0;
        XpSec = 0;
        XpPct = 0;
        Gold = 0;
        GoldSec = 0;
        xpHistory.Clear();
        goldHistory.Clear();
    }
}

using System;
using System.Collections.Generic;
using Assets.Scripts.Actors.Player;
using Inventory__Items__Pickups;
using UnityEngine;

public class PlayerModule
{

    public static float lastRefresh = 0;
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

        if (player == null)
        {
            lastRefresh = TimeModule.GetTime();
            return;
        }
        else if (player.inventory == null || player.inventory.playerXp == null)
        {
            lastRefresh = TimeModule.GetTime();
            return;
        }

        if (TimeModule.GetTime() - lastRefresh < 0.1f)
        {
            return;
        }

        float lastXp = Xp;
        float lastGold = Gold;
        Xp = player.inventory.playerXp.xp;
        MaxXp = XpUtility.XpTotalNextLevel(player.inventory.playerXp.xp);
        Gold = player.inventory.gold;

        xpHistory.RemoveAll(t => TimeModule.GetTime() - t.Time > HISTORY_LIFETIME);
        if (Xp > lastXp)
        {
            xpHistory.Add(new HistoryEntry { Time = TimeModule.GetTime(), Value = Xp - lastXp });
        }

        goldHistory.RemoveAll(t => TimeModule.GetTime() - t.Time > HISTORY_LIFETIME);
        if (Gold > lastGold)
        {
            goldHistory.Add(new HistoryEntry { Time = TimeModule.GetTime(), Value = Gold - lastGold });
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

        XpPct = (MaxXp > 0) ? (Xp / MaxXp) * 100.0f : 0;
        lastRefresh = TimeModule.GetTime();
    }

    public static float GetXp()
    {
        RecalculateStats();
        return Xp;
    }

    public static float GetMaxXp()
    {
        RecalculateStats();
        return MaxXp;
    }

    public static float GetXpSec()
    {
        RecalculateStats();
        return XpSec;
    }

    public static float GetXpPct()
    {
        RecalculateStats();
        return XpPct;
    }

    public static float GetGold()
    {
        RecalculateStats();
        return Gold;
    }

    public static float GetGoldSec()
    {
        RecalculateStats();
        return GoldSec;
    }

    public static void Reset()
    {
        lastRefresh = 0;
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
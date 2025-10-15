
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

    public static List<Tuple<float, float>> xpHistory = new List<Tuple<float, float>>();
    public static List<Tuple<float, float>> goldHistory = new List<Tuple<float, float>>();

    public static void RecalculateStats()
    {
        MyPlayer player = MyPlayer.Instance;

        if (player == null)
        {
            lastRefresh = TimeModule.GetTime();
            return;
        }
        else if(player.inventory == null || player.inventory.playerXp == null)
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

        xpHistory.RemoveAll(t => TimeModule.GetTime() - t.Item1 > HISTORY_LIFETIME);
        if (Xp > lastXp)
        {
            xpHistory.Add(new Tuple<float, float>(TimeModule.GetTime(), Xp - lastXp));
        }

        goldHistory.RemoveAll(t => TimeModule.GetTime() - t.Item1 > HISTORY_LIFETIME);
        if (Gold > lastGold)
        {
            goldHistory.Add(new Tuple<float, float>(TimeModule.GetTime(), Gold - lastGold));
        }

        XpSec = 0;
        for (int i = 0; i < xpHistory.Count; i++)
        {
            XpSec += xpHistory[i].Item2;
        }
        XpSec /= HISTORY_LIFETIME;
        
        GoldSec = 0;
        for (int i = 0; i < goldHistory.Count; i++)
        {
            GoldSec += goldHistory[i].Item2;
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
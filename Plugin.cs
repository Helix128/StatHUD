using System.Collections.Generic;
using Assets.Scripts.Actors;
using Assets.Scripts.Actors.Enemies;
using Assets.Scripts.Actors.Player;
using Assets.Scripts.Game.Combat;
using Assets.Scripts.Inventory__Items__Pickups.Weapons;
using Assets.Scripts.Managers;
using Assets.Scripts.Utility;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Inventory__Items__Pickups;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatsHUD;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private static Harmony _harmony;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();

        ClassInjector.RegisterTypeInIl2Cpp<StatHUD>();
        ClassInjector.RegisterTypeInIl2Cpp<StatsUIPanel>();
        ClassInjector.RegisterTypeInIl2Cpp<ComboUIPanel>();

        GameObject hudGO = new GameObject("StatHUD");
        hudGO.AddComponent<StatHUD>();
        Object.DontDestroyOnLoad(hudGO);
    }

    [HarmonyPatch(typeof(Enemy))]
    [HarmonyPatch(nameof(Enemy.DamageFromPlayerWeapon))]
    public class EnemyHurtPatch
    {
        public static void Postfix(DamageContainer dc)
        {
            StatHUD.AddDamageInstance(
                new StatHUD.DamageInstance
                {
                    damage = dc.damage,
                    time = Time.time,
                    source = dc.damageSource
                }
            );
        }
    }

    public class StatHUD : MonoBehaviour
    {
        int lastScene = 0;
        bool inGame = false;
        bool isInitialized = false;
        MyPlayer player;

        UiManager uiManager;
        StatsUIPanel uiPanel;
        ComboUIPanel comboPanel;

        public struct DamageInstance
        {
            public float damage;
            public float time;
            public string source;
        }

        Queue<(float damage, float time)> damageWindow = new Queue<(float, float)>();
        float totalDamageInWindow = 0f;

        float XPS; // experience per second
        float DPS; // damage per second
        float GPS; // gold per second
        const float DPS_WINDOW = 6.0f;
        const float COMBO_WINDOW = 2.0f;
        const float XPS_WINDOW = 5.0f;
        const float GPS_WINDOW = 5.0f;

        int lastXp = 0;
        Queue<(int xp, float time)> xpHistory = new Queue<(int, float)>();

        int lastGold = 0;
        Queue<(int gold, float time)> goldHistory = new Queue<(int, float)>();

        float currentComboDamage = 0f;
        float currentComboStartTime = 0f;
        float currentComboTime = 0f;
        float lastDamageTime = 0f;
        int currentComboHits = 0;
        bool comboActive = false;

        float highestComboDamage = 0f;
        int highestComboHits = 0;

        bool isPlaying;
        bool playerDead = false;
        float gameTime = 0f;
        bool uiReadyToShow = false;
        
        void Update()
        {
            if (SceneManager.GetActiveScene().buildIndex != lastScene)
            {
                lastScene = SceneManager.GetActiveScene().buildIndex;
                int sceneIdx = lastScene;
                if (sceneIdx == 2)
                {
                    if (!inGame)
                    {
                        inGame = true;
                        Log.LogInfo($"Entered game scene ({sceneIdx}).");
                        ResetStats();
                        isPlaying = true;
                        uiReadyToShow = false;
                    }
                    else
                    {
                        Log.LogInfo($"Returned to game scene ({sceneIdx}) - resetting for new run.");
           
                        isInitialized = false;
                        player = null;
                        uiManager = null;
                        ResetStats();
                        isPlaying = true;
                        uiReadyToShow = false;
                    }
            
                }
                else if (sceneIdx == 3)
                {
                    DisableUI();
                    isPlaying = false;
                    Log.LogInfo("Entered loading screen.");
                }
                else if (sceneIdx <= 1)
                {

                    if (inGame)
                    {
                        inGame = false;
                        isInitialized = false;
                        player = null;
                        uiManager = null;
                        ResetStats();
                        DisableUI();
                        Log.LogInfo("Exited to main menu or non-gameplay scene.");
                    }
                }
                else
                {
                    DisableUI();
                }
            }
            if (inGame)
            {
                FindPlayerRefs();
                CalculateStats();
                UpdateUIPanel();
            }
        }

        void ResetStats(bool fullReset = true)
        {
            if (fullReset)
            {
                isInitialized = false;
                player = null;
                uiManager = null;
            }
            
            gameTime = 0f;
            
            damageWindow.Clear();
            totalDamageInWindow = 0f;
            DPS = 0f;
            
            lastXp = 0;
            xpHistory.Clear();
            XPS = 0f;
            
            lastGold = 0;
            goldHistory.Clear();
            GPS = 0f;
            
            currentComboDamage = 0f;
            currentComboStartTime = 0f;
            currentComboTime = 0f;
            lastDamageTime = 0f;
            currentComboHits = 0;
            comboActive = false;
            highestComboDamage = 0f;
            highestComboHits = 0;
            playerDead = false;
            uiReadyToShow = false;

            if (uiPanel != null)
            {
                uiPanel.ResetPanel();
            }
            if (comboPanel != null)
            {
                comboPanel.ResetPanel();
            }
            
            Log.LogInfo("Stats reset for new run.");
        }

        void EnableUI()
        {
            if (uiPanel != null)
            {
                uiPanel.EnablePanel();
            }
            if (comboPanel != null)
            {
                comboPanel.DisablePanel(); 
            }
            if (uiPanel != null || comboPanel != null)
            {
                Log.LogInfo("UI panels enabled.");
            }
        }

        void DisableUI()
        {
            if (uiPanel != null)
            {
                uiPanel.DisablePanel();
            }
            if (comboPanel != null)
            {
                comboPanel.DisablePanel();
            }
            Log.LogInfo("UI panels disabled.");
        }

        void FindPlayerRefs()
        {
            if (isInitialized) return;
            
            if (player == null)
            {
                var players = FindObjectsByType<MyPlayer>(FindObjectsSortMode.None);
                if (players.Length > 0)
                {
                    player = players[0];
                    Log.LogInfo("Player reference found.");
                }
                else
                {
                    return; 
                }
            }
            
            if (uiManager == null)
            {
                var uiManagers = FindObjectsByType<UiManager>(FindObjectsSortMode.None);
                if (uiManagers.Length > 0)
                {
                    uiManager = uiManagers[0];
                    Log.LogInfo("UiManager reference found.");
                }
                else
                {
                    return;
                }
            }
            
            if (player != null && uiManager != null)
            {
                try
                {
                    if (player.inventory != null && player.inventory.GetCharacterLevel() == 0)
                    {
                        ResetStats(false);
                        Log.LogInfo("Fresh run detected - stats reset.");
                    }
                    
                    isInitialized = true;
                    InitializeUI();
                    Log.LogInfo("StatHUD initialized successfully.");
                }
                catch (System.Exception ex)
                {
                    Log.LogError($"Error during initialization: {ex.Message}");
                    isInitialized = false;
                }
            }
        }

        void InitializeUI()
        {
            if (uiPanel == null)
            {
                GameObject uiGO = new GameObject("StatsUIPanel");
                uiGO.transform.SetParent(transform, false);
                uiPanel = uiGO.AddComponent<StatsUIPanel>();
                uiPanel.Initialize();
                Log.LogInfo("StatsUIPanel created and initialized.");
            }

            if (comboPanel == null)
            {
                GameObject comboGO = new GameObject("ComboUIPanel");
                comboGO.transform.SetParent(transform, false);
                comboPanel = comboGO.AddComponent<ComboUIPanel>();
                comboPanel.Initialize();
                Log.LogInfo("ComboUIPanel created and initialized.");
            }

        }

        void UpdateUIPanel()
        {
            if (uiPanel == null || comboPanel == null || !isInitialized || !isPlaying) return;

            try
            {
                if (player == null || player.inventory == null || 
                    player.inventory.playerXp == null || 
                    player.inventory.playerHealth == null)
                {
                    return;
                }

                if (!uiReadyToShow)
                {
                    uiReadyToShow = true;
                    uiPanel.EnablePanel();
                    Log.LogInfo("UI panels ready - sliding in.");
                }

                int currentXp = player.inventory.playerXp.GetXpInt();
                float xpPercent = XpUtility.CurrentLevelProgress(currentXp);
                float xpToNextLevel = XpUtility.XpToNextLevel(currentXp);
                float maxXp = currentXp + xpToNextLevel;

                float stageDmgMult = CombatScaling.stageDamageMultiplier;
                float timeDmgMult = CombatScaling.damageMultiplicationPerMinute * CombatScaling.GetMinutes();
                float totalDmgMult = stageDmgMult * timeDmgMult;
                float minutes = CombatScaling.GetMinutes();

                float playerHp = player.inventory.playerHealth.hp;
                
                float maxDmg = 0f;
                float minDmg = float.MaxValue;
                
                if (EnemyManager.Instance != null && EnemyManager.Instance.enemies != null)
                {
                    var enemies = EnemyManager.Instance.enemies;
                    
                    if (enemies.Count > 0)
                    {
                        foreach (var enemy in enemies.Values)
                        {
                            try
                            {
                                var dc = DamageUtility.GetPlayerDamage(enemy, Vector3.zero, DcFlags.None);
                                float dmg = dc.damage;
                                float blocked = dc.damageBlockedByArmor;
                                dmg -= blocked;
                                if (dmg > maxDmg) maxDmg = dmg;
                                if (dmg < minDmg) minDmg = dmg;
                            }
                            catch (System.Exception)
                            {
                                continue;
                            }
                        }
                    }
                }
                
                if (minDmg == float.MaxValue)
                {
                    minDmg = 0f;
                }
                
                if (playerHp <= 0 && !playerDead)
                {
                    playerDead = true;
                    uiPanel.DisablePanel();
                    comboPanel.DisablePanel();
                    Log.LogInfo("Player died.");
                }
                else if (playerHp > 0 && playerDead)
                {
                    playerDead = false;
                }
                
                int maxDangerLevel = 0;
                if (maxDmg > 0)
                {
                    if (playerHp <= maxDmg)
                        maxDangerLevel = 2; 
                    else if (playerHp <= maxDmg * 2)
                        maxDangerLevel = 1; 
                }
                

                int minDangerLevel = 0;
                if (minDmg > 0)
                {
                    if (playerHp <= minDmg)
                        minDangerLevel = 2; 
                    else if (playerHp <= minDmg * 2)
                        minDangerLevel = 1; 
                }
                
                float comboRemaining = COMBO_WINDOW - (gameTime - lastDamageTime);

                StatData data = new StatData
                {
                    currentXp = currentXp,
                    maxXp = maxXp,
                    xpPercent = xpPercent,
                    xps = XPS,
                    gold = player.inventory.goldInt,
                    gps = GPS,
                    stageDmgMult = stageDmgMult,
                    timeDmgMult = timeDmgMult,
                    totalDmgMult = totalDmgMult,
                    minutes = minutes,
                    maxEnemyDmg = maxDmg,
                    minEnemyDmg = minDmg,
                    maxDangerLevel = maxDangerLevel,
                    minDangerLevel = minDangerLevel,
                    dps = DPS,
                    maxComboDamage = highestComboDamage,
                    maxComboHits = highestComboHits
                };

                ComboStatData comboData = new ComboStatData
                {
                    comboActive = comboActive,
                    currentComboHits = currentComboHits,
                    currentComboDamage = currentComboDamage,
                    currentComboTime = currentComboTime,
                    comboRemainingTime = comboRemaining
                };

                uiPanel.UpdateStats(data);
                comboPanel.UpdateComboStats(comboData);

                if (comboActive)
                {
                    comboPanel.EnablePanel();
                }
                else
                {
                    comboPanel.DisablePanel();
                }
            }
            catch (System.Exception ex)
            {
                Log.LogWarning($"Exception in UpdateUIPanel: {ex.Message}");
            }
        }

        void CalculateStats()
        {
            if (!isInitialized || uiManager == null) return;
            
            try
            {
                bool pause = !(uiManager.pause.current == null) && uiManager.pause.current.activeInHierarchy;
                isPlaying = uiManager.encounterWindows.activeEncounterWindow == null && !pause;

                if (isPlaying)
                {
                    gameTime += Time.deltaTime;
                }
                else
                {
                    return;
                }

                CalculateDPS(gameTime);
                CalculateXPS(gameTime);
                CalculateGPS(gameTime);
                UpdateComboState(gameTime);
            }
            catch (System.Exception ex)
            {
                Log.LogWarning($"Exception in CalculateStats: {ex.Message}");
            }
        }

        void CalculateDPS(float currentTime)
        {
            float windowStart = currentTime - DPS_WINDOW;
            
            while (damageWindow.Count > 0 && damageWindow.Peek().time < windowStart)
            {
                var removed = damageWindow.Dequeue();
                totalDamageInWindow -= removed.damage;
            }

            if (damageWindow.Count == 0)
            {
                DPS = 0f;
                return;
            }

            float oldestTime = damageWindow.Peek().time;
            float timeSpan = currentTime - oldestTime;
            DPS = timeSpan > 0.1f ? totalDamageInWindow / timeSpan : totalDamageInWindow / DPS_WINDOW;
        }

        void CalculateXPS(float currentTime)
        {
            if (player == null || player.inventory == null)
            {
                return;
            }
            if (player.inventory.playerXp == null)
            {
                return;
            }
            int currentXp = player.inventory.playerXp.xp;

            if (currentXp != lastXp)
            {
                int xpGained = currentXp - lastXp;
                if (xpGained > 0)
                {
                    xpHistory.Enqueue((xpGained, currentTime));
                }
                lastXp = currentXp;
            }

            while (xpHistory.Count > 0 && xpHistory.Peek().time < currentTime - XPS_WINDOW)
            {
                xpHistory.Dequeue();
            }

            if (xpHistory.Count == 0)
            {
                XPS = 0f;
                return;
            }

            int totalXp = 0;
            float oldestXpTime = currentTime;

            foreach (var entry in xpHistory)
            {
                totalXp += entry.xp;
                if (entry.time < oldestXpTime)
                {
                    oldestXpTime = entry.time;
                }
            }

            float timeSpan = currentTime - oldestXpTime;
            XPS = timeSpan > 0.1f ? totalXp / timeSpan : totalXp / XPS_WINDOW;
        }

        void CalculateGPS(float currentTime)
        {
            if (player == null || player.inventory == null)
            {
                return;
            }

            int currentGold = player.inventory.goldInt;

            if (currentGold != lastGold)
            {
                int goldGained = currentGold - lastGold;
                if (goldGained > 0)
                {
                    goldHistory.Enqueue((goldGained, currentTime));
                }
                lastGold = currentGold;
            }

            while (goldHistory.Count > 0 && goldHistory.Peek().time < currentTime - GPS_WINDOW)
            {
                goldHistory.Dequeue();
            }

            if (goldHistory.Count == 0)
            {
                GPS = 0f;
                return;
            }

            int totalGold = 0;
            float oldestGoldTime = currentTime;

            foreach (var entry in goldHistory)
            {
                totalGold += entry.gold;
                if (entry.time < oldestGoldTime)
                {
                    oldestGoldTime = entry.time;
                }
            }

            float timeSpan = currentTime - oldestGoldTime;
            GPS = timeSpan > 0.1f ? totalGold / timeSpan : totalGold / GPS_WINDOW;
        }
        

        void UpdateComboState(float currentTime)
        {
            if (!comboActive) return;

            if (currentTime - lastDamageTime > COMBO_WINDOW)
            {
                EndCombo();
                return;
            }

            currentComboTime = currentTime - currentComboStartTime;
        }

        void EndCombo()
        {
            comboActive = false;

            if (currentComboDamage > highestComboDamage)
            {
                highestComboDamage = currentComboDamage;
                highestComboHits = currentComboHits;
            }

            currentComboDamage = 0f;
            currentComboTime = 0f;
            currentComboHits = 0;
        }

        public static void AddDamageInstance(DamageInstance di)
        {
            var hud = FindFirstObjectByType<StatHUD>();
            if (hud != null && hud.isInitialized && hud.isPlaying)
            {
                hud.damageWindow.Enqueue((di.damage, hud.gameTime));
                hud.totalDamageInWindow += di.damage;
                
                if (!hud.comboActive)
                {
                    hud.comboActive = true;
                    hud.currentComboStartTime = hud.gameTime;
                    hud.currentComboDamage = 0f;
                    hud.currentComboHits = 0;
                }

                hud.currentComboDamage += di.damage;
                hud.currentComboHits++;
                hud.lastDamageTime = hud.gameTime;
            }
        }
    }
}

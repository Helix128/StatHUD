
using System;
using System.Runtime.InteropServices;
using FoxLib.UI;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Runtime;
using StatsHUD;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

class StatGUI : MonoBehaviour
{
    public Image mainPanel;
    public bool isCreated = false;
    public int lastScene = 0;
    public float lastCheck = 0;

    void Update()
    {
        if (Time.time - lastCheck > 0.01f)
        {
            lastCheck = Time.time;

            int currentScene = SceneManager.GetActiveScene().buildIndex;
            if (currentScene != lastScene)
            {
                lastScene = currentScene;
                if (currentScene == 2) //game scene
                {
                    Plugin.Log.LogInfo($"Enabling GUI.");
                    EnableGUI();
                }
                else if (isCreated && isVisible)
                {
                    Plugin.Log.LogInfo($"Disabling GUI.");
                    DisableGUI();
                }
            }
        }

        if (lastScene == 2)
        {
            TimeModule.Update();
            EnemyModule.Update();
            PlayerModule.Update();
            DPSModule.Update();
        }
        if (isCreated)
        {
            UpdateGUI();
        }
    }

    // Data text
    // resources
    public Text xpText;
    public Text xpSecText;
    public Text goldText;
    public Text goldSecText;

    // enemies
    public Text minDamageText;
    public Text maxDamageText;

    // damage
    public Text dpsText;

    public static Vector2 TargetPosition = new Vector2(800, -70);
    public static bool isVisible;

    private float _lastXp, _lastMaxXp, _lastXpPct;
    private float _lastGold, _lastGoldSec;
    private float _lastMinDmg, _lastMaxDmg;
    private float _lastDps;

    private string _xpString, _goldString, _minDmgString, _maxDmgString, _dpsString;
    void CreateGUI()
    {
        if (isCreated) { return; }

        mainPanel = UIBuilder.CreatePanel(TargetPosition, new Vector2(300, 300), "StatHUD Panel");

        var layoutGroup = mainPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.spacing = 10;
        layoutGroup.padding.top = 10;
        layoutGroup.padding.bottom = 10;
        layoutGroup.padding.left = 20;
        layoutGroup.padding.right = 10;
        layoutGroup.childForceExpandHeight = false;


        xpText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "XP: 0", 24, "XPText");
        xpText.transform.SetParent(mainPanel.transform, false);
        xpSecText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "XP/s: 0", 24, "XPSecText");
        xpSecText.transform.SetParent(mainPanel.transform, false);
        goldText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Gold: 0", 24, "GoldText");
        goldText.transform.SetParent(mainPanel.transform, false);
        goldSecText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Gold/s: 0", 24, "GoldSecText");
        goldSecText.transform.SetParent(mainPanel.transform, false);

        minDamageText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Min Foe Damage: 0", 24, "MinDamageText");
        minDamageText.transform.SetParent(mainPanel.transform, false);
        maxDamageText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Max Foe Damage: 0", 24, "MaxDamageText");
        maxDamageText.transform.SetParent(mainPanel.transform, false);

        dpsText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "DPS: 0", 24, "DPSText");
        dpsText.transform.SetParent(mainPanel.transform, false);

        Plugin.Log.LogInfo("StatGUI created.");
        isCreated = true;

        DisableGUI();
        UpdateGUIPosition();
        EnableGUI();
    }

    void EnableGUI()
    {
        if (!isCreated) { CreateGUI(); }

        isVisible = true;
        mainPanel.gameObject.SetActive(true);
    }

    void DisableGUI()
    {
        if (!isCreated) { CreateGUI(); }

        isVisible = false;
        mainPanel.gameObject.SetActive(false);

        DPSModule.Reset();
        PlayerModule.Reset();
        TimeModule.Reset();
        EnemyModule.Reset();
    }

    void UpdateGUI()
    {
        if (isVisible)
        {
            UpdateGUIText();
        }
        UpdateGUIPosition();
    }

    void UpdateGUIPosition()
    {
        if (isVisible)
        {
            mainPanel.rectTransform.anchoredPosition = Vector2.Lerp(mainPanel.rectTransform.anchoredPosition, TargetPosition, Time.deltaTime * 12);
        }
        else
        {
            mainPanel.rectTransform.anchoredPosition = new Vector2(TargetPosition.x + 550, TargetPosition.y);
        }
    }

    void UpdateGUIText()
    {
        float curXp = PlayerModule.GetXp();
        float curMaxXp = PlayerModule.GetMaxXp();
        float curXpPct = PlayerModule.GetXpPct();

        if (curXp != _lastXp || curMaxXp != _lastMaxXp || curXpPct != _lastXpPct)
        {
            _xpString = $"XP: {curXp.ToString("F0")}/{curMaxXp.ToString("F0")} ({curXpPct.ToString("F1")}%)";
            UIBuilder.SetText(xpText, _xpString);

            _lastXp = curXp;
            _lastMaxXp = curMaxXp;
            _lastXpPct = curXpPct;
        }

        float curGold = PlayerModule.GetGold();
        float curGoldSec = PlayerModule.GetGoldSec();
        if (curGold != _lastGold || curGoldSec != _lastGoldSec)
        {
            _goldString = $"Gold: {curGold.ToString("F0")}";
            UIBuilder.SetText(goldText, _goldString);

            _goldString = $"Gold/s: {curGoldSec.ToString("F0")}"; 
            UIBuilder.SetText(goldSecText, _goldString);

            _lastGold = curGold;
            _lastGoldSec = curGoldSec;
        }
        
        float curMinDmg = EnemyModule.MinDamage;
        float curMaxDmg = EnemyModule.MaxDamage;
        if (curMinDmg != _lastMinDmg || curMaxDmg != _lastMaxDmg)
        {
            _minDmgString = $"Min. Foe Dmg: {curMinDmg.ToString("F0")}";
            UIBuilder.SetText(minDamageText, _minDmgString);

            _maxDmgString = $"Max. Foe Dmg: {curMaxDmg.ToString("F0")}";
            UIBuilder.SetText(maxDamageText, _maxDmgString);

            _lastMinDmg = curMinDmg;
            _lastMaxDmg = curMaxDmg;

            var player = PlayerModule.GetPlayer();
            if (player)
            {
                if (player.inventory.playerHealth.WillDamageKill(EnemyModule.MinDamage, false))
                {
                    UIBuilder.SetColor(minDamageText, Color.red);
                }
                else
                {
                    UIBuilder.SetColor(minDamageText, Color.white);
                }

                if (player.inventory.playerHealth.WillDamageKill(EnemyModule.MaxDamage, false))
                {
                    UIBuilder.SetColor(maxDamageText, Color.red);
                }
                else
                {
                    UIBuilder.SetColor(maxDamageText, Color.white);
                }
            }
        }


        float curDps = DPSModule.GetDPS();
        if (curDps != _lastDps)
        {
            _dpsString = $"DPS: {curDps.ToString("F1")}";
            UIBuilder.SetText(dpsText, _dpsString);
            _lastDps = curDps;
        }
    }

}
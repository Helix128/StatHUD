
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


        xpText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "XP: 200", 24, "XPText");
        xpText.transform.SetParent(mainPanel.transform, false);
        xpSecText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "XP/s: 20", 24, "XPSecText");
        xpSecText.transform.SetParent(mainPanel.transform, false);
        goldText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Gold: 500", 24, "GoldText");
        goldText.transform.SetParent(mainPanel.transform, false);
        goldSecText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Gold/s: 50", 24, "GoldSecText");
        goldSecText.transform.SetParent(mainPanel.transform, false);

        minDamageText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Min Foe Damage: 100", 24, "MinDamageText");
        minDamageText.transform.SetParent(mainPanel.transform, false);
        maxDamageText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "Max Foe Damage: 200", 24, "MaxDamageText");
        maxDamageText.transform.SetParent(mainPanel.transform, false);

        dpsText = UIBuilder.CreateText(Vector2.zero, new Vector2(380, 72), "DPS: 200", 24, "DPSText");
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
        UIBuilder.SetText(xpText, $"XP: {PlayerModule.GetXp():0}/{PlayerModule.GetMaxXp():0} ({PlayerModule.GetXpPct():0.0}%)");
        UIBuilder.SetText(xpSecText, $"XP/s: {PlayerModule.GetXpSec():0}");
        UIBuilder.SetText(goldText, $"Gold: {PlayerModule.GetGold():0}");
        UIBuilder.SetText(goldSecText, $"Gold/s: {PlayerModule.GetGoldSec():0}");
        UIBuilder.SetText(minDamageText, $"Min Foe Damage: {EnemyModule.MinDamage:0}");
        UIBuilder.SetText(maxDamageText, $"Max Foe Damage: {EnemyModule.MaxDamage:0}");
        UIBuilder.SetText(dpsText, $"DPS: {DPSModule.GetDPS():0.0}");
    }
    
}
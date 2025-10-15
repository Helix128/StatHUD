
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using FoxUI;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace StatsHUD;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private static Harmony _harmony;

    public override void Load()
    {
        Log = base.Log;
        Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} is loaded!");

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();
        
        ClassInjector.RegisterTypeInIl2Cpp<AssetManager>();
        var assetManager = new GameObject("AssetManager");
        assetManager.AddComponent<AssetManager>();
        Object.DontDestroyOnLoad(assetManager);

        ClassInjector.RegisterTypeInIl2Cpp<UIBuilder>();
        var uiBuilder = new GameObject("UIBuilder");
        uiBuilder.AddComponent<UIBuilder>();
        Object.DontDestroyOnLoad(uiBuilder);

        ClassInjector.RegisterTypeInIl2Cpp<StatGUI>();
        var statGUI = new GameObject("StatGUI");
        statGUI.AddComponent<StatGUI>();
        Object.DontDestroyOnLoad(statGUI);
    }

}

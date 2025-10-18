
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using FoxLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using FoxLib.UI;

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

        FoxUI.Initialize();

        ClassInjector.RegisterTypeInIl2Cpp<StatGUI>();
        var statGUI = new GameObject("StatHUD");
        statGUI.AddComponent<StatGUI>();
        Object.DontDestroyOnLoad(statGUI);
    }

}

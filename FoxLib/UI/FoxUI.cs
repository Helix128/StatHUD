using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace FoxLib.UI
{
    public class FoxUI
    {
        public static void Initialize()
        {
            FoxLib.Initialize();
            ClassInjector.RegisterTypeInIl2Cpp<UIBuilder>();
            var uiBuilder = new GameObject("FoxLib UI Builder");
            uiBuilder.AddComponent<UIBuilder>();
            Object.DontDestroyOnLoad(uiBuilder);
        }
    }
}
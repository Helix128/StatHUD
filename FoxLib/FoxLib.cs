using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace FoxLib
{
    public class FoxLib
    {
        public static void Initialize()
        {
            ClassInjector.RegisterTypeInIl2Cpp<AssetManager>();
            var assetManager = new GameObject("FoxLib Asset Manager");
            assetManager.AddComponent<AssetManager>();
            Object.DontDestroyOnLoad(assetManager);
        }
    }
}
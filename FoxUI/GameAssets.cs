using UnityEngine;
using StatsHUD;
using System.Linq;

namespace FoxUI
{
    public class AssetManager : MonoBehaviour
    {
        // REFERENCES TO COMMON GAME ASSETS
        public static Font font; // alagard
        public static Sprite border; // Border2_Gray

        public static Font GetFont()
        {
            if (font == null)
            {   
                Plugin.Log.LogInfo("Loading font...");
                font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(f => f.name == "alagard");
                if (font == null)
                {
                    Plugin.Log.LogError("Font not found!");
                }
                else
                {
                    Plugin.Log.LogInfo("Font loaded successfully.");
                }
            }
            return font;
        }
        public static Sprite GetBorder()
        {
            if (border == null)
            {   Plugin.Log.LogInfo("Loading border sprite...");
                border = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Border2_Gray");
                if (border == null)
                {
                    Plugin.Log.LogError("Border sprite not found!");
                }
                else
                {
                    Plugin.Log.LogInfo("Border sprite loaded successfully.");
                }
            }
            return border;
        }

        public void Start()
        {
            Plugin.Log.LogInfo("AssetManager initialized.");
        }   
        
    }
}
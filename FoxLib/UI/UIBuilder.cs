using System.Collections.Generic;
using System.Text;
using StatsHUD;
using UnityEngine;
using UnityEngine.UI;

namespace FoxLib.UI
{
    public class UIBuilder : MonoBehaviour
    {
        public static Canvas canvas;
        public static UIBuilder Instance;
        public void Start()
        {   
            if(Instance != null)
            {
                Plugin.Log.LogWarning("Multiple UI Builder instances detected! Destroying duplicate");
                Destroy(this);
                return;
            }
            Instance = this;

            Plugin.Log.LogInfo("FoxLib UI Builder initialized.");

            GameObject canvasGo = new GameObject("FoxLib UI");
            DontDestroyOnLoad(canvasGo);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            canvasGo.AddComponent<GraphicRaycaster>();

            Plugin.Log.LogInfo("FoxLib UI canvas created.");
        }

        public static Image CreatePanel(Vector2 position, Vector2 size, string name = "Panel")
        {
            Plugin.Log.LogInfo($"Creating panel: {name} at {position} with size {size}");

            Image panel = new GameObject(name).AddComponent<Image>();
 
            panel.rectTransform.anchoredPosition = position;
            panel.rectTransform.sizeDelta = size;

            panel.sprite = AssetManager.GetBorder();
            panel.type = Image.Type.Sliced;
            panel.color = new Color(1f, 1f, 1f, 0.8f);

            panel.transform.SetParent(canvas.transform, false);

            return panel;
        }

        public static Text CreateText(Vector2 position, Vector2 size, string content, int fontSize = 24, string name = "Text")
        {
            Plugin.Log.LogInfo($"Creating text: {name} at {position} with size {size} and content: {content}");

            Text text = new GameObject(name).AddComponent<Text>();

            text.rectTransform.anchoredPosition = position;
            text.rectTransform.sizeDelta = size;

            text.font = AssetManager.GetFont();
            text.text = content;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2, -2);
            text.transform.SetParent(canvas.transform, false);

            return text;
        }


        public static void SetText(Text text, string content)
        {
            if (text.text != content)
            {
                text.text = content;
            }
        }

        public static void SetColor(Text text, Color color)
        {
            if (text.color != color)
            {
                text.color = color;
            }
        }

        public static void SetColor(Text text, float r = 1.0f, float g = 1.0f, float b = 1.0f, float a = 1f)
        {
            Color color = new Color(r, g, b, a);
            if (text.color != color)
            {
                text.color = color;
            }
        }
    }
}
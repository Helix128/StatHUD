using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace StatsHUD
{
    public class StatsUIPanel : MonoBehaviour
    {
        public static StatsUIPanel Instance { get; private set; }
        private Canvas _canvas;
        private RectTransform _panel;
        private VerticalLayoutGroup _layoutGroup;
        private readonly Dictionary<string, Text> _statTexts = new Dictionary<string, Text>();
        private static Font gameFont;
        private static Sprite borderSprite;
        private bool _initialized = false;
        
        private Vector2 _targetPosition;
        private Vector2 _hiddenPosition;
        
        private bool _slideActive = false;
        private bool _slideOut = false;
        private float _slideTime = 0f;
        private float _slideDuration = 0f;
        private Vector2 _slideStart;
        private Vector2 _slideTarget;

        public void Initialize()
        {
            if (_initialized) return;

            InitializeResources();
            CreateCanvas();
            _initialized = true;
        }

        public void EnablePanel()
        {
            if (_canvas != null) SlideIn();
        }

        public void DisablePanel()
        {
            if (_canvas != null) SlideOut();
        }

        public void ResetPanel()
        {
            foreach (var statText in _statTexts.Values)
            {
                if (statText != null)
                {
                    statText.text = "";
                    statText.gameObject.SetActive(false);
                }
            }
        }
        
        private void SlideIn()
        {
            _canvas.gameObject.SetActive(true);
            _slideActive = true;
            _slideOut = false;
            _slideDuration = 0.3f;
            _slideTime = 0f;
            _slideStart = _panel.anchoredPosition;
            _slideTarget = _targetPosition;
        }

        private void SlideOut()
        {
            _slideActive = true;
            _slideOut = true;
            _slideDuration = 0.3f;
            _slideTime = 0f;
            _slideStart = _panel.anchoredPosition;
            _slideTarget = _hiddenPosition;
        }

        private static void InitializeResources()
        {
            if (gameFont == null)
            {
                gameFont = Resources
                    .FindObjectsOfTypeAll<Font>()
                    .FirstOrDefault(f => f.name == "alagard");

                if (gameFont != null && gameFont.material?.mainTexture != null)
                    gameFont.material.mainTexture.filterMode = FilterMode.Point;
            }

            if (borderSprite == null)
            {
                borderSprite = Resources
                    .FindObjectsOfTypeAll<Sprite>()
                    .FirstOrDefault(s => s.name == "Border2_Gray");
            }
        }

        private void CreateCanvas()
        {
            Instance = this;

            GameObject canvas = new GameObject("StatsHUDCanvas");
            canvas.transform.SetParent(transform, false);

            _canvas = canvas.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = -100;

            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            canvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("StatsPanel");
            panel.transform.SetParent(canvas.transform, false);
            _panel = panel.AddComponent<RectTransform>();

            Image image = panel.AddComponent<Image>();
            if (borderSprite != null)
            {
                image.sprite = borderSprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            else
            {
                image.color = new Color(0f, 0f, 0f, 0.7f);
            }
            image.color = image.color * 0.85f;
            _panel.anchorMin = new Vector2(1f, 0.5f);
            _panel.anchorMax = new Vector2(1f, 0.5f);
            _panel.pivot = new Vector2(1f, 0.5f);
            _targetPosition = new Vector2(-20f, -10f);
            _hiddenPosition = new Vector2(300f, -10f);
            _panel.anchoredPosition = _hiddenPosition;
            _panel.sizeDelta = new Vector2(280f, 300f);

            _layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
            _layoutGroup.childAlignment = TextAnchor.UpperLeft;
            _layoutGroup.spacing = 5;
            _layoutGroup.padding.bottom = 10;
            _layoutGroup.padding.top = 20;
            _layoutGroup.padding.left = 20;
            _layoutGroup.padding.right = 10;

            _layoutGroup.childControlHeight = false;
            _layoutGroup.childControlWidth = true;
            _layoutGroup.childForceExpandHeight = false;
            _layoutGroup.childForceExpandWidth = true;

            CreateStatLabels();

            _canvas.gameObject.SetActive(false);
        }

        private void CreateStatLabels()
        {
            string[] statKeys = new string[]
            {
                "xp",
                "xps",
                "gold",
                "gps",
                "maxEnemyDmg",
                "minEnemyDmg",
                "dps"
            };

            foreach (string key in statKeys)
            {
                _statTexts[key] = CreateStatText(key);
            }
            
            _statTexts["maxCombo"] = CreateMultiLineStatText("maxCombo", 60);
        }

        private Text CreateStatText(string key)
        {
            GameObject textObj = new GameObject($"Stat_{key}");
            textObj.transform.SetParent(_panel, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 24);

            Text text = textObj.AddComponent<Text>();
            text.font = gameFont;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = ""; 
            
            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);

            LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 24;
            layoutElement.preferredHeight = 24;

            textObj.SetActive(false);
            return text;
        }

        private Text CreateMultiLineStatText(string key, float height)
        {
            GameObject textObj = new GameObject($"Stat_{key}");
            textObj.transform.SetParent(_panel, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, height);

            Text text = textObj.AddComponent<Text>();
            text.font = gameFont;
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = ""; 

            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);

            LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;

            textObj.SetActive(false);
            return text;
        }

        private void Update()
        {
            if (_slideActive)
            {
                _slideTime += Time.deltaTime;
                float t = Mathf.Clamp01(_slideTime / _slideDuration);
                t = t * t * (3f - 2f * t);
                _panel.anchoredPosition = Vector2.Lerp(_slideStart, _slideTarget, t);

                if (t >= 1f)
                {
                    if (_slideOut) _canvas.gameObject.SetActive(false);
                    _slideActive = false;
                }
            }
        }

        public void UpdateStats(StatData data)
        {
            if (!_initialized) return;

            UpdateStatText("xp", $"XP: {data.currentXp:F0}/{data.maxXp:F0} ({data.xpPercent * 100:F0}%)");
            UpdateStatText("xps", $"XP/s: {data.xps:F1}");
            UpdateStatText("gold", $"Gold: {data.gold}");
            UpdateStatText("gps", $"Gold/s: {data.gps:F1}");
            
            if (data.maxEnemyDmg > 0)
            {
                UpdateStatText("maxEnemyDmg", $"Max. Enemy Dmg: {data.maxEnemyDmg:F1}");
                
                if (_statTexts.ContainsKey("maxEnemyDmg") && _statTexts["maxEnemyDmg"] != null)
                {
                    if (data.maxDangerLevel == 2)
                        _statTexts["maxEnemyDmg"].color = Color.red;
                    else if (data.maxDangerLevel == 1)
                        _statTexts["maxEnemyDmg"].color = Color.yellow;
                    else
                        _statTexts["maxEnemyDmg"].color = Color.white;
                }
            }
            else
            {
                UpdateStatText("maxEnemyDmg", $"Max. Enemy Dmg: safe :)");
                if (_statTexts.ContainsKey("maxEnemyDmg") && _statTexts["maxEnemyDmg"] != null)
                {
                    _statTexts["maxEnemyDmg"].color = Color.white;
                }
            }

            if (data.minEnemyDmg > 0)
            {
                UpdateStatText("minEnemyDmg", $"Min. Enemy Dmg: {data.minEnemyDmg:F1}");
                if (_statTexts.ContainsKey("minEnemyDmg") && _statTexts["minEnemyDmg"] != null)
                {
                    if (data.minDangerLevel == 2)
                        _statTexts["minEnemyDmg"].color = Color.red;
                    else if (data.minDangerLevel == 1)
                        _statTexts["minEnemyDmg"].color = Color.yellow;
                    else
                        _statTexts["minEnemyDmg"].color = Color.white;
                }
            }
            else
            {
                UpdateStatText("minEnemyDmg", $"Min. Enemy Dmg: safe :)");
                if (_statTexts.ContainsKey("minEnemyDmg") && _statTexts["minEnemyDmg"] != null)
                {
                    _statTexts["minEnemyDmg"].color = Color.white;
                }
            }

            UpdateStatText("dps", $"DPS: {data.dps:F1}");
            UpdateStatText("maxCombo", $"Best Combo\n  - {data.maxComboDamage:F0} damage\n  - {data.maxComboHits} hits");
        }

        private void UpdateStatText(string key, string text)
        {
            if (_statTexts.ContainsKey(key) && _statTexts[key] != null)
            {
                _statTexts[key].text = text;
                _statTexts[key].gameObject.SetActive(true);
            }
        }
    }

    public struct StatData
    {
        public int currentXp;
        public float maxXp;
        public float xpPercent;
        public float xps;
        public int gold;
        public float gps;
        public float stageDmgMult;
        public float timeDmgMult;
        public float totalDmgMult;
        public float minutes;
        public float maxEnemyDmg;
        public float minEnemyDmg;
        public int maxDangerLevel;
        public int minDangerLevel;
        public float dps;
        public float maxComboDamage;
        public int maxComboHits;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace StatsHUD
{
    public class ComboUIPanel : MonoBehaviour
    {
        public static ComboUIPanel Instance { get; private set; }
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

        private bool _punchActive = false;
        private float _punchTime = 0f;
        private int _lastHitCount = 0;

        ComboStatData data;
        public void Initialize()
        {
            if (_initialized) return;

            InitializeResources();
            CreateCanvas();
            _initialized = true;
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

            GameObject canvas = new GameObject("ComboHUDCanvas");
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

            GameObject panel = new GameObject("ComboPanel");
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
                image.color = new Color(0f, 0f, 0f, 0.85f);
            }
            image.color = image.color * 0.85f;

            _panel.anchorMin = new Vector2(1f, 0.5f);
            _panel.anchorMax = new Vector2(1f, 0.5f);
            _panel.pivot = new Vector2(1f, 1f);
            _targetPosition = new Vector2(-20f, -160f);
            _hiddenPosition = new Vector2(300f, -160f);
            _panel.anchoredPosition = _hiddenPosition;
            _panel.sizeDelta = new Vector2(280f, 180f);

            _layoutGroup = panel.AddComponent<VerticalLayoutGroup>();
            _layoutGroup.childAlignment = TextAnchor.UpperCenter;
            _layoutGroup.spacing = 8;
            _layoutGroup.padding.bottom = 15;
            _layoutGroup.padding.top = 20;
            _layoutGroup.padding.left = 20;
            _layoutGroup.padding.right = 20;

            _layoutGroup.childControlHeight = false;
            _layoutGroup.childControlWidth = true;
            _layoutGroup.childForceExpandHeight = false;
            _layoutGroup.childForceExpandWidth = true;

            CreateStatLabels();
            
            _canvas.gameObject.SetActive(false);
        }

        private void CreateStatLabels()
        {
            _statTexts["currentHits"] = CreateLargeHitText();
            _statTexts["currentCombo"] = CreateStatText("currentCombo", 20);
            _statTexts["comboTimer"] = CreateStatText("comboTimer", 18);
        }

        private Text CreateLargeHitText()
        {
            GameObject textObj = new GameObject("Stat_currentHits");
            textObj.transform.SetParent(_panel, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 60);

            Text text = textObj.AddComponent<Text>();
            text.font = gameFont;
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "";

            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(4f, -4f);

            LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
            layoutElement.minHeight = 60;
            layoutElement.preferredHeight = 60;

            textObj.SetActive(false);
            return text;
        }

        private Text CreateStatText(string key, int fontSize)
        {
            GameObject textObj = new GameObject($"Stat_{key}");
            textObj.transform.SetParent(_panel, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 24);

            Text text = textObj.AddComponent<Text>();
            text.font = gameFont;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
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
            _lastHitCount = 0;
            _punchActive = false;
            _punchTime = 0f;
        }

        private void SlideIn()
        {
            _canvas.gameObject.SetActive(true);
            _slideActive = true;
            _slideOut = false;
            _slideDuration = 0.045f;
            _slideTime = 0f;
            _slideStart = _panel.anchoredPosition;
            _slideTarget = _targetPosition;
        }

        private void SlideOut()
        {
            _slideActive = true;
            _slideOut = true;
            _slideDuration = 0.045f;
            _slideTime = 0f;
            _slideStart = _panel.anchoredPosition;
            _slideTarget = _hiddenPosition;
        }

        private void StartPunch()
        {
            if (_statTexts.ContainsKey("currentHits"))
            {
                _punchActive = true;
                _punchTime = 0f;
            }
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

            if (_punchActive)
            {
                _punchTime += Time.deltaTime;
                float t = Mathf.Clamp01(_punchTime / 0.15f);

                if (_statTexts.ContainsKey("currentHits") && _statTexts["currentHits"] != null)
                {
                    Text hitText = _statTexts["currentHits"];
                    int size = t < 0.5f
                        ? (int)Mathf.Lerp(42, 56, t * 2f)
                        : (int)Mathf.Lerp(56, 42, (t - 0.5f) * 2f);
                    hitText.fontSize = size + Mathf.FloorToInt(size * 0.1f);
                }

                if (t >= 1f)
                {
                    if (_statTexts.ContainsKey("currentHits"))
                        _statTexts["currentHits"].fontSize = 48;
                    _punchActive = false;
                }
            }
        }

        public void UpdateComboStats(ComboStatData data)
        {
            if (!_initialized) return;
            this.data = data;
            if (data.comboActive)
            {
                if (data.currentComboHits > _lastHitCount)
                {
                    StartPunch();
                    _lastHitCount = data.currentComboHits;
                }

                UpdateStatText("currentHits", $"{data.currentComboHits} hits");
                UpdateStatText("currentCombo", $"{data.currentComboDamage:F0} damage");

                float remainingTime = data.comboRemainingTime;
                UpdateStatText("comboTimer", $"{remainingTime:F1}s");
            }
            else
            {
                _lastHitCount = 0;
            }
        }

        private void UpdateStatText(string key, string text)
        {
            if (_statTexts.ContainsKey(key))
            {
                _statTexts[key].text = text;
                _statTexts[key].gameObject.SetActive(true);
            }
        }
    }
}

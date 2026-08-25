using UnityEngine;
using UnityEngine.UI;

namespace CyberRakshak.Platformer
{
    /// <summary>Creates a minimal on-screen HP bar without adding scene-specific UI dependencies.</summary>
    public sealed class PlatformerHud : MonoBehaviour
    {
        public static PlatformerHud Instance { get; private set; }

        private Image fill;
        private Text label;

        public static void EnsureCreated()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject root = new GameObject("PlatformerHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PlatformerHud));
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Build();
            SetHealth(PlayerHealth.MaxHealth, PlayerHealth.MaxHealth);
        }

        public void SetHealth(int current, int max)
        {
            if (fill == null)
            {
                return;
            }

            float value = current / (float)max;
            fill.fillAmount = value;
            fill.color = value > 0.5f ? new Color(0.15f, 0.9f, 0.95f) : new Color(1f, 0.35f, 0.25f);
            label.text = $"SYSTEM INTEGRITY  {current}/{max}";
        }

        private void Build()
        {
            RectTransform panel = CreateImage("HealthPanel", transform, new Color(0.015f, 0.04f, 0.1f, 0.9f));
            Anchor(panel, new Vector2(0.035f, 0.88f), new Vector2(0.30f, 0.955f));

            RectTransform background = CreateImage("HealthBarBackground", panel, new Color(0.05f, 0.1f, 0.17f, 1f));
            Anchor(background, new Vector2(0.05f, 0.17f), new Vector2(0.95f, 0.52f));
            fill = CreateImage("HealthBarFill", background, Color.cyan).GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            Anchor(fill.rectTransform, Vector2.zero, Vector2.one);

            label = CreateText("HealthLabel", panel);
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = 22;
            Anchor(label.rectTransform, new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.93f));
        }

        private static RectTransform CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        private static Text CreateText(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            return text;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

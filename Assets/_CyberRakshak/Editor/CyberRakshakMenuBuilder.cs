using System.Reflection;
using CyberRakshak.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CyberRakshakMenuBuilder
{
    private static readonly Color Navy = new Color(0.025f, 0.055f, 0.12f, 1f);
    private static readonly Color NavySoft = new Color(0.055f, 0.1f, 0.2f, 0.96f);
    private static readonly Color Cyan = new Color(0.16f, 0.85f, 1f, 1f);
    private static readonly Color Coral = new Color(1f, 0.37f, 0.3f, 1f);
    private static readonly Color White = new Color(0.94f, 0.98f, 1f, 1f);
    private static readonly Color Muted = new Color(0.6f, 0.7f, 0.82f, 1f);

    [MenuItem("CyberRakshak/Build Menu UI")]
    public static void Build()
    {
        var existing = GameObject.Find("CyberRakshakMenuUI");
        if (existing != null) Object.DestroyImmediate(existing);
        var previousEvent = Object.FindFirstObjectByType<EventSystem>();
        if (previousEvent != null) Object.DestroyImmediate(previousEvent.gameObject);

        var root = new GameObject("CyberRakshakMenuUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(root.transform, false);

        var bg = Image("Background", root.transform, Navy);
        Stretch(bg.rectTransform);
        var accent = Image("TopAccent", root.transform, new Color(Cyan.r, Cyan.g, Cyan.b, 0.18f));
        Place(accent.rectTransform, new Vector2(0f, .92f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

        var menu = Panel("MainMenu", root.transform, Color.clear);
        Stretch(menu.rectTransform);
        var wordmark = Text("Wordmark", menu.transform, "CYBERRAKSHAK", 76, White, FontStyles.Bold);
        Place(wordmark.rectTransform, new Vector2(.075f,.79f), new Vector2(.48f,.91f), Vector2.zero, Vector2.zero);
        var sub = Text("Subtitle", menu.transform, "ETHICAL HACKING TRAINING SIMULATION", 19, Cyan, FontStyles.Bold);
        Place(sub.rectTransform, new Vector2(.079f,.745f), new Vector2(.49f,.79f), Vector2.zero, Vector2.zero);

        var statement = Text("Statement", menu.transform, "Train your instinct.\nProtect the system.", 33, White, FontStyles.Normal);
        statement.lineSpacing = 1.15f;
        Place(statement.rectTransform, new Vector2(.079f,.58f), new Vector2(.46f,.71f), Vector2.zero, Vector2.zero);
        var rail = Image("SelectionRail", menu.transform, Cyan);
        Place(rail.rectTransform, new Vector2(.082f,.285f), new Vector2(.085f,.52f), Vector2.zero, Vector2.zero);

        var start = MenuButton("StartTrainingButton", menu.transform, "START TRAINING", true);
        Place(start.GetComponent<RectTransform>(), new Vector2(.105f,.465f), new Vector2(.41f,.525f), Vector2.zero, Vector2.zero);
        var continueButton = MenuButton("ContinueButton", menu.transform, "CONTINUE", false);
        Place(continueButton.GetComponent<RectTransform>(), new Vector2(.105f,.395f), new Vector2(.41f,.455f), Vector2.zero, Vector2.zero);
        var settings = MenuButton("MainSettingsButton", menu.transform, "SETTINGS", false);
        Place(settings.GetComponent<RectTransform>(), new Vector2(.105f,.325f), new Vector2(.41f,.385f), Vector2.zero, Vector2.zero);
        var exit = MenuButton("ExitButton", menu.transform, "EXIT", false);
        Place(exit.GetComponent<RectTransform>(), new Vector2(.105f,.255f), new Vector2(.41f,.315f), Vector2.zero, Vector2.zero);
        var hint = Text("Hint", menu.transform, "PATCH IS READY FOR A NEW TRAINING RUN.", 16, Muted, FontStyles.Normal);
        Place(hint.rectTransform, new Vector2(.079f,.13f), new Vector2(.48f,.18f), Vector2.zero, Vector2.zero);

        var patchWindow = Image("PatchWindow", menu.transform, new Color(.04f,.095f,.18f,.78f));
        Place(patchWindow.rectTransform, new Vector2(.56f,.10f), new Vector2(.95f,.88f), Vector2.zero, Vector2.zero);
        var patchLabel = Text("PatchLabel", patchWindow.transform, "PATCH // CYBER GUIDE", 16, Cyan, FontStyles.Bold);
        Place(patchLabel.rectTransform, new Vector2(.07f,.86f), new Vector2(.9f,.95f), Vector2.zero, Vector2.zero);
        var patchMessage = Text("PatchMessage", patchWindow.transform, "“I checked the firewall.\nIt blinked first.”", 25, White, FontStyles.Italic);
        Place(patchMessage.rectTransform, new Vector2(.07f,.06f), new Vector2(.9f,.2f), Vector2.zero, Vector2.zero);
        BuildPatchPreview(patchWindow.transform);

        var pause = Panel("PauseMenu", root.transform, new Color(0f,0.015f,.045f,.76f));
        Stretch(pause.rectTransform);
        var pauseCard = Image("PauseCard", pause.transform, NavySoft);
        Place(pauseCard.rectTransform, new Vector2(.34f,.25f), new Vector2(.66f,.75f), Vector2.zero, Vector2.zero);
        var pauseTitle = Text("PauseTitle", pauseCard.transform, "PAUSED", 52, White, FontStyles.Bold);
        pauseTitle.alignment = TextAnchor.MiddleCenter;
        Place(pauseTitle.rectTransform, new Vector2(.08f,.73f), new Vector2(.92f,.9f), Vector2.zero, Vector2.zero);
        var pauseSub = Text("PauseSub", pauseCard.transform, "PATCH IS HOLDING THE LINE.", 15, Cyan, FontStyles.Bold);
        pauseSub.alignment = TextAnchor.MiddleCenter;
        Place(pauseSub.rectTransform, new Vector2(.08f,.64f), new Vector2(.92f,.73f), Vector2.zero, Vector2.zero);
        var resume = ModalButton("ResumeButton", pauseCard.transform, "RESUME TRAINING", Cyan);
        Place(resume.GetComponent<RectTransform>(), new Vector2(.17f,.45f), new Vector2(.83f,.56f), Vector2.zero, Vector2.zero);
        var pauseSettings = ModalButton("PauseSettingsButton", pauseCard.transform, "SETTINGS", new Color(.12f,.22f,.36f,1f));
        Place(pauseSettings.GetComponent<RectTransform>(), new Vector2(.17f,.31f), new Vector2(.83f,.42f), Vector2.zero, Vector2.zero);
        var returnMenu = ModalButton("ReturnMenuButton", pauseCard.transform, "RETURN TO MENU", new Color(.12f,.22f,.36f,1f));
        Place(returnMenu.GetComponent<RectTransform>(), new Vector2(.17f,.17f), new Vector2(.83f,.28f), Vector2.zero, Vector2.zero);

        var settingsPanel = Panel("SettingsMenu", root.transform, new Color(0f,0.015f,.045f,.84f));
        Stretch(settingsPanel.rectTransform);
        var settingsCard = Image("SettingsCard", settingsPanel.transform, NavySoft);
        Place(settingsCard.rectTransform, new Vector2(.31f,.19f), new Vector2(.69f,.81f), Vector2.zero, Vector2.zero);
        var settingsTitle = Text("SettingsTitle", settingsCard.transform, "SETTINGS", 48, White, FontStyles.Bold);
        settingsTitle.alignment = TextAnchor.MiddleCenter;
        Place(settingsTitle.rectTransform, new Vector2(.08f,.78f), new Vector2(.92f,.92f), Vector2.zero, Vector2.zero);
        var settingsLine = Image("SettingsLine", settingsCard.transform, Cyan);
        Place(settingsLine.rectTransform, new Vector2(.12f,.75f), new Vector2(.88f,.758f), Vector2.zero, Vector2.zero);
        var music = SettingSlider("MusicSlider", settingsCard.transform, "MUSIC", .8f);
        Place(music.GetComponent<RectTransform>(), new Vector2(.13f,.59f), new Vector2(.87f,.68f), Vector2.zero, Vector2.zero);
        var sfx = SettingSlider("SfxSlider", settingsCard.transform, "SFX", .7f);
        Place(sfx.GetComponent<RectTransform>(), new Vector2(.13f,.45f), new Vector2(.87f,.54f), Vector2.zero, Vector2.zero);
        var sensitivity = SettingSlider("SensitivitySlider", settingsCard.transform, "CAMERA SENSITIVITY", .62f);
        Place(sensitivity.GetComponent<RectTransform>(), new Vector2(.13f,.31f), new Vector2(.87f,.4f), Vector2.zero, Vector2.zero);
        var back = ModalButton("SettingsBackButton", settingsCard.transform, "BACK", new Color(.12f,.22f,.36f,1f));
        Place(back.GetComponent<RectTransform>(), new Vector2(.34f,.10f), new Vector2(.66f,.20f), Vector2.zero, Vector2.zero);

        pause.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        var controller = root.AddComponent<CyberRakshakMenuController>();
        Set(controller, "mainMenu", menu.gameObject);
        Set(controller, "pauseMenu", pause.gameObject);
        Set(controller, "settingsMenu", settingsPanel.gameObject);
        Set(controller, "mainStartButton", start);
        Set(controller, "pauseResumeButton", resume);
        Set(controller, "settingsBackButton", back);
        Set(controller, "musicSlider", music);
        Set(controller, "sfxSlider", sfx);
        Set(controller, "sensitivitySlider", sensitivity);
        Set(controller, "cameraController", Object.FindFirstObjectByType<ThirdPersonCameraController>());

        start.onClick.AddListener(controller.StartTraining);
        continueButton.onClick.AddListener(controller.ContinueTraining);
        settings.onClick.AddListener(controller.OpenMainSettings);
        exit.onClick.AddListener(controller.ExitGame);
        resume.onClick.AddListener(controller.ResumeTraining);
        pauseSettings.onClick.AddListener(controller.OpenPauseSettings);
        returnMenu.onClick.AddListener(controller.OpenMainMenu);
        back.onClick.AddListener(controller.CloseSettings);
        music.onValueChanged.AddListener(controller.SetMusic);
        sfx.onValueChanged.AddListener(controller.SetSfx);
        sensitivity.onValueChanged.AddListener(controller.SetCameraSensitivity);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("CyberRakshak UI built successfully.");
    }

    private static void BuildPatchPreview(Transform parent)
    {
        var source = GameObject.Find("PATCH_ShoulderCompanion");
        if (source == null) return;
        var rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32) { name = "PatchMenuPreviewRT" };
        var clone = Object.Instantiate(source);
        clone.name = "MenuPatchPreviewWorld";
        clone.transform.position = new Vector3(10000f, 0f, 0f);
        clone.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        var renderers = clone.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        clone.transform.position += new Vector3(10000f - bounds.center.x, 1.6f - bounds.center.y, 0f - bounds.center.z);
        bounds.center = new Vector3(10000f, 1.6f, 0f);
        var cameraObject = new GameObject("MenuPatchPreviewCamera", typeof(Camera));
        var camera = cameraObject.GetComponent<Camera>();
        cameraObject.transform.position = new Vector3(10000f, 1.6f, -12f);
        cameraObject.transform.LookAt(bounds.center);
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(bounds.size.y, bounds.size.x) * .72f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.04f,.095f,.18f,1f);
        camera.targetTexture = rt;
        camera.enabled = true;
        var raw = new GameObject("PatchPreview", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        raw.transform.SetParent(parent, false);
        raw.texture = rt;
        raw.color = Color.white;
        Place(raw.rectTransform, new Vector2(.04f,.22f), new Vector2(.96f,.84f), Vector2.zero, Vector2.zero);
    }

    private static Image Panel(string name, Transform parent, Color color)
    {
        return Image(name, parent, color);
    }

    private static Image Image(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static UnityEngine.UI.Text Text(string name, Transform parent, string content, float size, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = Mathf.RoundToInt(size);
        text.color = color;
        text.fontStyle = (style & FontStyles.Bold) != 0 ? FontStyle.Bold : ((style & FontStyles.Italic) != 0 ? FontStyle.Italic : FontStyle.Normal);
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button MenuButton(string name, Transform parent, string label, bool primary)
    {
        var image = Image(name, parent, primary ? new Color(Cyan.r,Cyan.g,Cyan.b,.13f) : new Color(1f,1f,1f,0f));
        var button = image.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white; colors.highlightedColor = new Color(1f,1f,1f,.92f); colors.pressedColor = new Color(1f,1f,1f,.72f);
        button.colors = colors;
        var t = Text("Label", image.transform, label, 28, primary ? White : Muted, FontStyles.Bold);
        t.alignment = TextAnchor.MiddleLeft;
        Place(t.rectTransform, new Vector2(.06f,0), new Vector2(.96f,1), Vector2.zero, Vector2.zero);
        return button;
    }

    private static Button ModalButton(string name, Transform parent, string label, Color color)
    {
        var image = Image(name, parent, color);
        var button = image.gameObject.AddComponent<Button>();
        var t = Text("Label", image.transform, label, 20, White, FontStyles.Bold);
        t.alignment = TextAnchor.MiddleCenter;
        Stretch(t.rectTransform);
        return button;
    }

    private static Slider SettingSlider(string name, Transform parent, string label, float value)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        var labelText = Text("Label", root.transform, label, 17, Muted, FontStyles.Bold);
        Place(labelText.rectTransform, new Vector2(0,.45f), new Vector2(1,1), Vector2.zero, Vector2.zero);
        var track = Image("Track", root.transform, new Color(.15f,.24f,.36f,1f));
        Place(track.rectTransform, new Vector2(0,.14f), new Vector2(1,.30f), Vector2.zero, Vector2.zero);
        var fillArea = new GameObject("FillArea", typeof(RectTransform)).GetComponent<RectTransform>();
        fillArea.SetParent(track.transform, false);
        Stretch(fillArea);
        var fill = Image("Fill", fillArea, Cyan);
        Stretch(fill.rectTransform);
        var handleArea = new GameObject("HandleArea", typeof(RectTransform)).GetComponent<RectTransform>();
        handleArea.SetParent(track.transform, false);
        Stretch(handleArea);
        var handle = Image("Handle", handleArea, White);
        handle.rectTransform.sizeDelta = new Vector2(20,20);
        var slider = root.GetComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 1; slider.value = value;
        slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
    }

    private static void Stretch(RectTransform rect) => Place(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

    private static void Set(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
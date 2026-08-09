using System.Reflection;
using CyberRakshak.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CyberRakshakSceneBuilder
{
    private static readonly Color Navy = new Color(.015f, .035f, .085f, 1f);
    private static readonly Color Card = new Color(.055f, .105f, .19f, .96f);
    private static readonly Color Cyan = new Color(.2f, .85f, 1f, 1f);
    private static readonly Color Coral = new Color(1f, .38f, .32f, 1f);
    private static readonly Color White = new Color(.95f, .98f, 1f, 1f);
    private static readonly Color Muted = new Color(.52f, .64f, .78f, 1f);
    private const string BackgroundPath = "Assets/_CyberRakshak/Art/UI/MainMenu_Background_1920x1080.png";

    [MenuItem("CyberRakshak/Build Scene Flow")]
    public static void BuildSceneFlow()
    {
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

        BuildSplash();
        BuildMainMenu();
        BuildLevelSelect();
        BuildTutorialScene();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/Splash.unity", true),
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/LevelSelect.unity", true),
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CyberRakshak scene flow built: Splash -> Main Menu -> Level Select -> Tutorial.");
    }

    private static void BuildSplash()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddSceneCameraAndLight();
        var canvas = CanvasRoot("SplashUI");
        Background(canvas.transform);
        var title = Text("Title", canvas.transform, "CYBERRAKSHAK", 92, White, FontStyle.Bold);
        title.alignment = TextAnchor.MiddleCenter;
        Place(title.rectTransform, .12f, .52f, .88f, .66f);
        var subtitle = Text("Subtitle", canvas.transform, "THE ETHICAL HACKER", 24, Cyan, FontStyle.Bold);
        subtitle.alignment = TextAnchor.MiddleCenter;
        Place(subtitle.rectTransform, .12f, .46f, .88f, .52f);
        var prompt = Text("Prompt", canvas.transform, "PRESS ANY KEY TO CONTINUE", 19, White, FontStyle.Bold);
        prompt.alignment = TextAnchor.MiddleCenter;
        Place(prompt.rectTransform, .2f, .14f, .8f, .20f);
        canvas.gameObject.AddComponent<SplashController>();
        Save(scene, "Assets/_CyberRakshak/Scenes/Splash.unity");
    }

    private static void BuildMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddSceneCameraAndLight();
        var canvas = CanvasRoot("MainMenuUI");
        Background(canvas.transform);

        var title = Text("Title", canvas.transform, "CYBERRAKSHAK", 72, White, FontStyle.Bold);
        Place(title.rectTransform, .07f, .74f, .52f, .84f);
        var subtitle = Text("Subtitle", canvas.transform, "THE ETHICAL HACKER", 20, Cyan, FontStyle.Bold);
        Place(subtitle.rectTransform, .075f, .70f, .52f, .74f);
        var line = Image("TitleLine", canvas.transform, Cyan);
        Place(line.rectTransform, .075f, .685f, .32f, .690f);

        var navigator = canvas.gameObject.AddComponent<SceneNavigator>();
        var start = MenuButton("StartTrainingButton", canvas.transform, "START TRAINING", Coral, true);
        Place(start.GetComponent<RectTransform>(), .10f, .52f, .37f, .59f);
        start.onClick.AddListener(navigator.StartTraining);
        var cont = MenuButton("ContinueButton", canvas.transform, "CONTINUE", Muted, false);
        Place(cont.GetComponent<RectTransform>(), .10f, .44f, .37f, .51f);
        cont.onClick.AddListener(navigator.ContinueTraining);
        var settings = MenuButton("SettingsButton", canvas.transform, "SETTINGS", Muted, false);
        Place(settings.GetComponent<RectTransform>(), .10f, .36f, .37f, .43f);
        var exit = MenuButton("ExitButton", canvas.transform, "EXIT", Muted, false);
        Place(exit.GetComponent<RectTransform>(), .10f, .28f, .37f, .35f);
        exit.onClick.AddListener(navigator.QuitGame);

        var settingsPanel = OverlaySettings(canvas.transform, "SettingsPanel");
        var menuController = canvas.gameObject.AddComponent<MainMenuController>();
        Set(menuController, "continueButton", cont);
        Set(menuController, "settingsPanel", settingsPanel);
        settings.onClick.AddListener(menuController.OpenSettings);

        var strap = Text("Tagline", canvas.transform, "LEARN.  PROTECT.  EMPOWER.", 17, Cyan, FontStyle.Bold);
        strap.alignment = TextAnchor.MiddleRight;
        Place(strap.rectTransform, .56f, .08f, .92f, .13f);
        Save(scene, "Assets/_CyberRakshak/Scenes/MainMenu.unity");
    }

    private static void BuildLevelSelect()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AddSceneCameraAndLight();
        var canvas = CanvasRoot("LevelSelectUI");
        Background(canvas.transform);
        var title = Text("Title", canvas.transform, "SELECT TRAINING MODULE", 50, White, FontStyle.Bold);
        Place(title.rectTransform, .08f, .80f, .7f, .88f);
        var caption = Text("Caption", canvas.transform, "Build your ethical-hacking instincts one mission at a time.", 21, Muted, FontStyle.Normal);
        Place(caption.rectTransform, .08f, .75f, .7f, .80f);

        var navigator = canvas.gameObject.AddComponent<SceneNavigator>();
        var tutorial = LevelCard("TutorialCard", canvas.transform, "TUTORIAL", "Meet Patch and learn the controls.", Cyan, true);
        Place(tutorial.GetComponent<RectTransform>(), .08f, .47f, .42f, .68f);
        tutorial.onClick.AddListener(navigator.LoadTutorial);
        var levelOne = LevelCard("LevelOneCard", canvas.transform, "LEVEL 1  //  FIREWALL FOUNDATIONS", "Complete the tutorial to unlock.", Muted, false);
        Place(levelOne.GetComponent<RectTransform>(), .46f, .47f, .80f, .68f);
        levelOne.onClick.AddListener(navigator.LoadLevelOne);
        var back = MenuButton("BackButton", canvas.transform, "BACK", Muted, false);
        Place(back.GetComponent<RectTransform>(), .08f, .18f, .25f, .25f);
        back.onClick.AddListener(navigator.ReturnToMainMenu);

        var controller = canvas.gameObject.AddComponent<LevelSelectController>();
        Set(controller, "levelOneButton", levelOne);
        Save(scene, "Assets/_CyberRakshak/Scenes/LevelSelect.unity");
    }

    private static void BuildTutorialScene()
    {
        const string source = "Assets/_CyberRakshak/Scenes/PrototypeArena.unity";
        const string destination = "Assets/_CyberRakshak/Scenes/Game_Tutorial.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(destination) != null)
            AssetDatabase.DeleteAsset(destination);
        AssetDatabase.CopyAsset(source, destination);
        var scene = EditorSceneManager.OpenScene(destination, OpenSceneMode.Single);
        var oldMenu = GameObject.Find("CyberRakshakMenuUI");
        if (oldMenu != null) Object.DestroyImmediate(oldMenu);
        var oldPreview = GameObject.Find("MenuPatchPreviewWorld");
        if (oldPreview != null) Object.DestroyImmediate(oldPreview);
        var oldCamera = GameObject.Find("MenuPatchPreviewCamera");
        if (oldCamera != null) Object.DestroyImmediate(oldCamera);

        var canvas = CanvasRoot("GameplayUI");
        var pause = OverlayPause(canvas.transform);
        var settings = OverlaySettings(canvas.transform, "GameplaySettingsPanel");
        pause.SetActive(false);
        settings.SetActive(false);
        var controller = canvas.gameObject.AddComponent<GameplayPauseController>();
        Set(controller, "pausePanel", pause);
        Set(controller, "settingsPanel", settings);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Canvas CanvasRoot(string name)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(go.transform, false);
        return canvas;
    }

    private static void AddSceneCameraAndLight()
    {
        new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).tag = "MainCamera";
        var light = new GameObject("Directional Light", typeof(Light));
        light.GetComponent<Light>().type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void Background(Transform parent)
    {
        var image = Image("Background", parent, Color.white);
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        image.preserveAspect = false;
        Stretch(image.rectTransform);
    }

    private static GameObject OverlayPause(Transform parent)
    {
        var overlay = Image("PausePanel", parent, new Color(0f, .01f, .04f, .83f));
        Stretch(overlay.rectTransform);
        var card = Image("PauseCard", overlay.transform, Card);
        Place(card.rectTransform, .35f, .26f, .65f, .74f);
        var title = Text("Title", card.transform, "PAUSED", 48, White, FontStyle.Bold);
        title.alignment = TextAnchor.MiddleCenter;
        Place(title.rectTransform, .1f, .70f, .9f, .86f);
        return overlay.gameObject;
    }

    private static GameObject OverlaySettings(Transform parent, string name)
    {
        var overlay = Image(name, parent, new Color(0f, .01f, .04f, .88f));
        Stretch(overlay.rectTransform);
        var card = Image("Card", overlay.transform, Card);
        Place(card.rectTransform, .34f, .22f, .66f, .78f);
        var title = Text("Title", card.transform, "SETTINGS", 44, White, FontStyle.Bold);
        title.alignment = TextAnchor.MiddleCenter;
        Place(title.rectTransform, .1f, .72f, .9f, .86f);
        var close = MenuButton("CloseButton", card.transform, "BACK", Cyan, true);
        Place(close.GetComponent<RectTransform>(), .28f, .12f, .72f, .23f);
        overlay.gameObject.SetActive(false);
        return overlay.gameObject;
    }

    private static Image Image(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text Text(string name, Transform parent, string value, int size, Color color, FontStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Button MenuButton(string name, Transform parent, string label, Color color, bool primary)
    {
        var image = Image(name, parent, primary ? new Color(color.r, color.g, color.b, .16f) : new Color(1f, 1f, 1f, .035f));
        var button = image.gameObject.AddComponent<Button>();
        var text = Text("Label", image.transform, label, 28, primary ? White : color, FontStyle.Bold);
        Place(text.rectTransform, .07f, 0f, .96f, 1f);
        return button;
    }

    private static Button LevelCard(string name, Transform parent, string label, string detail, Color color, bool available)
    {
        var image = Image(name, parent, new Color(Card.r, Card.g, Card.b, available ? .96f : .65f));
        var button = image.gameObject.AddComponent<Button>();
        button.interactable = available;
        var heading = Text("Heading", image.transform, label, 24, available ? White : Muted, FontStyle.Bold);
        Place(heading.rectTransform, .08f, .55f, .92f, .85f);
        var body = Text("Detail", image.transform, detail, 17, available ? Cyan : Muted, FontStyle.Normal);
        Place(body.rectTransform, .08f, .20f, .92f, .52f);
        return button;
    }

    private static void Place(RectTransform rect, float xMin, float yMin, float xMax, float yMax)
    {
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Stretch(RectTransform rect) => Place(rect, 0f, 0f, 1f, 1f);

    private static void Set(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }

    private static void Save(Scene scene, string path)
    {
        EditorSceneManager.SaveScene(scene, path);
    }
}
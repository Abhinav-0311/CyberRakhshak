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
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/Game_Tutorial.unity", true),
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/Game_Level01.unity", true),
            new EditorBuildSettingsScene("Assets/_CyberRakshak/Scenes/Game_Level02.unity", true)
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
        scene.name = "LevelSelect";
        RebuildLevelSelectUI();
        Save(scene, "Assets/_CyberRakshak/Scenes/LevelSelect.unity");
    }

    [MenuItem("CyberRakshak/Rebuild Level Select UI")]
    public static void RebuildLevelSelectUI()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.name != "LevelSelect")
            throw new System.InvalidOperationException("Open LevelSelect in Edit Mode first.");
        var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        if (background == null)
            throw new System.InvalidOperationException("The clean PATCH background is missing.");

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Rebuild Level Select UI");
        var previous = GameObject.Find("LevelSelectUI");
        var canvas = CanvasRoot("LevelSelectUI_New");
        canvas.gameObject.SetActive(false);
        Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Create native module directory");
        Background(canvas.transform);
        var content = Image("ModuleDirectory", canvas.transform, Color.clear);
        Stretch(content.rectTransform);
        var brand = Text("Brand", content.transform, "CYBERRAKSHAK  /  TRAINING", 22, Cyan, FontStyle.Bold);
        Place(brand.rectTransform, .065f, .88f, .6f, .94f);
        var title = Text("Title", content.transform, "Choose your mission.", 62, White, FontStyle.Bold);
        Place(title.rectTransform, .065f, .78f, .62f, .87f);
        var caption = Text("Caption", content.transform, "Learn the controls. Defend the system. Spot the deception.", 24, Muted, FontStyle.Normal);
        Place(caption.rectTransform, .067f, .735f, .65f, .79f);

        const string prefabPath = "Assets/_CyberRakshak/Prefabs/UI/TrainingModuleCard.prefab";
        if (!AssetDatabase.IsValidFolder("Assets/_CyberRakshak/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/_CyberRakshak/Prefabs", "UI");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            var template = CreateModuleCard();
            prefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, prefabPath);
            Object.DestroyImmediate(template.gameObject);
        }
        var tutorial = ModuleCard(prefab, content.transform, "Tutorial", "00", "ONBOARDING", "Tutorial",
            "Meet PATCH. Learn to move, jump and interact.", .565f, UiAction.LoadTutorial);
        var levelOne = ModuleCard(prefab, content.transform, "LevelOne", "01", "FIREWALL FOUNDATIONS", "Firewall Foundations",
            "Clear the patrol. Extinguish the fire. Reach the exit.", .385f, UiAction.LoadLevelOne);
        var levelTwo = ModuleCard(prefab, content.transform, "LevelTwo", "02", "MAZE PROTOTYPE", "Level 2 Maze",
            "Find the exit. Phishing challenges are not implemented yet.", .205f, UiAction.LoadLevelTwo);
        var back = MenuButton("BackHit", content.transform, "<  BACK TO MENU", Cyan, true);
        Place(back.GetComponent<RectTransform>(), .065f, .075f, .26f, .135f);
        back.gameObject.AddComponent<UiActionButton>().action = UiAction.ReturnMainMenu;
        back.transition = Selectable.Transition.ColorTint;
        var backColors = back.colors;
        backColors.highlightedColor = backColors.selectedColor = new Color(.55f, .9f, 1f);
        back.colors = backColors;
        var hint = Text("NavigationHint", content.transform, "ARROWS  /  SELECT     ENTER  /  PLAY", 19, Muted, FontStyle.Normal);
        hint.alignment = TextAnchor.MiddleRight;
        Place(hint.rectTransform, .31f, .08f, .60f, .13f);
        var patchTag = Text("PatchLabel", content.transform, "PATCH  /  TRAINING ASSISTANT", 20, Cyan, FontStyle.Bold);
        patchTag.alignment = TextAnchor.MiddleCenter;
        Place(patchTag.rectTransform, .66f, .16f, .95f, .22f);
        var patchMessage = Text("PatchMessage", content.transform, "Start with the tutorial.\nI'll be with you every step of the way.", 24, White, FontStyle.Normal);
        patchMessage.alignment = TextAnchor.MiddleCenter;
        Place(patchMessage.rectTransform, .64f, .075f, .97f, .16f);

        var navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = back, selectOnDown = levelOne };
        tutorial.navigation = navigation;
        navigation.selectOnUp = tutorial; navigation.selectOnDown = levelTwo;
        levelOne.navigation = navigation;
        navigation.selectOnUp = levelOne; navigation.selectOnDown = back;
        levelTwo.navigation = navigation;
        navigation.selectOnUp = levelTwo; navigation.selectOnDown = tutorial;
        back.navigation = navigation;
        var eventSystem = canvas.GetComponentInChildren<EventSystem>();
        eventSystem.firstSelectedGameObject = tutorial.gameObject;
        canvas.gameObject.AddComponent<SceneNavigator>();
        var controller = canvas.gameObject.AddComponent<LevelSelectController>();
        Set(controller, "tutorialButton", tutorial);
        Set(controller, "tutorialStatus", tutorial.transform.Find("TutorialStatus").GetComponent<Text>());
        Set(controller, "levelOneButton", levelOne);
        Set(controller, "levelOneStatus", levelOne.transform.Find("LevelOneStatus").GetComponent<Text>());
        Set(controller, "levelTwoButton", levelTwo);
        Set(controller, "levelTwoStatus", levelTwo.transform.Find("LevelTwoStatus").GetComponent<Text>());
        foreach (var graphic in canvas.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = graphic.GetComponent<Button>() != null;
        if (previous != null) Undo.DestroyObjectImmediate(previous);
        canvas.name = "LevelSelectUI";
        canvas.gameObject.SetActive(true);
        controller.Refresh();
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static Button CreateModuleCard()
    {
        var image = Image("TrainingModuleCard", null, Color.white);
        image.rectTransform.sizeDelta = new Vector2(1030, 166);
        var border = image.gameObject.AddComponent<Outline>();
        border.effectColor = new Color(.18f, .65f, .85f, .4f);
        border.effectDistance = new Vector2(1.5f, -1.5f);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = new Color(.035f, .08f, .14f, .96f);
        colors.highlightedColor = colors.selectedColor = new Color(.07f, .20f, .28f, 1f);
        colors.pressedColor = new Color(.015f, .055f, .09f, 1f);
        colors.disabledColor = new Color(.025f, .045f, .075f, .95f);
        colors.fadeDuration = .12f;
        button.colors = colors;
        var rail = Image("AccentRail", image.transform, Cyan);
        Place(rail.rectTransform, 0f, .12f, .004f, .88f);
        var number = Text("Number", image.transform, "00", 46, Cyan, FontStyle.Bold);
        number.alignment = TextAnchor.MiddleCenter;
        Place(number.rectTransform, .012f, .14f, .12f, .88f);
        var eyebrow = Text("Eyebrow", image.transform, "MODULE", 17, Cyan, FontStyle.Bold);
        Place(eyebrow.rectTransform, .14f, .69f, .77f, .90f);
        var heading = Text("Heading", image.transform, "Training Module", 38, White, FontStyle.Bold);
        Place(heading.rectTransform, .14f, .36f, .78f, .70f);
        var detail = Text("Detail", image.transform, "Mission description", 22, Muted, FontStyle.Normal);
        Place(detail.rectTransform, .14f, .12f, .79f, .36f);
        var status = Text("Status", image.transform, "PLAY", 23, Cyan, FontStyle.Bold);
        status.alignment = TextAnchor.MiddleCenter;
        Place(status.rectTransform, .81f, .35f, .975f, .65f);
        foreach (var graphic in image.GetComponentsInChildren<Graphic>())
            graphic.raycastTarget = graphic == image;
        return button;
    }

    [MenuItem("CyberRakshak/Build PATCH Dialogue UI")]
    public static void BuildPatchDialogueUI()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Leave Play Mode before authoring dialogue UI.");
        const string folder = "Assets/_CyberRakshak/Resources/UI";
        const string path = folder + "/PatchDialoguePanel.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            throw new System.InvalidOperationException("Dialogue prefab exists; edit it directly instead of rebuilding it.");
        if (AssetDatabase.LoadAssetAtPath<Sprite>(folder + "/PatchPortrait.png") == null)
            throw new System.InvalidOperationException("Import the PATCH portrait as a sprite first.");
        var root = new GameObject("PATCH_DialogueUI", typeof(RectTransform));
        try
        {
            var presenter = root.AddComponent<CyberRakshak.PATCH.PatchDialoguePresenter>();
            Set(presenter, "bubbleSprite", AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"));
            presenter.GetType().GetMethod("BuildDefaultPanel", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(presenter, null);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Button ModuleCard(GameObject prefab, Transform parent, string key, string number, string eyebrow,
        string heading, string detail, float bottom, UiAction? action)
    {
        var card = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        card.name = key + "Hit";
        Place(card.GetComponent<RectTransform>(), .065f, bottom, .60f, bottom + .155f);
        card.transform.Find("Number").GetComponent<Text>().text = number;
        card.transform.Find("Eyebrow").GetComponent<Text>().text = eyebrow;
        card.transform.Find("Heading").GetComponent<Text>().text = heading;
        card.transform.Find("Heading").name = key + "Title";
        card.transform.Find("Detail").GetComponent<Text>().text = detail;
        card.transform.Find("Status").name = key + "Status";
        var button = card.GetComponent<Button>();
        if (action.HasValue) card.AddComponent<UiActionButton>().action = action.Value;
        else
        {
            button.interactable = false;
            foreach (var text in card.GetComponentsInChildren<Text>()) text.color = Muted;
            card.transform.Find("AccentRail").GetComponent<Image>().color = Muted;
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(card.transform);
        foreach (var component in card.GetComponentsInChildren<Component>())
            if (component != null) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        return button;
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

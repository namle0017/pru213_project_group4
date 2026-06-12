using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LuckySpinSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SpinScene.unity";
    private const string MenuItemPath = "Tools/HillClimb/Build Lucky Spin Scene";

    private const string WheelSpritePath = "Assets/Sprites/component/Spin/SpinWheel_Rotating.png";
    private const string PointerSpritePath = "Assets/Sprites/component/Spin/SpinPointer_Static.png";
    private const string SpinButtonSpritePath = "Assets/Sprites/component/Spin/SpinIconButton_Circular.png";
    private const string RewardPopupSpritePath = "Assets/Sprites/component/Spin/reward_popup.png";
    private const string OkButtonSpritePath = "Assets/Sprites/component/Spin/ok_button.png";
    private const string SpinAgainSpritePath = "Assets/Sprites/component/Spin/spin_again_bottom.png";
    private const string RewardImageSpritePath = "Assets/Sprites/component/Spin/f1_reward.png";
    private const string CoinHudSpritePath = "Assets/Sprites/component/Everything/coin_hud.png";
    private const string FontAssetPath = "Assets/Fredoka-VariableFont_wdth,wght SDF.asset";

    [MenuItem(MenuItemPath)]
    public static void BuildLuckySpinScene()
    {
        EnsureSceneFolderExists();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SpinScene";

        Camera camera = CreateCamera();
        CreateEventSystem();
        Canvas canvas = CreateCanvas();

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        Sprite wheelSprite = LoadSprite(WheelSpritePath);
        Sprite pointerSprite = LoadSprite(PointerSpritePath);
        Sprite spinButtonSprite = LoadSprite(SpinButtonSpritePath);
        Sprite rewardPopupSprite = LoadSprite(RewardPopupSpritePath);
        Sprite okButtonSprite = LoadSprite(OkButtonSpritePath);
        Sprite spinAgainSprite = LoadSprite(SpinAgainSpritePath);
        Sprite rewardImageSprite = LoadSprite(RewardImageSpritePath);
        Sprite coinHudSprite = LoadSprite(CoinHudSpritePath);

        CreateBackground(canvas.transform);
        Button homeButton = CreateHomeButton(canvas.transform, fontAsset);
        CreateTitle(canvas.transform, fontAsset);
        TextMeshProUGUI coinText = CreateCoinHud(canvas.transform, coinHudSprite, fontAsset);
        Image wheelImage = CreateWheel(canvas.transform, wheelSprite);
        CreatePointer(canvas.transform, pointerSprite);
        Button spinButton = CreateSpinButton(canvas.transform, spinButtonSprite);

        ResultPopupRefs popupRefs = CreateResultPopup(
            canvas.transform,
            rewardPopupSprite,
            okButtonSprite,
            spinAgainSprite,
            rewardImageSprite,
            fontAsset);

        LuckySpinController controller = CreateController(
            canvas.transform,
            coinText,
            popupRefs,
            homeButton,
            spinButton,
            wheelImage.rectTransform);

        Selection.activeObject = controller.gameObject;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("LuckySpinSceneBuilder: SpinScene has been created and added to Build Settings.");
    }

    private static void EnsureSceneFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.98f, 0.97f, 0.94f, 1f);
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform rect = canvasObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return canvas;
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject background = CreateUiObject("Background", parent);
        Image image = background.AddComponent<Image>();
        image.color = new Color(1f, 0.988f, 0.95f, 1f);

        RectTransform rect = background.GetComponent<RectTransform>();
        StretchFullScreen(rect);
    }

    private static Button CreateHomeButton(Transform parent, TMP_FontAsset fontAsset)
    {
        GameObject buttonObject = CreateUiObject("HomeButton", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.19f, 0.51f, 0.93f, 1f);
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(180f, 76f);
        rect.anchoredPosition = new Vector2(40f, -36f);

        TextMeshProUGUI text = CreateTmpText("HomeButtonText", buttonObject.transform, fontAsset, "HOME", 34f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineColor = Color.black;
        text.outlineWidth = 0.18f;
        StretchWithPadding(text.rectTransform, 10f, 6f);

        return button;
    }

    private static void CreateTitle(Transform parent, TMP_FontAsset fontAsset)
    {
        TextMeshProUGUI title = CreateTmpText("TitleText", parent, fontAsset, "LUCKY SPIN", 72f, FontStyles.Bold);
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.17f, 0.2f, 0.29f, 1f);
        title.outlineColor = Color.white;
        title.outlineWidth = 0.15f;

        RectTransform rect = title.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(700f, 120f);
        rect.anchoredPosition = new Vector2(0f, -34f);
    }

    private static TextMeshProUGUI CreateCoinHud(Transform parent, Sprite frameSprite, TMP_FontAsset fontAsset)
    {
        GameObject root = CreateUiObject("CoinHud", parent);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(330f, 108f);
        rect.anchoredPosition = new Vector2(-38f, -26f);

        GameObject frameObject = CreateUiObject("CoinFrameImage", root.transform);
        Image frame = frameObject.AddComponent<Image>();
        frame.sprite = frameSprite;
        frame.preserveAspect = true;
        StretchFullScreen(frameObject.GetComponent<RectTransform>());

        TextMeshProUGUI coinText = CreateTmpText("CoinText", root.transform, fontAsset, "0", 44f, FontStyles.Bold);
        coinText.alignment = TextAlignmentOptions.Center;
        coinText.color = Color.white;
        coinText.outlineColor = Color.black;
        coinText.outlineWidth = 0.2f;

        RectTransform coinRect = coinText.rectTransform;
        coinRect.anchorMin = new Vector2(0.42f, 0.18f);
        coinRect.anchorMax = new Vector2(0.88f, 0.82f);
        coinRect.offsetMin = Vector2.zero;
        coinRect.offsetMax = Vector2.zero;

        return coinText;
    }

    private static Image CreateWheel(Transform parent, Sprite wheelSprite)
    {
        GameObject wheelObject = CreateUiObject("SpinWheel_Rotating", parent);
        Image wheelImage = wheelObject.AddComponent<Image>();
        wheelImage.sprite = wheelSprite;
        wheelImage.preserveAspect = true;

        RectTransform rect = wheelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(620f, 620f);
        rect.anchoredPosition = new Vector2(0f, -20f);

        return wheelImage;
    }

    private static void CreatePointer(Transform parent, Sprite pointerSprite)
    {
        GameObject pointerObject = CreateUiObject("SpinPointer_Static", parent);
        Image image = pointerObject.AddComponent<Image>();
        image.sprite = pointerSprite;
        image.preserveAspect = true;

        RectTransform rect = pointerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(120f, 180f);
        rect.anchoredPosition = new Vector2(0f, 340f);
    }

    private static Button CreateSpinButton(Transform parent, Sprite buttonSprite)
    {
        GameObject buttonObject = CreateUiObject("SpinButton", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.preserveAspect = true;
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(150f, 150f);
        rect.anchoredPosition = new Vector2(0f, -390f);

        return button;
    }

    private static ResultPopupRefs CreateResultPopup(
        Transform parent,
        Sprite popupSprite,
        Sprite okSprite,
        Sprite spinAgainSprite,
        Sprite rewardSprite,
        TMP_FontAsset fontAsset)
    {
        ResultPopupRefs refs = new ResultPopupRefs();

        GameObject root = CreateUiObject("ResultPopupRoot", parent);
        refs.root = root;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchFullScreen(rootRect);

        GameObject overlay = CreateUiObject("PopupOverlay", root.transform);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.45f);
        StretchFullScreen(overlay.GetComponent<RectTransform>());

        GameObject panel = CreateUiObject("PopupPanel", root.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = popupSprite;
        panelImage.preserveAspect = true;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(860f, 700f);
        panelRect.anchoredPosition = new Vector2(0f, 0f);

        refs.resultTitleText = CreateTmpText("ResultTitleText", panel.transform, fontAsset, "SPIN RESULT", 54f, FontStyles.Bold);
        refs.resultTitleText.alignment = TextAlignmentOptions.Center;
        refs.resultTitleText.color = new Color(0.19f, 0.15f, 0.1f, 1f);
        refs.resultTitleText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        refs.resultTitleText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        refs.resultTitleText.rectTransform.pivot = new Vector2(0.5f, 1f);
        refs.resultTitleText.rectTransform.sizeDelta = new Vector2(500f, 70f);
        refs.resultTitleText.rectTransform.anchoredPosition = new Vector2(0f, -54f);

        GameObject rewardImageObject = CreateUiObject("RewardImage", panel.transform);
        refs.rewardImage = rewardImageObject.AddComponent<Image>();
        refs.rewardImage.sprite = rewardSprite;
        refs.rewardImage.preserveAspect = true;

        RectTransform rewardRect = rewardImageObject.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardRect.pivot = new Vector2(0.5f, 0.5f);
        rewardRect.sizeDelta = new Vector2(240f, 180f);
        rewardRect.anchoredPosition = new Vector2(0f, 40f);

        refs.resultMessageText = CreateTmpText("ResultMessageText", panel.transform, fontAsset, "Reward preview", 34f, FontStyles.Bold);
        refs.resultMessageText.alignment = TextAlignmentOptions.Center;
        refs.resultMessageText.color = new Color(0.22f, 0.18f, 0.13f, 1f);
        refs.resultMessageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        refs.resultMessageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        refs.resultMessageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        refs.resultMessageText.rectTransform.sizeDelta = new Vector2(620f, 120f);
        refs.resultMessageText.rectTransform.anchoredPosition = new Vector2(0f, -110f);

        refs.okButton = CreatePopupButton("OkButton", panel.transform, okSprite, new Vector2(-150f, -255f), new Vector2(250f, 96f), "OK", fontAsset);
        refs.spinAgainButton = CreatePopupButton("SpinAgainButton", panel.transform, spinAgainSprite, new Vector2(150f, -255f), new Vector2(320f, 96f), "SPIN AGAIN", fontAsset);

        root.SetActive(false);
        return refs;
    }

    private static Button CreatePopupButton(string name, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size, string label, TMP_FontAsset fontAsset)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = CreateTmpText(name + "Text", buttonObject.transform, fontAsset, label, 34f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineColor = Color.black;
        text.outlineWidth = 0.18f;
        StretchWithPadding(text.rectTransform, 16f, 10f);

        return button;
    }

    private static LuckySpinController CreateController(
        Transform parent,
        TextMeshProUGUI coinText,
        ResultPopupRefs popupRefs,
        Button homeButton,
        Button spinButton,
        RectTransform wheelTransform)
    {
        GameObject controllerObject = new GameObject("GameManager");
        controllerObject.transform.SetParent(parent, false);
        LuckySpinController controller = controllerObject.AddComponent<LuckySpinController>();

        SerializedObject serializedObject = new SerializedObject(controller);
        serializedObject.FindProperty("wheelTransform").objectReferenceValue = wheelTransform;
        serializedObject.FindProperty("spinButton").objectReferenceValue = spinButton;
        serializedObject.FindProperty("resultPopupRoot").objectReferenceValue = popupRefs.root;
        serializedObject.FindProperty("resultRewardImage").objectReferenceValue = popupRefs.rewardImage;
        serializedObject.FindProperty("resultTitleText").objectReferenceValue = popupRefs.resultTitleText;
        serializedObject.FindProperty("resultMessageText").objectReferenceValue = popupRefs.resultMessageText;
        serializedObject.FindProperty("okButton").objectReferenceValue = popupRefs.okButton;
        serializedObject.FindProperty("spinAgainButton").objectReferenceValue = popupRefs.spinAgainButton;
        serializedObject.FindProperty("coinText").objectReferenceValue = coinText;
        serializedObject.FindProperty("homeButton").objectReferenceValue = homeButton;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        return controller;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == scenePath)
            {
                scene.enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static TextMeshProUGUI CreateTmpText(string name, Transform parent, TMP_FontAsset fontAsset, string textValue, float fontSize, FontStyles style)
    {
        GameObject textObject = CreateUiObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.enableWordWrapping = false;
        text.font = fontAsset;
        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void StretchWithPadding(RectTransform rect, float horizontal, float vertical)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(horizontal, vertical);
        rect.offsetMax = new Vector2(-horizontal, -vertical);
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning("LuckySpinSceneBuilder: Missing sprite at " + path);
        }

        return sprite;
    }

    private sealed class ResultPopupRefs
    {
        public GameObject root;
        public TextMeshProUGUI resultTitleText;
        public Image rewardImage;
        public TextMeshProUGUI resultMessageText;
        public Button okButton;
        public Button spinAgainButton;
    }
}

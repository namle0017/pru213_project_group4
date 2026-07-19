using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DailyRewardSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DailyRewardScene.unity";
    private const string MenuItemPath = "Tools/HillClimb/Build Daily Reward Scene";

    private const string BackgroundSpritePath = "Assets/Sprites/component/daily_gift/background.png";
    private const string ClaimBadgeSpritePath = "Assets/Sprites/component/daily_gift/claim_badge.png";
    private const string ClaimButtonSpritePath = "Assets/Sprites/component/daily_gift/daily_claim_button.png";
    private const string ClaimedButtonSpritePath = "Assets/Sprites/component/daily_gift/claimed_button.png";
    private const string PopupSpritePath = "Assets/Sprites/component/daily_gift/popup_reward.png";
    private const string CoinIconSpritePath = "Assets/Sprites/component/Everything/Icon_start.png";
    private const string MotorIconSpritePath = "Assets/Sprites/component/daily_gift/motor_reward.png";
    private const string FontAssetPath = "Assets/Fredoka-VariableFont_wdth,wght SDF.asset";

    [MenuItem(MenuItemPath)]
    public static void BuildDailyRewardScene()
    {
        EnsureSceneFolderExists();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "DailyRewardScene";

        CreateCamera();
        CreateEventSystem();
        Canvas canvas = CreateCanvas();

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        Sprite backgroundSprite = LoadSprite(BackgroundSpritePath);
        Sprite claimBadgeSprite = LoadSprite(ClaimBadgeSpritePath);
        Sprite claimButtonSprite = LoadSprite(ClaimButtonSpritePath);
        Sprite claimedButtonSprite = LoadSprite(ClaimedButtonSpritePath);
        Sprite popupSprite = LoadSprite(PopupSpritePath);
        Sprite coinIconSprite = LoadSprite(CoinIconSpritePath);
        Sprite motorIconSprite = LoadSprite(MotorIconSpritePath);

        CreateBackground(canvas.transform, backgroundSprite);
        CreateTitle(canvas.transform, fontAsset);
        Button homeButton = CreateHomeButton(canvas.transform, fontAsset);

        CreateDaySlots(canvas.transform, fontAsset, coinIconSprite, motorIconSprite, claimBadgeSprite);
        Button claimButton = CreateImageButton("ClaimButton", canvas.transform, claimButtonSprite, new Vector2(0f, -456f), new Vector2(650f, 118f), true);
        Button claimedButton = CreateImageButton("ClaimedButton", canvas.transform, claimedButtonSprite, new Vector2(0f, -456f), new Vector2(650f, 118f), true);
        claimedButton.interactable = false;
        claimedButton.gameObject.SetActive(false);

        PopupRefs popupRefs = CreateRewardPopup(canvas.transform, popupSprite, coinIconSprite, fontAsset);
        DailyRewardController controller = CreateController(canvas.transform, homeButton, claimButton, claimedButton, popupRefs, coinIconSprite, motorIconSprite);

        Selection.activeObject = controller.gameObject;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("DailyRewardSceneBuilder: DailyRewardScene has been created and added to Build Settings.");
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
        camera.backgroundColor = new Color(0.08f, 0.18f, 0.34f, 1f);
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
        StretchFullScreen(rect);
        return canvas;
    }

    private static void CreateBackground(Transform parent, Sprite backgroundSprite)
    {
        GameObject background = CreateUiObject("BackgroundImage", parent);
        Image image = background.AddComponent<Image>();
        image.sprite = backgroundSprite;
        image.preserveAspect = false;
        image.raycastTarget = false;
        StretchFullScreen(background.GetComponent<RectTransform>());
    }

    private static void CreateTitle(Transform parent, TMP_FontAsset fontAsset)
    {
        TextMeshProUGUI title = CreateTmpText("TitleText", parent, fontAsset, "DAILY REWARD", 70f, FontStyles.Bold);
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        title.outlineColor = new Color(0.06f, 0.12f, 0.22f, 1f);
        title.outlineWidth = 0.06f;

        RectTransform rect = title.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(780f, 110f);
        rect.anchoredPosition = new Vector2(0f, -78f);
    }

    private static Button CreateHomeButton(Transform parent, TMP_FontAsset fontAsset)
    {
        GameObject buttonObject = CreateUiObject("HomeButton", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.1f, 0.45f, 0.95f, 0.92f);
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(178f, 76f);
        rect.anchoredPosition = new Vector2(38f, -34f);

        TextMeshProUGUI text = CreateTmpText("HomeButtonText", buttonObject.transform, fontAsset, "HOME", 32f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineColor = Color.black;
        text.outlineWidth = 0.04f;
        StretchWithPadding(text.rectTransform, 8f, 4f);
        return button;
    }

    private static DaySlotRefs[] CreateDaySlots(
        Transform parent,
        TMP_FontAsset fontAsset,
        Sprite coinSprite,
        Sprite motorSprite,
        Sprite claimBadgeSprite)
    {
        DaySlotRefs[] slots = new DaySlotRefs[7];
        Vector2[] positions =
        {
            new Vector2(-420f, 184f),
            new Vector2(0f, 184f),
            new Vector2(420f, 184f),
            new Vector2(-420f, -62f),
            new Vector2(0f, -62f),
            new Vector2(420f, -62f),
            new Vector2(0f, -326f)
        };

        string[] rewards = { "50", "100", "150", "200", "250", "300", "MOTOR" };

        for (int i = 0; i < slots.Length; i++)
        {
            bool isMotorReward = i == 6;
            Vector2 size = isMotorReward ? new Vector2(520f, 205f) : new Vector2(245f, 170f);
            slots[i] = CreateDaySlot(
                parent,
                "DaySlot" + (i + 1),
                "DAY " + (i + 1),
                isMotorReward ? motorSprite : coinSprite,
                rewards[i],
                claimBadgeSprite,
                positions[i],
                size,
                fontAsset,
                isMotorReward);
        }

        return slots;
    }

    private static DaySlotRefs CreateDaySlot(
        Transform parent,
        string objectName,
        string dayLabel,
        Sprite rewardSprite,
        string rewardAmount,
        Sprite claimBadgeSprite,
        Vector2 anchoredPosition,
        Vector2 size,
        TMP_FontAsset fontAsset,
        bool isLargeSlot)
    {
        GameObject root = CreateUiObject(objectName, parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = size;
        rootRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI label = CreateTmpText("DayLabel", root.transform, fontAsset, dayLabel, isLargeSlot ? 36f : 28f, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.outlineColor = Color.black;
        label.outlineWidth = 0.04f;
        AnchorRect(label.rectTransform, new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.98f));

        GameObject rewardIconObject = CreateUiObject("RewardIcon", root.transform);
        Image rewardIcon = rewardIconObject.AddComponent<Image>();
        rewardIcon.sprite = rewardSprite;
        rewardIcon.preserveAspect = true;
        rewardIcon.raycastTarget = false;
        RectTransform iconRect = rewardIconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = isLargeSlot ? new Vector2(180f, 118f) : new Vector2(92f, 76f);
        iconRect.anchoredPosition = isLargeSlot ? new Vector2(-95f, -6f) : new Vector2(0f, 2f);

        TextMeshProUGUI amountText = CreateTmpText("RewardAmountText", root.transform, fontAsset, rewardAmount, isLargeSlot ? 40f : 34f, FontStyles.Bold);
        amountText.alignment = TextAlignmentOptions.Center;
        amountText.color = new Color(1f, 0.93f, 0.45f, 1f);
        amountText.outlineColor = Color.black;
        amountText.outlineWidth = 0.04f;
        AnchorRect(amountText.rectTransform, new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.28f));
        if (isLargeSlot)
        {
            amountText.rectTransform.anchoredPosition = new Vector2(105f, -58f);
        }

        GameObject badgeObject = CreateUiObject("ClaimBadge", root.transform);
        Image badge = badgeObject.AddComponent<Image>();
        badge.sprite = claimBadgeSprite;
        badge.preserveAspect = true;
        badge.raycastTarget = false;
        RectTransform badgeRect = badgeObject.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.sizeDelta = isLargeSlot ? new Vector2(250f, 140f) : new Vector2(155f, 92f);
        badgeRect.anchoredPosition = Vector2.zero;
        badgeObject.SetActive(false);

        return new DaySlotRefs
        {
            root = root,
            dayLabel = label,
            rewardIcon = rewardIcon,
            rewardAmountText = amountText,
            claimBadge = badge
        };
    }

    private static Button CreateImageButton(string objectName, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size, bool preserveAspect)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        return button;
    }

    private static PopupRefs CreateRewardPopup(Transform parent, Sprite popupSprite, Sprite rewardSprite, TMP_FontAsset fontAsset)
    {
        PopupRefs refs = new PopupRefs();

        GameObject root = CreateUiObject("RewardPopupRoot", parent);
        refs.root = root;
        StretchFullScreen(root.GetComponent<RectTransform>());

        GameObject overlay = CreateUiObject("PopupOverlay", root.transform);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.45f);
        StretchFullScreen(overlay.GetComponent<RectTransform>());

        GameObject panel = CreateUiObject("PopupPanel", root.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = popupSprite;
        panelImage.preserveAspect = true;
        panelImage.raycastTarget = false;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 620f);
        panelRect.anchoredPosition = Vector2.zero;

        refs.titleText = CreateTmpText("PopupTitleText", panel.transform, fontAsset, "DAILY REWARD", 44f, FontStyles.Bold);
        refs.titleText.alignment = TextAlignmentOptions.Center;
        refs.titleText.color = Color.white;
        refs.titleText.outlineColor = Color.black;
        refs.titleText.outlineWidth = 0.05f;
        AnchorRect(refs.titleText.rectTransform, new Vector2(0.2f, 0.81f), new Vector2(0.8f, 0.96f));

        GameObject rewardImageObject = CreateUiObject("PopupRewardIcon", panel.transform);
        refs.rewardIcon = rewardImageObject.AddComponent<Image>();
        refs.rewardIcon.sprite = rewardSprite;
        refs.rewardIcon.preserveAspect = true;
        refs.rewardIcon.raycastTarget = false;
        RectTransform rewardRect = rewardImageObject.GetComponent<RectTransform>();
        rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardRect.pivot = new Vector2(0.5f, 0.5f);
        rewardRect.sizeDelta = new Vector2(155f, 155f);
        rewardRect.anchoredPosition = new Vector2(0f, 82f);

        refs.line1Text = CreateTmpText("PopupLine1Text", panel.transform, fontAsset, "DAY 1 REWARD", 30f, FontStyles.Bold);
        refs.line1Text.alignment = TextAlignmentOptions.Center;
        refs.line1Text.color = Color.white;
        refs.line1Text.outlineColor = Color.black;
        refs.line1Text.outlineWidth = 0.04f;
        AnchorRect(refs.line1Text.rectTransform, new Vector2(0.18f, 0.37f), new Vector2(0.82f, 0.46f));

        refs.line2Text = CreateTmpText("PopupLine2Text", panel.transform, fontAsset, "+50 COINS", 34f, FontStyles.Bold);
        refs.line2Text.alignment = TextAlignmentOptions.Center;
        refs.line2Text.color = new Color(1f, 0.91f, 0.35f, 1f);
        refs.line2Text.outlineColor = Color.black;
        refs.line2Text.outlineWidth = 0.04f;
        AnchorRect(refs.line2Text.rectTransform, new Vector2(0.18f, 0.27f), new Vector2(0.82f, 0.36f));

        refs.okButton = CreatePopupOkButton(panel.transform, fontAsset);

        root.SetActive(false);
        return refs;
    }

    private static Button CreatePopupOkButton(Transform parent, TMP_FontAsset fontAsset)
    {
        GameObject buttonObject = CreateUiObject("PopupOkButton", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.1f, 0.63f, 0.95f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(250f, 82f);
        rect.anchoredPosition = new Vector2(0f, 54f);

        TextMeshProUGUI text = CreateTmpText("PopupOkButtonText", buttonObject.transform, fontAsset, "OK", 36f, FontStyles.Bold);
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineColor = Color.black;
        text.outlineWidth = 0.04f;
        StretchWithPadding(text.rectTransform, 8f, 4f);
        return button;
    }

    private static DailyRewardController CreateController(
        Transform parent,
        Button homeButton,
        Button claimButton,
        Button claimedButton,
        PopupRefs popupRefs,
        Sprite coinRewardSprite,
        Sprite motorRewardSprite)
    {
        GameObject controllerObject = new GameObject("DailyRewardController");
        controllerObject.transform.SetParent(parent, false);
        DailyRewardController controller = controllerObject.AddComponent<DailyRewardController>();

        SerializedObject serializedObject = new SerializedObject(controller);
        serializedObject.FindProperty("homeButton").objectReferenceValue = homeButton;
        serializedObject.FindProperty("claimButton").objectReferenceValue = claimButton;
        serializedObject.FindProperty("claimedButton").objectReferenceValue = claimedButton;
        serializedObject.FindProperty("popupOkButton").objectReferenceValue = popupRefs.okButton;
        serializedObject.FindProperty("rewardPopupRoot").objectReferenceValue = popupRefs.root;
        serializedObject.FindProperty("popupRewardIcon").objectReferenceValue = popupRefs.rewardIcon;
        serializedObject.FindProperty("popupTitleText").objectReferenceValue = popupRefs.titleText;
        serializedObject.FindProperty("popupLine1Text").objectReferenceValue = popupRefs.line1Text;
        serializedObject.FindProperty("popupLine2Text").objectReferenceValue = popupRefs.line2Text;
        serializedObject.FindProperty("coinRewardSprite").objectReferenceValue = coinRewardSprite;
        serializedObject.FindProperty("motorRewardSprite").objectReferenceValue = motorRewardSprite;
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

    private static TextMeshProUGUI CreateTmpText(string objectName, Transform parent, TMP_FontAsset fontAsset, string value, float size, FontStyles style)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = fontAsset;
        text.fontSize = size;
        text.fontStyle = style;
        text.enableWordWrapping = false;
        return text;
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
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

    private static void AnchorRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning("DailyRewardSceneBuilder: Missing sprite at " + path);
        }

        return sprite;
    }

    private sealed class DaySlotRefs
    {
        public GameObject root;
        public TextMeshProUGUI dayLabel;
        public Image rewardIcon;
        public TextMeshProUGUI rewardAmountText;
        public Image claimBadge;
    }

    private sealed class PopupRefs
    {
        public GameObject root;
        public Image rewardIcon;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI line1Text;
        public TextMeshProUGUI line2Text;
        public Button okButton;
    }
}

using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 앱을 켜면 처음 나오는 로딩(타이틀) 화면을 한 번에 만들어 주는 "에디터 전용 도구"입니다.
// 유니티 위쪽 메뉴 Bushi > 타이틀(로딩) 화면 만들기 를 누르면:
// 1) MainMenuScene에 게임 타이틀(BUSHI) 그림을 추가합니다.
// 2) TitleScene을 새로 만들어 배경·큰 타이틀·제작자 이름·로딩 스크립트를 배치합니다.
// 3) Build Settings 씬 순서를 TitleScene(0) → MainMenuScene(1) → InGameScene(2)로 맞춥니다.
// (게임 빌드에는 들어가지 않습니다. 만든 뒤에는 보통 UI처럼 Inspector에서 고쳐도 됩니다)
public static class TitleSceneBuilder
{
    private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenuScene.unity";
    private const string TitleSpritePath = "Assets/Sprites/UI/Bushi_Title.png";
    private const string BackgroundPath = "Assets/Sprites/BackGround/PlayBG.jpeg";
    private const string FontPath = "Assets/Fonts/Galmuri11.asset";

    // 제작자 이름 (만든 뒤 TitleScene의 CreatorText에서 바꿀 수 있습니다. 폰트에 영문만 있어서 영어로 씁니다)
    private const string CreatorName = "by Skarnd0814";

    // 타이틀 그림(여백을 잘라 낸 417x186)의 정확한 배수로 크기를 정해 도트가 깨지지 않게 합니다.
    private static readonly Vector2 MenuTitleSize = new Vector2(834f, 372f);      // 2배
    private static readonly Vector2 MenuTitlePosition = new Vector2(0f, 290f);    // BEST 표시 위쪽 빈 공간
    private static readonly Vector2 LoadingTitleSize = new Vector2(1251f, 558f);  // 3배
    private static readonly Vector2 LoadingTitlePosition = new Vector2(0f, 40f);

    // 제작자 이름 뒤의 검은 상자 (게임 오버 화면의 어두운 판과 같은 투명도)
    private static readonly Color CreatorBackgroundColor = new Color(0f, 0f, 0f, 0.63f);
    private static readonly RectOffset CreatorBackgroundPadding = new RectOffset(24, 24, 8, 8); // 왼쪽, 오른쪽, 위, 아래 여백

    [MenuItem("Bushi/타이틀(로딩) 화면 만들기")]
    private static void Build()
    {
        if (File.Exists(TitleScenePath))
        {
            EditorUtility.DisplayDialog("타이틀 화면 만들기", "이미 TitleScene이 있습니다.\n다시 만들려면 Assets/Scenes/TitleScene을 먼저 지워 주세요.", "확인");
            return;
        }

        // 지금 열린 씬에 저장하지 않은 변경이 있으면 먼저 저장할지 물어봅니다.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite titleSprite = LoadSprite(TitleSpritePath);
        Sprite backgroundSprite = LoadSprite(BackgroundPath);
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (titleSprite == null || backgroundSprite == null || font == null)
        {
            EditorUtility.DisplayDialog("타이틀 화면 만들기", "필요한 그림이나 폰트를 찾지 못했습니다. Console 창의 노란 경고를 확인해 주세요.", "확인");
            return;
        }

        AddTitleToMainMenu(titleSprite);
        CreateTitleScene(titleSprite, backgroundSprite, font);
        SetBuildOrder();

        EditorUtility.DisplayDialog("타이틀 화면 만들기",
            "완료했습니다.\n- MainMenuScene에 GameTitle 추가\n- TitleScene 생성 (지금 열려 있음)\n- 씬 순서: TitleScene → MainMenuScene → InGameScene\n\nTitleScene에서 Play를 눌러 확인해 보세요.", "확인");
    }

    // ===== 1. 메인 메뉴 타이틀 =====
    private static void AddTitleToMainMenu(Sprite titleSprite)
    {
        Scene menuScene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

        if (Object.FindFirstObjectByType<MainMenuTitle>() == null)
        {
            MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();
            Transform canvas = menu != null ? menu.transform : null;
            if (canvas == null)
            {
                Debug.LogWarning("타이틀 화면 만들기: MainMenuScene에서 MainMenuController(Canvas)를 찾지 못해 메인 메뉴 타이틀을 추가하지 않았습니다.");
                return;
            }

            RectTransform title = CreateRect("GameTitle", canvas, new Vector2(0.5f, 0.5f), MenuTitlePosition, MenuTitleSize);
            AddImage(title.gameObject, titleSprite, false).preserveAspect = true;
            title.gameObject.AddComponent<MainMenuTitle>();

            // 배경 바로 위(앞)에 그려지도록 Background 다음 순서에 둡니다. 버튼·팝업보다는 뒤입니다.
            Transform background = canvas.Find("Background");
            title.SetSiblingIndex(background != null ? background.GetSiblingIndex() + 1 : 0);
        }

        EditorSceneManager.SaveScene(menuScene);
    }

    // ===== 2. 로딩(타이틀) 씬 =====
    private static void CreateTitleScene(Sprite titleSprite, Sprite backgroundSprite, TMP_FontAsset font)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라: 맨 처음 검은 화면을 그려 줍니다. 메인 메뉴 카메라(Depth -1)보다 뒤에 그려지도록 Depth -10.
        // 소리 듣는 귀(AudioListener)는 메인 메뉴가 열리는 순간 TitleLoader가 꺼서 두 개가 되지 않게 합니다.
        GameObject cameraObject = new GameObject("LoadingCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.depth = -10f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        AudioListener listener = cameraObject.AddComponent<AudioListener>();

        // 캔버스: 메인 메뉴 캔버스보다 앞에 보이도록 Sort Order 100. 기준 해상도는 다른 씬과 같게.
        GameObject canvasObject = new GameObject("LoadingCanvas", typeof(RectTransform));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>(); // 로딩 중에는 뒤의 메인 메뉴 버튼이 눌리지 않게 막습니다.
        CanvasGroup screenGroup = canvasObject.AddComponent<CanvasGroup>();
        screenGroup.alpha = 0f; // 검은 화면에서 시작

        // 배경: 인게임 배경 그림을 화면 비율에 맞춰 빈틈없이 덮습니다. (메인 메뉴 배경과 같은 방식)
        RectTransform background = CreateRect("Background", canvasObject.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920f, 1080f));
        AddImage(background.gameObject, backgroundSprite, true);
        AspectRatioFitter fitter = background.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = backgroundSprite.rect.width / backgroundSprite.rect.height;
        CanvasGroup backgroundGroup = background.gameObject.AddComponent<CanvasGroup>();

        // 가운데의 큰 타이틀
        RectTransform title = CreateRect("Title", canvasObject.transform, new Vector2(0.5f, 0.5f), LoadingTitlePosition, LoadingTitleSize);
        AddImage(title.gameObject, titleSprite, false).preserveAspect = true;

        // 제작자 이름: 타이틀 왼쪽 아래
        Vector2 creatorPosition = LoadingTitlePosition + new Vector2(-LoadingTitleSize.x * 0.5f + 40f, -LoadingTitleSize.y * 0.5f - 10f);
        RectTransform creator = CreateRect("CreatorText", canvasObject.transform, new Vector2(0.5f, 0.5f), creatorPosition, new Vector2(700f, 70f));
        creator.pivot = new Vector2(0f, 1f);
        creator.anchoredPosition = creatorPosition;
        TextMeshProUGUI creatorText = creator.gameObject.AddComponent<TextMeshProUGUI>();
        creatorText.font = font;
        creatorText.fontSize = 48; // Galmuri11은 12의 배수
        creatorText.color = Color.white;
        creatorText.alignment = TextAlignmentOptions.Left;
        creatorText.textWrappingMode = TextWrappingModes.NoWrap;
        creatorText.raycastTarget = false;
        creatorText.text = CreatorName;
        CanvasGroup creatorGroup = WrapCreatorWithBackground(creator);

        // 로딩 스크립트 연결
        TitleLoader loader = canvasObject.AddComponent<TitleLoader>();
        SerializedObject data = new SerializedObject(loader);
        data.FindProperty("screenGroup").objectReferenceValue = screenGroup;
        data.FindProperty("backgroundGroup").objectReferenceValue = backgroundGroup;
        data.FindProperty("title").objectReferenceValue = title;
        data.FindProperty("creatorGroup").objectReferenceValue = creatorGroup;
        data.FindProperty("loadingListener").objectReferenceValue = listener;
        data.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, TitleScenePath);
    }

    // ===== 제작자 이름 뒤 검은 상자 =====

    // 이미 만든 TitleScene에 검은 상자만 추가하는 메뉴입니다.
    [MenuItem("Bushi/제작자 이름에 검은 배경 넣기")]
    private static void AddCreatorBackground()
    {
        TitleLoader loader = Object.FindFirstObjectByType<TitleLoader>();
        if (loader == null)
        {
            EditorUtility.DisplayDialog("제작자 이름 배경", "TitleScene을 연 다음 다시 눌러 주세요.", "확인");
            return;
        }
        if (loader.transform.Find("CreatorBackground") != null)
        {
            EditorUtility.DisplayDialog("제작자 이름 배경", "이미 CreatorBackground가 있습니다.", "확인");
            return;
        }
        RectTransform creator = loader.transform.Find("CreatorText") as RectTransform;
        if (creator == null)
        {
            EditorUtility.DisplayDialog("제작자 이름 배경", "LoadingCanvas 아래에서 CreatorText를 찾지 못했습니다.", "확인");
            return;
        }

        // 사라지는 효과(CanvasGroup)를 글자에서 상자로 옮겨서, 상자와 글자가 함께 사라지게 합니다.
        CanvasGroup oldGroup = creator.GetComponent<CanvasGroup>();
        if (oldGroup != null) Undo.DestroyObjectImmediate(oldGroup);

        CanvasGroup creatorGroup = WrapCreatorWithBackground(creator);

        SerializedObject data = new SerializedObject(loader);
        data.FindProperty("creatorGroup").objectReferenceValue = creatorGroup;
        data.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(loader.gameObject.scene);
        Selection.activeGameObject = creatorGroup.gameObject;
        EditorUtility.DisplayDialog("제작자 이름 배경", "CreatorText 뒤에 검은 상자(CreatorBackground)를 넣었습니다.\nCtrl+S로 씬을 저장해 주세요.", "확인");
    }

    // 제작자 이름을 검은 상자로 감쌉니다. 상자는 글자 길이에 맞춰 크기가 저절로 바뀝니다.
    // (부모가 자식보다 먼저 그려지므로, 상자를 글자의 부모로 두면 상자가 글자 뒤에 보입니다)
    private static CanvasGroup WrapCreatorWithBackground(RectTransform creator)
    {
        RectTransform box = CreateRect("CreatorBackground", creator.parent, creator.anchorMin, creator.anchoredPosition, Vector2.zero);
        box.pivot = creator.pivot; // 글자와 같은 왼쪽 위 모서리 기준
        box.anchoredPosition = creator.anchoredPosition;
        box.SetSiblingIndex(creator.GetSiblingIndex());
        Undo.RegisterCreatedObjectUndo(box.gameObject, "제작자 이름 배경");

        Image image = AddImage(box.gameObject, null, false);
        image.color = CreatorBackgroundColor;

        // 자식(글자)의 크기 + 여백만큼 상자 크기를 자동으로 맞춥니다.
        HorizontalLayoutGroup layout = box.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = CreatorBackgroundPadding;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        ContentSizeFitter fitter = box.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Undo.SetTransformParent(creator, box, "제작자 이름 배경");
        return box.gameObject.AddComponent<CanvasGroup>();
    }

    // ===== 3. 씬 순서 =====
    private static void SetBuildOrder()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != TitleScenePath).ToList();
        scenes.Insert(0, new EditorBuildSettingsScene(TitleScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------- 만들기 도우미 ----------

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image AddImage(GameObject go, Sprite sprite, bool raycastTarget)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        if (sprite == null) Debug.LogWarning($"타이틀 화면 만들기: '{path}'에서 그림을 찾지 못했습니다.");
        return sprite;
    }
}

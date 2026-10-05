using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 메인 메뉴에 스킬 상점 UI(SHOP 버튼 + 상점 팝업)를 한 번에 만들어 주는 "에디터 전용 도구"입니다.
// 유니티 위쪽 메뉴 Bushi > 스킬 상점 UI 만들기 를 누르면 실행됩니다. (게임 빌드에는 들어가지 않습니다)
// 만든 뒤에는 보통 UI처럼 Inspector에서 위치·크기·글자를 자유롭게 고쳐도 됩니다.
public static class SkillShopBuilder
{
    // ---------- 사용하는 그림·폰트 경로 ----------
    private const string ShopPanelPath = "Assets/Sprites/UI/ShopUI.png";
    private const string ShopButtonPath = "Assets/Sprites/UI/ShopButton.png";
    private const string CloseButtonPath = "Assets/Sprites/UI/CloseButton-removebg-preview.png";
    private const string SkillIconPath = "Assets/Sprites/SkillEffect/Bushi_Skill_Icon.png";
    private const string CoinPath = "Assets/Sprites/SkillEffect/Coin_Split.png";
    private const string FontPath = "Assets/Fonts/Galmuri11.asset";

    // ---------- 색 ----------
    private static readonly Color Brown = new Color(0.29f, 0.17f, 0.09f);     // 설정창 글자와 같은 갈색
    private static readonly Color LightBrown = new Color(0.45f, 0.3f, 0.18f);
    private static readonly Color Gold = new Color(1f, 0.84f, 0.29f);         // BEST 글자와 같은 금색
    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);          // 팝업 뒤 어두운 배경

    // ---------- 상품 정보 (만든 뒤 Inspector에서 바꿀 수 있습니다) ----------
    private static readonly string[] SkillNames = { "CYCLONE SLASH", "TRIPLE COMBO", "RISING CRESCENT" };
    private static readonly string[] SkillDescriptions = { "Spin & slash around", "3 piercing sword waves", "Air jump + big wave" };
    private static readonly int[] Prices = { 100, 150, 200 };

    [MenuItem("Bushi/스킬 상점 UI 만들기")]
    private static void Build()
    {
        MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();
        if (menu == null)
        {
            EditorUtility.DisplayDialog("스킬 상점 만들기", "MainMenuScene을 연 다음 다시 눌러 주세요.\n(MainMenuController를 찾지 못했습니다)", "확인");
            return;
        }

        Transform canvas = menu.transform;
        if (canvas.Find("SkillShopPopup") != null || canvas.Find("ShopButton") != null)
        {
            EditorUtility.DisplayDialog("스킬 상점 만들기", "이미 ShopButton 또는 SkillShopPopup이 있습니다.\n다시 만들려면 두 오브젝트를 먼저 지워 주세요.", "확인");
            return;
        }

        Sprite panelSprite = LoadSprite(ShopPanelPath, "ShopUI_0");
        Sprite buttonSprite = LoadSprite(ShopButtonPath, "ShopButton_0");
        Sprite closeSprite = LoadSprite(CloseButtonPath, null);
        Sprite coinSprite = LoadSprite(CoinPath, "Coin_Split_0");
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        Sprite[] skillIcons =
        {
            LoadSprite(SkillIconPath, "Bushi_Skill_Icon_0"),
            LoadSprite(SkillIconPath, "Bushi_Skill_Icon_1"),
            LoadSprite(SkillIconPath, "Bushi_Skill_Icon_2"),
        };
        if (panelSprite == null || buttonSprite == null || coinSprite == null || font == null || skillIcons.Any(s => s == null))
        {
            EditorUtility.DisplayDialog("스킬 상점 만들기", "필요한 그림이나 폰트를 찾지 못했습니다. Console 창의 노란 경고를 확인해 주세요.", "확인");
            return;
        }

        // ===== 1. 메인 메뉴 오른쪽 위의 SHOP 버튼 =====
        RectTransform shopButton = CreateRect("ShopButton", canvas, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-95f, -80f), new Vector2(286f, 130f));
        Button shopButtonComponent = MakeButton(shopButton.gameObject, buttonSprite);
        CreateText("Text", shopButton, Vector2.zero, new Vector2(240f, 80f), "SHOP", 48, Color.white, TextAlignmentOptions.Center, font);

        // 설정창 팝업보다 뒤에 그려지도록 SettingsPopup 바로 앞 순서에 둡니다.
        Transform settingsPopup = canvas.Find("SettingsPopup");
        if (settingsPopup != null) shopButton.SetSiblingIndex(settingsPopup.GetSiblingIndex());

        // ===== 2. 상점 팝업: 화면 전체를 덮는 어두운 배경 =====
        RectTransform popup = CreateRect("SkillShopPopup", canvas, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        popup.anchorMin = Vector2.zero;
        popup.anchorMax = Vector2.one;
        popup.offsetMin = Vector2.zero;
        popup.offsetMax = Vector2.zero;
        AddImage(popup.gameObject, null, Dim, true); // 뒤의 메인 메뉴 버튼이 눌리지 않게 막습니다.
        popup.SetAsLastSibling();

        // 나무 액자 패널 (원본 그림 598x394의 정확히 2배)
        RectTransform panel = CreateRect("Panel", popup, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1196f, 788f));
        AddImage(panel.gameObject, panelSprite, Color.white, true);

        // 위쪽 이름표 칸의 제목
        CreateText("TitleText", panel, new Vector2(0f, 330f), new Vector2(300f, 56f), "SKILL SHOP", 36, Gold, TextAlignmentOptions.Center, font);

        // 닫기 버튼 (설정창과 같은 그림)
        RectTransform close = CreateRect("CloseButton", panel, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-40f, -40f), new Vector2(110f, 110f));
        Button closeButton = MakeButton(close.gameObject, closeSprite);

        // 상점 안 보유 코인 표시
        RectTransform coinIcon = CreateRect("CoinIcon", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-70f, 225f), new Vector2(56f, 56f));
        AddImage(coinIcon.gameObject, coinSprite, Color.white, false).preserveAspect = true;
        TMP_Text coinText = CreateText("CoinText", panel, new Vector2(70f, 225f), new Vector2(200f, 60f), "0", 48, Brown, TextAlignmentOptions.Left, font);

        // 안내 글자 (PURCHASED! / NOT ENOUGH COINS)
        TMP_Text messageText = CreateText("MessageText", panel, new Vector2(0f, -255f), new Vector2(900f, 40f), "NOT ENOUGH COINS", 24, Brown, TextAlignmentOptions.Center, font);
        messageText.gameObject.SetActive(false);

        // ===== 3. 스킬 상품 칸 3개 =====
        float[] columnX = { -310f, 0f, 310f };
        for (int i = 0; i < 3; i++)
        {
            CreateItem(panel, i, columnX[i], skillIcons[i], buttonSprite, coinSprite, font);
        }

        // ===== 4. 스크립트 연결 =====
        SkillShopPopup popupComponent = popup.gameObject.AddComponent<SkillShopPopup>();
        SerializedObject popupData = new SerializedObject(popupComponent);
        popupData.FindProperty("closeButton").objectReferenceValue = closeButton;
        popupData.FindProperty("coinText").objectReferenceValue = coinText;
        popupData.FindProperty("messageText").objectReferenceValue = messageText;
        popupData.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject menuData = new SerializedObject(menu);
        menuData.FindProperty("shopButton").objectReferenceValue = shopButtonComponent;
        menuData.FindProperty("shopPopup").objectReferenceValue = popupComponent;
        menuData.ApplyModifiedProperties();

        // 처음에는 닫혀 있게 합니다. (SHOP 버튼을 누르면 열립니다)
        popup.gameObject.SetActive(false);

        Undo.RegisterCreatedObjectUndo(shopButton.gameObject, "스킬 상점 UI 만들기");
        Undo.RegisterCreatedObjectUndo(popup.gameObject, "스킬 상점 UI 만들기");
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = popup.gameObject;

        EditorUtility.DisplayDialog("스킬 상점 만들기",
            "ShopButton과 SkillShopPopup을 만들었습니다.\nCtrl+S로 씬을 저장해 주세요.", "확인");
    }

    // 상품 칸 하나: 아이콘 / 이름 / 설명 / 가격 버튼
    private static void CreateItem(Transform panel, int index, float x, Sprite icon, Sprite buttonSprite, Sprite coinSprite, TMP_FontAsset font)
    {
        RectTransform item = CreateRect($"SkillItem{index + 1}", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(x, 0f), new Vector2(290f, 500f));

        RectTransform iconRect = CreateRect("Icon", item, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(256f, 162f));
        AddImage(iconRect.gameObject, icon, Color.white, false).preserveAspect = true;

        CreateText("NameText", item, new Vector2(0f, -20f), new Vector2(290f, 40f), SkillNames[index], 24, Brown, TextAlignmentOptions.Center, font);
        CreateText("DescText", item, new Vector2(0f, -60f), new Vector2(290f, 36f), SkillDescriptions[index], 24, LightBrown, TextAlignmentOptions.Center, font);

        RectTransform buy = CreateRect("BuyButton", item, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(240f, 110f));
        Button buyButton = MakeButton(buy.gameObject, buttonSprite);

        RectTransform priceCoin = CreateRect("CoinIcon", buy, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-55f, 4f), new Vector2(48f, 48f));
        AddImage(priceCoin.gameObject, coinSprite, Color.white, false).preserveAspect = true;
        TMP_Text priceText = CreateText("PriceText", buy, new Vector2(25f, 4f), new Vector2(180f, 60f), Prices[index].ToString(), 36, Color.white, TextAlignmentOptions.Center, font);

        SkillShopItem itemComponent = item.gameObject.AddComponent<SkillShopItem>();
        SerializedObject data = new SerializedObject(itemComponent);
        data.FindProperty("skillNumber").intValue = index + 1;
        data.FindProperty("price").intValue = Prices[index];
        data.FindProperty("buyButton").objectReferenceValue = buyButton;
        data.FindProperty("buyButtonText").objectReferenceValue = priceText;
        data.FindProperty("priceCoinIcon").objectReferenceValue = priceCoin.gameObject;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- 만들기 도우미 ----------

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image AddImage(GameObject go, Sprite sprite, Color color, bool raycastTarget)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    // 그림 버튼 + 클릭음
    private static Button MakeButton(GameObject go, Sprite sprite)
    {
        Image image = AddImage(go, sprite, Color.white, true);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        go.AddComponent<ButtonClickSound>();
        return button;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, string text,
        float fontSize, Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        tmp.text = text;
        return tmp;
    }

    // 여러 장으로 잘린 그림에서 이름이 같은 조각을 찾습니다. (spriteName이 null이면 첫 조각)
    private static Sprite LoadSprite(string path, string spriteName)
    {
        Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .FirstOrDefault(s => spriteName == null || s.name == spriteName);
        if (sprite == null) Debug.LogWarning($"스킬 상점 만들기: '{path}'에서 그림 '{spriteName}'을(를) 찾지 못했습니다.");
        return sprite;
    }
}

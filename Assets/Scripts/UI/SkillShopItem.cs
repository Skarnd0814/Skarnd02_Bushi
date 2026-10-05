using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스킬 상점의 상품 칸 하나입니다. (아이콘, 이름, 설명, 가격 버튼)
// 버튼이 눌리면 SkillShopPopup에 알리고, 실제 구매(코인 빼기, 잠금 해제)는 SkillShopPopup이 처리합니다.
public class SkillShopItem : MonoBehaviour
{
    [Header("상품")]
    [Tooltip("몇 번 스킬을 파는 칸인지 (1~3). Player에 붙은 스킬 스크립트의 Skill Number와 같아야 합니다.")]
    [SerializeField] private int skillNumber = 1;
    [Tooltip("가격 (코인)")]
    [SerializeField] private int price = 100;

    [Header("화면 연결")]
    [SerializeField] private Button buyButton;
    [Tooltip("버튼 위의 글자 (가격 숫자 또는 OWNED)")]
    [SerializeField] private TMP_Text buyButtonText;
    [Tooltip("가격 숫자 옆의 작은 코인 그림 (이미 산 스킬이면 숨깁니다)")]
    [SerializeField] private GameObject priceCoinIcon;

    [Header("글자 색")]
    [SerializeField] private Color affordableColor = Color.white;
    [Tooltip("코인이 모자랄 때 가격 색")]
    [SerializeField] private Color notEnoughColor = new Color(1f, 0.45f, 0.4f);
    [SerializeField] private Color ownedColor = new Color(1f, 0.84f, 0.29f);

    private Vector2 priceTextPosition;

    public int SkillNumber => skillNumber;
    public int Price => price;
    public bool IsOwned => SkillUnlocks.IsUnlocked(skillNumber);

    // 구매 버튼이 눌렸을 때 알려 줍니다. (SkillShopPopup이 듣고 구매를 처리합니다)
    public event Action<SkillShopItem> BuyClicked;

    private void Awake()
    {
        buyButton.onClick.AddListener(() => BuyClicked?.Invoke(this));
        priceTextPosition = buyButtonText.rectTransform.anchoredPosition;
    }

    // 보유 코인에 맞춰 버튼 모습을 다시 그립니다.
    public void Refresh(int coins)
    {
        bool owned = IsOwned;

        if (priceCoinIcon != null) priceCoinIcon.SetActive(!owned);

        // 이미 산 스킬: 코인 그림 없이 버튼 가운데에 OWNED, 버튼은 눌리지 않게
        buyButtonText.text = owned ? "OWNED" : price.ToString();
        buyButtonText.rectTransform.anchoredPosition = owned ? new Vector2(0f, priceTextPosition.y) : priceTextPosition;
        buyButtonText.color = owned ? ownedColor : (coins >= price ? affordableColor : notEnoughColor);

        // 코인이 모자라도 버튼은 눌리게 둡니다. (누르면 "NOT ENOUGH COINS" 안내가 나옵니다)
        buyButton.interactable = !owned;
    }
}

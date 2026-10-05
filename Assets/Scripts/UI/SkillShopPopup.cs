using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메인 메뉴의 스킬 상점 팝업입니다.
// - 열 때마다 보유 코인과 각 스킬의 구매 상태를 새로 보여 줍니다.
// - 구매 버튼을 누르면 코인이 충분할 때만 코인을 빼고(CoinWallet) 스킬을 잠금 해제합니다(SkillUnlocks).
// 상품 칸(SkillShopItem)은 이 팝업 안에 있는 것을 자동으로 찾습니다.
public class SkillShopPopup : MonoBehaviour
{
    [Header("화면 연결")]
    [SerializeField] private Button closeButton;
    [Tooltip("상점 안에 보여 줄 보유 코인 숫자")]
    [SerializeField] private TMP_Text coinText;
    [Tooltip("\"PURCHASED!\", \"NOT ENOUGH COINS\" 같은 안내 글자")]
    [SerializeField] private TMP_Text messageText;
    [Tooltip("안내 글자를 몇 초 동안 보여 줄지")]
    [SerializeField] private float messageDuration = 1.5f;
    [SerializeField] private Color successColor = new Color(0.15f, 0.45f, 0.1f);
    [SerializeField] private Color failColor = new Color(0.7f, 0.15f, 0.1f);

    [Header("소리 (비워 두면 재생하지 않습니다 / 설정창의 SFX 볼륨을 따릅니다)")]
    [Tooltip("구매에 성공했을 때")]
    [SerializeField] private AudioClip purchaseSound;
    [Tooltip("코인이 모자라서 살 수 없을 때")]
    [SerializeField] private AudioClip notEnoughSound;

    private SkillShopItem[] items;
    private float messageHideTime;

    private void Awake()
    {
        items = GetComponentsInChildren<SkillShopItem>(true);
        foreach (SkillShopItem item in items) item.BuyClicked += OnBuyClicked;

        closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        CoinWallet.Changed += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinWallet.Changed -= OnCoinsChanged;
    }

    private void Update()
    {
        if (messageText != null && messageText.gameObject.activeSelf && Time.unscaledTime >= messageHideTime)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        if (messageText != null) messageText.gameObject.SetActive(false);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void OnBuyClicked(SkillShopItem item)
    {
        if (item.IsOwned) return;

        if (CoinWallet.TrySpend(item.Price))
        {
            SkillUnlocks.Unlock(item.SkillNumber);
            PlaySound(purchaseSound);
            ShowMessage("PURCHASED!", successColor);
        }
        else
        {
            PlaySound(notEnoughSound);
            ShowMessage("NOT ENOUGH COINS", failColor);
        }
        Refresh();
    }

    private void OnCoinsChanged(int coins)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (items == null) return;

        int coins = CoinWallet.Coins;
        if (coinText != null) coinText.text = coins.ToString();
        foreach (SkillShopItem item in items) item.Refresh(coins);
    }

    private void ShowMessage(string message, Color color)
    {
        if (messageText == null) return;

        messageText.text = message;
        messageText.color = color;
        messageText.gameObject.SetActive(true);
        messageHideTime = Time.unscaledTime + messageDuration;
    }

    private void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clip);
    }
}

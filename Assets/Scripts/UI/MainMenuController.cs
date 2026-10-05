using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메인 메뉴의 버튼들이 눌렸을 때 무슨 일을 할지 정하는 스크립트입니다.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsPopup settingsPopup;

    [Header("최고 점수 표시 (비워 두면 표시하지 않습니다)")]
    [SerializeField] private TMP_Text bestScoreText;

    [Header("보유 코인 표시 (비워 두면 표시하지 않습니다)")]
    [SerializeField] private TMP_Text coinText;

    [Header("START를 누르면 이동할 씬 이름")]
    [SerializeField] private string inGameSceneName = "InGameScene";

    private void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(settingsPopup.Open);
    }

    private void Start()
    {
        if (bestScoreText != null)
        {
            int bestScore = PlayerPrefs.GetInt(ScoreManager.BestScoreKey, 0);
            bestScoreText.text = $"BEST {bestScore}";
        }

        RefreshCoinText(CoinWallet.Coins);
    }

    // 나중에 상점에서 코인을 쓰면 숫자가 바로 바뀌도록 지갑의 알림을 듣습니다.
    private void OnEnable()
    {
        CoinWallet.Changed += RefreshCoinText;
    }

    private void OnDisable()
    {
        CoinWallet.Changed -= RefreshCoinText;
    }

    private void RefreshCoinText(int coins)
    {
        if (coinText != null) coinText.text = coins.ToString();
    }

    private void StartGame()
    {
        // 두 번 눌러서 씬을 두 번 불러오는 일이 없도록 버튼을 잠급니다.
        startButton.interactable = false;
        SceneManager.LoadScene(inGameSceneName);
    }
}

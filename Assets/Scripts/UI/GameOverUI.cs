using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 오버 화면을 보여 주고, "다시 하기" / "메인 메뉴" 버튼을 처리하는 스크립트입니다.
// 이 스크립트는 항상 켜져 있는 오브젝트(Canvas)에 붙이고, 화면 자체(screenRoot)는 처음에 숨겨 둡니다.
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ComboManager comboManager;
    [Tooltip("이번 판에 얻은 코인을 보여 줄 때 연결합니다 (비워 두면 코인 줄을 표시하지 않습니다)")]
    [SerializeField] private CoinManager coinManager;

    [Header("게임 오버 화면")]
    [Tooltip("게임 오버 때 나타날 화면 전체 (어두운 판과 그 안의 모든 것)")]
    [SerializeField] private GameObject screenRoot;
    [SerializeField] private TMP_Text resultText;
    [Tooltip("최고 점수를 새로 세웠을 때만 보여 줄 \"NEW BEST!\" 글자")]
    [SerializeField] private GameObject newBestLabel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [Tooltip("결과 글자에서 \"COIN +12\" 줄의 색")]
    [SerializeField] private Color coinLineColor = new Color(1f, 0.84f, 0.29f);

    [Header("게임 오버 때 숨길 것들 (조작 버튼, 콤보 글자 등)")]
    [SerializeField] private GameObject[] hideOnGameOver;

    [Header("설정")]
    [Tooltip("죽고 나서 몇 초 뒤에 게임 오버 화면을 보여 줄지 (사망 모션 길이에 맞춥니다)")]
    [SerializeField] private float showDelay = 1.2f;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private void Awake()
    {
        screenRoot.SetActive(false);
        restartButton.onClick.AddListener(Restart);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void OnEnable()
    {
        gameManager.GameOverHappened += OnGameOver;
    }

    private void OnDisable()
    {
        gameManager.GameOverHappened -= OnGameOver;
    }

    private void OnGameOver()
    {
        foreach (GameObject target in hideOnGameOver)
        {
            if (target != null) target.SetActive(false);
        }

        StartCoroutine(ShowAfterDelay());
    }

    // Coroutine(코루틴): 중간에 "몇 초 기다리기"를 넣을 수 있는 특별한 함수입니다.
    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(showDelay);

        resultText.text =
            $"SCORE {scoreManager.Score}\n" +
            $"BEST {scoreManager.BestScore}\n" +
            $"MAX COMBO {comboManager.MaxCombo}";

        // 이번 판에 얻은 코인은 금색으로 한 줄 더 보여 줍니다. (<color>: 이 부분만 글자 색을 바꾸는 표시)
        if (coinManager != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(coinLineColor);
            resultText.text += $"\n<color=#{hex}>COIN +{coinManager.EarnedThisRun}</color>";
        }

        newBestLabel.SetActive(scoreManager.IsNewBest);
        screenRoot.SetActive(true);
    }

    private void Restart()
    {
        LockButtons();
        // 지금 씬(InGameScene)을 처음부터 다시 엽니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        LockButtons();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 두 번 눌러서 씬을 두 번 불러오는 일이 없도록 버튼을 잠급니다.
    private void LockButtons()
    {
        restartButton.interactable = false;
        mainMenuButton.interactable = false;
    }
}

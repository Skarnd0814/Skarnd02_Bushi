using TMPro;
using UnityEngine;

// 점수를 계산하고 화면에 보여 주는 스크립트입니다.
// - 살아 있는 동안 1초마다 점수가 오릅니다.
// - 장애물을 부수면 추가 점수를 받습니다. 콤보가 높을수록 더 많이 받습니다.
// - 게임 오버 때 최고 점수를 저장합니다.
public class ScoreManager : MonoBehaviour
{
    // 최고 점수를 저장하는 이름표입니다. 메인 메뉴에서도 같은 이름으로 읽어 갑니다.
    public const string BestScoreKey = "BestScore";

    public static ScoreManager Instance { get; private set; }

    [SerializeField] private GameManager gameManager;

    [Header("생존 점수")]
    [Tooltip("살아 있는 동안 1초에 오르는 점수")]
    [SerializeField] private float survivalScorePerSecond = 10f;

    [Header("장애물 파괴 점수")]
    [Tooltip("장애물 하나를 부술 때 받는 기본 점수 (1콤보)")]
    [SerializeField] private int destroyScore = 100;
    [Tooltip("콤보가 1 오를 때마다 더해지는 점수")]
    [SerializeField] private int comboBonusPerCombo = 10;
    [Tooltip("콤보 보너스의 최대치 (기본 점수에 이 이상은 더해지지 않습니다)")]
    [SerializeField] private int maxComboBonus = 200;

    [Header("화면 표시")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;

    private float survivalScore; // 1초에 10점이면 0.016초마다 0.16점처럼 소수점으로 쌓입니다.
    private int bonusScore;
    private int lastShownScore = -1;

    public int Score => Mathf.FloorToInt(survivalScore) + bonusScore;
    public int BestScore { get; private set; }
    public bool IsNewBest { get; private set; }

    // 장애물 점수 배율입니다. 피버 타임 동안 2로 바꾸면 점수가 2배가 됩니다. (STEP 11에서 사용)
    public float DestroyScoreMultiplier { get; set; } = 1f;

    private void Awake()
    {
        Instance = this;
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    private void OnEnable()
    {
        gameManager.GameOverHappened += OnGameOver;
    }

    private void OnDisable()
    {
        gameManager.GameOverHappened -= OnGameOver;
    }

    private void Start()
    {
        RefreshText();
    }

    private void Update()
    {
        if (gameManager.IsGameOver) return;

        survivalScore += survivalScorePerSecond * Time.deltaTime;

        // 숫자가 바뀌었을 때만 글자를 다시 씁니다.
        if (Score != lastShownScore) RefreshText();
    }

    // ComboManager가 장애물을 부술 때마다 현재 콤보 수와 함께 호출합니다.
    // 예) 기본 100점, 콤보당 +10점 → 1콤보 100점, 5콤보 140점, 21콤보 이상 300점(최대)
    public int AddDestroyScore(int combo)
    {
        if (gameManager.IsGameOver) return 0;

        int comboBonus = Mathf.Min((combo - 1) * comboBonusPerCombo, maxComboBonus);
        int points = Mathf.RoundToInt((destroyScore + comboBonus) * DestroyScoreMultiplier);

        bonusScore += points;
        RefreshText();
        return points;
    }

    private void OnGameOver()
    {
        if (Score > BestScore)
        {
            BestScore = Score;
            IsNewBest = true;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }
        RefreshText();
    }

    private void RefreshText()
    {
        lastShownScore = Score;
        scoreText.text = $"SCORE {Score}";

        // 지금 점수가 최고 점수를 넘으면 BEST도 같이 올라가는 모습을 보여 줍니다.
        bestScoreText.text = $"BEST {Mathf.Max(BestScore, Score)}";
    }
}

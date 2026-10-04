using System;
using TMPro;
using UnityEngine;

// 장애물을 연속으로 부순 횟수(콤보)를 세고 화면에 보여 주는 스크립트입니다.
// - 장애물을 하나 부술 때마다 콤보 +1 (한 번 휘둘러 2개를 부수면 +2)
// - 공격했는데 아무것도 못 부수면(공격 실패) 콤보가 0으로 돌아갑니다.
// - 바닥에 떨어진 장애물은 콤보에 영향을 주지 않습니다.
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("화면 표시")]
    [SerializeField] private TMP_Text comboText;
    [Tooltip("콤보가 몇 이상일 때부터 화면에 보여 줄지")]
    [SerializeField] private int showFromCombo = 2;
    [Tooltip("콤보가 오를 때 글자가 순간적으로 커지는 크기 (1 = 효과 없음)")]
    [SerializeField] private float punchScale = 1.3f;
    [Tooltip("커졌던 글자가 원래 크기로 돌아오는 데 걸리는 시간(초)")]
    [SerializeField] private float punchDuration = 0.15f;

    private float punchTimer;

    public int Combo { get; private set; }
    public int MaxCombo { get; private set; }

    // true인 동안에는 공격에 실패해도 콤보가 끊기지 않습니다. (피버 타임에서 사용)
    public bool ProtectComboOnFail { get; set; }

    // 콤보 수가 바뀔 때마다 알려 줍니다. (피버 타임에서 사용)
    public event Action<int> ComboChanged;

    // 장애물을 부숴서 점수를 받았을 때, 부순 위치와 받은 점수를 알려 줍니다. (점수 팝업에서 사용)
    public event Action<Vector3, int> ObstacleScored;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Obstacle.Broken += OnObstacleBroken;
        playerAttack.AttackJudged += OnAttackJudged;
    }

    private void OnDisable()
    {
        Obstacle.Broken -= OnObstacleBroken;
        playerAttack.AttackJudged -= OnAttackJudged;
    }

    private void Start()
    {
        RefreshText();
    }

    private void Update()
    {
        // 콤보 글자가 "툭" 커졌다가 원래 크기로 돌아오는 효과
        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(punchTimer / punchDuration);
            comboText.transform.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, t);
        }
    }

    private void OnObstacleBroken(Obstacle obstacle)
    {
        if (gameManager.IsGameOver) return;

        Combo++;
        MaxCombo = Mathf.Max(MaxCombo, Combo);

        // 콤보를 먼저 올린 다음, 올라간 콤보로 점수를 계산합니다.
        int points = scoreManager.AddDestroyScore(Combo);
        ObstacleScored?.Invoke(obstacle.transform.position, points);

        punchTimer = punchDuration;
        RefreshText();
        ComboChanged?.Invoke(Combo);
    }

    private void OnAttackJudged(int destroyedCount)
    {
        if (destroyedCount == 0 && !ProtectComboOnFail)
        {
            ResetCombo();
        }
    }

    public void ResetCombo()
    {
        if (Combo == 0) return;

        Combo = 0;
        RefreshText();
        ComboChanged?.Invoke(Combo);
    }

    private void RefreshText()
    {
        bool visible = Combo >= showFromCombo;
        comboText.gameObject.SetActive(visible);
        if (visible) comboText.text = $"{Combo} COMBO";
    }
}

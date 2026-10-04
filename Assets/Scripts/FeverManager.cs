using System;
using TMPro;
using UnityEngine;

// 콤보가 일정 수에 도달하면 피버 타임을 발동시키는 스크립트입니다.
// 피버 타임 동안:
// - 공격 쿨타임이 없어집니다.
// - 장애물 파괴 점수가 배로 늘어납니다.
// - 무적이 됩니다. (몸에 닿은 장애물은 게임 오버 대신 부서집니다 → GameManager에서 처리)
// - 공격에 실패해도 콤보가 끊기지 않습니다.
// - 시작하자마자 장애물이 잠깐 동안 마구 쏟아집니다.
// 피버 타임이 끝나면 콤보는 0부터 다시 시작합니다.
public class FeverManager : MonoBehaviour
{
    public static FeverManager Instance { get; private set; }

    [SerializeField] private ComboManager comboManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private ObstacleSpawner obstacleSpawner;

    [Header("피버 규칙")]
    [Tooltip("콤보가 몇이 되면 피버 타임이 시작되는지")]
    [SerializeField] private int feverComboThreshold = 10;
    [Tooltip("피버 타임이 계속되는 시간(초)")]
    [SerializeField] private float feverDuration = 5f;
    [Tooltip("피버 타임 동안 장애물 파괴 점수 배율")]
    [SerializeField] private float destroyScoreMultiplier = 2f;

    [Header("피버 시작 때 장애물 쏟아지기")]
    [Tooltip("장애물이 마구 쏟아지는 시간(초)")]
    [SerializeField] private float burstDuration = 1f;
    [Tooltip("쏟아지는 동안 몇 초마다 하나씩 만들지")]
    [SerializeField] private float burstInterval = 0.05f;

    [Header("피버 끝날 때 장애물 정리")]
    [Tooltip("피버가 끝나면 화면의 모든 장애물을 없앤 뒤, 몇 초 동안 새 장애물을 만들지 않을지")]
    [SerializeField] private float spawnDelayAfterFever = 1f;

    [Header("화면 표시 (비워 두면 표시하지 않습니다)")]
    [Tooltip("\"FEVER 4.2\"처럼 남은 시간을 보여 줄 글자")]
    [SerializeField] private TMP_Text feverText;
    [Tooltip("피버 동안 반짝일 플레이어 그림")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Color feverTint = new Color(1f, 0.85f, 0.3f);
    [Tooltip("1초에 몇 번 반짝일지")]
    [SerializeField] private float blinkSpeed = 6f;

    [Header("소리 (비워 두면 재생하지 않습니다)")]
    [Tooltip("피버 시작 효과음. 여러 개 넣으면 그중 하나를 랜덤으로 재생합니다.")]
    [SerializeField] private AudioClip[] feverStartSounds;

    private float feverEndTime;

    public bool IsFever { get; private set; }
    public float RemainingTime => IsFever ? Mathf.Max(0f, feverEndTime - Time.time) : 0f;

    public event Action FeverStarted;
    public event Action FeverEnded;

    private void Awake()
    {
        Instance = this;
        if (feverText != null) feverText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        comboManager.ComboChanged += OnComboChanged;
    }

    private void OnDisable()
    {
        comboManager.ComboChanged -= OnComboChanged;
    }

    private void Update()
    {
        if (!IsFever) return;

        if (Time.time >= feverEndTime)
        {
            EndFever();
            return;
        }

        if (feverText != null) feverText.text = $"FEVER {RemainingTime:0.0}";

        if (playerSprite != null)
        {
            // 흰색(원래 색)과 금색 사이를 왔다 갔다 하며 반짝입니다.
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            playerSprite.color = Color.Lerp(Color.white, feverTint, t);
        }
    }

    private void OnComboChanged(int combo)
    {
        if (!IsFever && combo >= feverComboThreshold)
        {
            StartFever();
        }
    }

    private void StartFever()
    {
        IsFever = true;
        feverEndTime = Time.time + feverDuration;

        playerAttack.CooldownDisabled = true;
        scoreManager.DestroyScoreMultiplier = destroyScoreMultiplier;
        comboManager.ProtectComboOnFail = true;
        obstacleSpawner.StartBurst(burstDuration, burstInterval);

        if (feverText != null) feverText.gameObject.SetActive(true);
        if (feverStartSounds != null && feverStartSounds.Length > 0 && AudioManager.Instance != null)
        {
            AudioClip clip = feverStartSounds[UnityEngine.Random.Range(0, feverStartSounds.Length)];
            AudioManager.Instance.PlayCombatSound(clip);
        }

        FeverStarted?.Invoke();
    }

    private void EndFever()
    {
        IsFever = false;

        playerAttack.CooldownDisabled = false;
        scoreManager.DestroyScoreMultiplier = 1f;
        comboManager.ProtectComboOnFail = false;
        comboManager.ResetCombo();

        // 무적이 풀리는 순간 몸에 닿아 있던 장애물 때문에 바로 죽는 일이 없도록, 화면을 깨끗이 정리합니다.
        obstacleSpawner.ClearAndPause(spawnDelayAfterFever);

        if (feverText != null) feverText.gameObject.SetActive(false);
        if (playerSprite != null) playerSprite.color = Color.white;

        FeverEnded?.Invoke();
    }
}

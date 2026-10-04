using System;
using UnityEngine;

// InGameScene의 진행(게임 중 / 게임 오버)을 관리하는 스크립트입니다.
// 장애물이 플레이어에 닿으면 게임 오버를 처리합니다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private FeverManager feverManager;

    [Header("소리")]
    [Tooltip("게임 오버 효과음. 여러 개 넣으면 그중 하나를 랜덤으로 재생합니다. (설정창의 EFFECT 볼륨을 따릅니다)")]
    [SerializeField] private AudioClip[] gameOverSounds;

    public bool IsGameOver { get; private set; }

    // 게임 오버가 되었을 때 알려 줍니다. (점수 저장, 게임 오버 화면에서 사용)
    public event Action GameOverHappened;

    private void Awake()
    {
        Instance = this;
    }

    // OnEnable / OnDisable: 장애물의 "플레이어에 닿았어요" 알림을 듣기 시작하고, 그만 듣습니다.
    private void OnEnable()
    {
        Obstacle.HitPlayer += OnPlayerHit;
    }

    private void OnDisable()
    {
        Obstacle.HitPlayer -= OnPlayerHit;
    }

    private void OnPlayerHit(Obstacle obstacle)
    {
        // 피버 타임에는 무적! 게임 오버 대신 몸에 닿은 장애물을 부숩니다. (점수와 콤보도 오릅니다)
        if (feverManager != null && feverManager.IsFever)
        {
            obstacle.Break();
            return;
        }

        TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        // 이미 게임 오버라면 다시 처리하지 않습니다. (장애물 여러 개에 동시에 맞는 경우 등)
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("게임 오버!");

        obstacleSpawner.StopSpawning();
        player.Die();

        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            audio.StopBgm();
            if (gameOverSounds != null && gameOverSounds.Length > 0)
            {
                AudioClip clip = gameOverSounds[UnityEngine.Random.Range(0, gameOverSounds.Length)];
                audio.PlayCombatSound(clip);
            }
        }

        GameOverHappened?.Invoke();
    }
}

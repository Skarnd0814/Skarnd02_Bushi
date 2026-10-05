using System;
using UnityEngine;

// InGameScene에서 코인을 얻는 규칙을 담당하는 스크립트입니다.
// - 피버 타임에 장애물을 부수면 코인을 얻어 지갑(CoinWallet)에 넣습니다.
// - 게임 오버, 씬 이동, 앱이 백그라운드로 갈 때 휴대폰에 저장합니다.
// 코인 글자·이펙트·소리는 CoinPopupSpawner가 이 스크립트의 알림(CoinEarned)을 듣고 보여 줍니다.
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private FeverManager feverManager;

    [Header("코인 규칙")]
    [Tooltip("피버 타임에 장애물 하나를 부술 때 얻는 코인 수")]
    [SerializeField] private int coinsPerObstacle = 1;

    // 이번 판에서 얻은 코인 수 (게임 오버 화면 등에서 사용할 수 있습니다)
    public int EarnedThisRun { get; private set; }

    // 코인을 얻었을 때, 얻은 위치와 개수를 알려 줍니다. (코인 이펙트에서 사용)
    public event Action<Vector3, int> CoinEarned;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        comboManager.ObstacleScored += OnObstacleScored;
        gameManager.GameOverHappened += OnGameOver;
    }

    private void OnDisable()
    {
        comboManager.ObstacleScored -= OnObstacleScored;
        gameManager.GameOverHappened -= OnGameOver;
    }

    private void OnObstacleScored(Vector3 position, int points)
    {
        if (feverManager == null || !feverManager.IsFever) return;

        CoinWallet.Add(coinsPerObstacle);
        EarnedThisRun += coinsPerObstacle;
        CoinEarned?.Invoke(position, coinsPerObstacle);
    }

    private void OnGameOver()
    {
        CoinWallet.Save();
    }

    // 일시정지 → MAIN / RESTART로 씬을 떠날 때도 저장합니다.
    private void OnDestroy()
    {
        CoinWallet.Save();
    }

    // 휴대폰에서 홈 버튼을 눌러 앱이 백그라운드로 가면 저장합니다. (그대로 앱이 종료돼도 코인이 남도록)
    private void OnApplicationPause(bool paused)
    {
        if (paused) CoinWallet.Save();
    }
}

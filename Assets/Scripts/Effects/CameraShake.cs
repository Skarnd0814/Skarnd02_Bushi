using UnityEngine;

// 카메라를 잠깐 흔들어서 타격감을 주는 스크립트입니다. Main Camera에 붙입니다.
// 장애물 파괴 / 피버 시작 / 게임 오버 순간에 저절로 흔들립니다.
public class CameraShake : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private FeverManager feverManager;

    [Header("장애물 파괴 (세기 = 최대 몇 칸 흔들릴지, 시간 = 초)")]
    [SerializeField] private float breakStrength = 0.06f;
    [SerializeField] private float breakDuration = 0.1f;

    [Header("피버 시작")]
    [SerializeField] private float feverStrength = 0.15f;
    [SerializeField] private float feverDuration = 0.3f;

    [Header("게임 오버")]
    [SerializeField] private float gameOverStrength = 0.3f;
    [SerializeField] private float gameOverDuration = 0.35f;

    private Vector3 originalPosition;
    private float strength;
    private float duration;
    private float timer;

    // 지금 흔들리고 있는 세기 (시간이 지날수록 약해집니다)
    private float CurrentStrength => timer > 0f ? strength * (timer / duration) : 0f;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        Obstacle.Broken += OnObstacleBroken;
        gameManager.GameOverHappened += OnGameOver;
        if (feverManager != null) feverManager.FeverStarted += OnFeverStarted;
    }

    private void OnDisable()
    {
        Obstacle.Broken -= OnObstacleBroken;
        gameManager.GameOverHappened -= OnGameOver;
        if (feverManager != null) feverManager.FeverStarted -= OnFeverStarted;
    }

    private void OnObstacleBroken(Obstacle obstacle) => Shake(breakStrength, breakDuration);
    private void OnFeverStarted() => Shake(feverStrength, feverDuration);
    private void OnGameOver() => Shake(gameOverStrength, gameOverDuration);

    // 지금 흔들림보다 센 흔들림이 들어올 때만 바꿉니다.
    // (피버 때 장애물이 우르르 부서져도 큰 흔들림이 작은 흔들림에 덮이지 않게)
    public void Shake(float newStrength, float newDuration)
    {
        if (newStrength < CurrentStrength || newDuration <= 0f) return;

        strength = newStrength;
        duration = newDuration;
        timer = newDuration;
    }

    // LateUpdate: 다른 움직임이 모두 끝난 뒤 카메라를 움직여야 화면이 덜덜 떨리지 않습니다.
    private void LateUpdate()
    {
        if (timer <= 0f)
        {
            transform.localPosition = originalPosition;
            return;
        }

        timer -= Time.deltaTime;
        Vector2 offset = Random.insideUnitCircle * CurrentStrength;
        transform.localPosition = originalPosition + (Vector3)offset;
    }
}

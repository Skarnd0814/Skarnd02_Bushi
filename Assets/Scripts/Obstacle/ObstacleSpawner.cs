using UnityEngine;
using UnityEngine.Serialization;

// 화면 위쪽 바깥(하늘)의 랜덤한 x 위치에서 장애물을 계속 만들어 떨어뜨리는 스크립트입니다.
// 시간이 지날수록 생성 간격이 점점 짧아져서 장애물이 더 많이 떨어집니다.
public class ObstacleSpawner : MonoBehaviour
{
    [Header("장애물 종류 (이 중 하나를 랜덤으로 생성)")]
    [SerializeField] private Obstacle[] obstaclePrefabs;

    [Header("바닥 (장애물이 사라질 높이)")]
    [SerializeField] private Collider2D ground;

    [Header("게임 시작 때의 생성 간격 (초) - 이 사이에서 랜덤")]
    // FormerlySerializedAs: 이름을 바꾸기 전에 Inspector에 입력해 둔 값을 그대로 이어받습니다.
    [FormerlySerializedAs("spawnIntervalMin")]
    [SerializeField] private float startIntervalMin = 0.8f;
    [FormerlySerializedAs("spawnIntervalMax")]
    [SerializeField] private float startIntervalMax = 1.6f;

    [Header("가장 어려울 때의 생성 간격 (초)")]
    [Tooltip("후반에는 이 간격마다 하나씩 떨어집니다.")]
    [SerializeField] private float finalInterval = 0.5f;

    [Tooltip("게임 시작 후 몇 초가 지나면 가장 어려운 상태(finalInterval)가 되는지")]
    [SerializeField] private float timeToMaxDifficulty = 120f;

    [Header("생성 위치")]
    [Tooltip("화면 위쪽 끝보다 몇 칸 더 위에서 만들지")]
    [SerializeField] private float spawnHeightAboveScreen = 1.5f;
    [Tooltip("화면 왼쪽/오른쪽 끝에서 몇 칸 안쪽까지만 만들지")]
    [SerializeField] private float sideMargin = 0.6f;

    private Camera mainCamera;
    private float startTime;
    private float nextSpawnTime;

    // 장애물 쏟아지기(피버 타임 시작 때)
    private float burstEndTime;
    private float burstInterval;
    private float nextBurstSpawnTime;

    public bool IsSpawning { get; private set; } = true;
    public bool IsBursting => Time.time < burstEndTime;

    // 지금 난이도 (0 = 게임 시작, 1 = 가장 어려움)
    public float Difficulty => Mathf.Clamp01((Time.time - startTime) / timeToMaxDifficulty);

    private void Start()
    {
        mainCamera = Camera.main;
        startTime = Time.time;
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (!IsSpawning) return;

        if (Time.time >= nextSpawnTime)
        {
            Spawn();
            ScheduleNextSpawn();
        }

        // 쏟아지는 중이면 평소 생성과 별개로 짧은 간격마다 추가로 만듭니다.
        // 화면 한 장면이 간격보다 길게 걸려도 빠뜨리지 않도록, 밀린 만큼 한꺼번에 만듭니다.
        while (IsBursting && Time.time >= nextBurstSpawnTime)
        {
            Spawn();
            nextBurstSpawnTime += burstInterval;
        }
    }

    // 지금부터 duration초 동안 interval초마다 장애물을 하나씩 마구 만듭니다. (FeverManager가 호출)
    public void StartBurst(float duration, float interval)
    {
        burstInterval = Mathf.Max(0.01f, interval); // 0이면 끝없이 만들게 되므로 최소값을 둡니다.
        burstEndTime = Time.time + duration;
        nextBurstSpawnTime = Time.time;
    }

    private void Spawn()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        Obstacle prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

        float screenHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        Vector3 cameraPosition = mainCamera.transform.position;
        float x = cameraPosition.x + Random.Range(-screenHalfWidth + sideMargin, screenHalfWidth - sideMargin);
        float y = cameraPosition.y + mainCamera.orthographicSize + spawnHeightAboveScreen;

        // 만든 장애물은 이 오브젝트 안에 모아 둡니다. (Hierarchy가 지저분해지지 않게)
        Obstacle obstacle = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
        obstacle.SetLandY(ground.bounds.max.y);
    }

    private void ScheduleNextSpawn()
    {
        // 난이도가 오를수록 시작 간격(0.8~1.6초)이 최종 간격(0.5초)에 가까워집니다.
        float difficulty = Difficulty;
        float intervalMin = Mathf.Lerp(startIntervalMin, finalInterval, difficulty);
        float intervalMax = Mathf.Lerp(startIntervalMax, finalInterval, difficulty);

        nextSpawnTime = Time.time + Random.Range(intervalMin, intervalMax);
    }

    // 화면의 모든 장애물을 없애고, delay초 동안 새 장애물을 만들지 않습니다. (피버 타임이 끝날 때 사용)
    public void ClearAndPause(float delay)
    {
        // 이 생성기가 만든 장애물은 모두 이 오브젝트 안(자식)에 들어 있습니다.
        foreach (Obstacle obstacle in GetComponentsInChildren<Obstacle>())
        {
            obstacle.Vanish();
        }

        burstEndTime = 0f; // 혹시 아직 쏟아지는 중이면 멈춥니다.
        nextSpawnTime = Time.time + delay;
    }

    // 게임 오버가 되면 더 이상 만들지 않습니다. (STEP 9에서 사용)
    public void StopSpawning()
    {
        IsSpawning = false;
    }
}

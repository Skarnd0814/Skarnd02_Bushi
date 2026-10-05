using System;
using System.Collections.Generic;
using UnityEngine;

// 스킬 검기 하나의 설정값 묶음입니다. 스킬 스크립트의 Inspector에서 펼쳐서 값을 넣습니다.
[Serializable]
public class SkillProjectileSettings
{
    [Tooltip("검기 그림들 (순서대로 반복 재생)")]
    public Sprite[] frames;
    [Tooltip("그림이 왼쪽을 향해 그려져 있으면 체크하세요. (검기가 반대로 보일 때 켜고 끕니다)")]
    public bool flipSprite;
    [Tooltip("1초에 몇 장씩 넘길지")]
    public float framesPerSecond = 12f;
    [Tooltip("1초에 몇 칸 날아가는지")]
    public float speed = 12f;
    [Tooltip("그림 크기 배율 (부수는 범위도 같이 커집니다)")]
    public float scale = 1f;
    [Tooltip("장애물을 부수는 범위 (가로, 세로 / 크기 배율 1일 때 기준)")]
    public Vector2 hitBoxSize = new Vector2(1f, 1.4f);
    [Tooltip("검기가 나오는 위치 (오른쪽을 볼 때 기준, 발밑으로부터의 거리)")]
    public Vector2 spawnOffset = new Vector2(0.5f, 0.9f);
    [Tooltip("부서진 장애물의 조각을 몇 배 많이 튀길지 (1 = 보통 공격과 같음)")]
    public float debrisMultiplier = 1f;
}

// 스킬로 발사된 검기 하나입니다. 날아가면서 닿는 장애물을 모두 부수고(관통),
// 화면 밖으로 나가면 사라집니다. Spawn()을 부르면 코드가 오브젝트를 만들어 줍니다.
public class SkillProjectile : MonoBehaviour
{
    // 화면 끝에서 몇 칸 더 나가면 지울지
    private const float OffscreenMargin = 2f;

    private SkillProjectileSettings settings;
    private Vector2 velocity;
    private Camera mainCamera;
    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    // owner(플레이어) 발밑 기준으로 검기를 만듭니다.
    // facing: 오른쪽 1, 왼쪽 -1 / angleUp: 수평에서 위쪽으로 몇 도 기울여 쏠지 (0 = 수평)
    public static SkillProjectile Spawn(SkillProjectileSettings settings, Transform owner, int facing,
        float angleUp, int sortingOrder)
    {
        if (settings == null || settings.frames == null || settings.frames.Length == 0)
        {
            Debug.LogWarning("검기 그림(Frames)이 비어 있어서 검기를 만들지 못했습니다. 스킬 Inspector를 확인하세요.");
            return null;
        }

        Vector3 position = owner.position + new Vector3(settings.spawnOffset.x * facing, settings.spawnOffset.y, 0f);
        SpriteFlipbook flipbook = SpriteFlipbook.Spawn("SkillProjectile", settings.frames,
            settings.framesPerSecond, true, position, settings.scale, sortingOrder);

        // 왼쪽으로 쏠 때는 좌우를 뒤집고 기울기도 반대로 합니다.
        // 그림 자체가 왼쪽을 향해 그려져 있으면(flipSprite) 한 번 더 뒤집어서 날아가는 방향을 보게 합니다.
        flipbook.Renderer.flipX = (facing < 0) != settings.flipSprite;
        flipbook.transform.rotation = Quaternion.Euler(0f, 0f, angleUp * facing);

        float radians = angleUp * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians) * facing, Mathf.Sin(radians));

        SkillProjectile projectile = flipbook.gameObject.AddComponent<SkillProjectile>();
        projectile.settings = settings;
        projectile.velocity = direction * settings.speed;
        return projectile;
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        hitFilter = new ContactFilter2D();
        hitFilter.useTriggers = true; // 장애물의 감지 영역(Trigger)도 찾을 수 있게 합니다.
    }

    private void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (IsOutsideScreen()) Destroy(gameObject);
    }

    // 장애물을 찾는 일은 물리 계산 주기에 맞춰 합니다.
    private void FixedUpdate()
    {
        // 게임 오버 뒤에는 날아가기만 하고 부수지 않습니다.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Vector2 size = settings.hitBoxSize * settings.scale;
        Physics2D.OverlapBox(transform.position, size, transform.eulerAngles.z, hitFilter, hitResults);
        foreach (Collider2D hit in hitResults)
        {
            Obstacle obstacle = hit.GetComponentInParent<Obstacle>();
            if (obstacle != null && !obstacle.IsBroken)
            {
                obstacle.Break(settings.debrisMultiplier);
            }
        }
    }

    private bool IsOutsideScreen()
    {
        if (mainCamera == null) return false;

        Vector3 center = mainCamera.transform.position;
        float halfHeight = mainCamera.orthographicSize + OffscreenMargin;
        float halfWidth = mainCamera.orthographicSize * mainCamera.aspect + OffscreenMargin;
        Vector3 p = transform.position;
        return p.x < center.x - halfWidth || p.x > center.x + halfWidth
            || p.y < center.y - halfHeight || p.y > center.y + halfHeight;
    }

    // Scene 창에서 검기를 선택하면 부수는 범위를 빨간 사각형으로 보여 줍니다.
    private void OnDrawGizmosSelected()
    {
        if (settings == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, settings.hitBoxSize * settings.scale);
    }
}

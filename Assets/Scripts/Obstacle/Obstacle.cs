using System;
using UnityEngine;

// 장애물 하나하나에 붙는 스크립트입니다.
// 하늘에서 아래로 떨어지고, 공격받으면 부서지고, 플레이어에 닿으면 알려 주고, 바닥에 닿으면 사라집니다.
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Obstacle : MonoBehaviour
{
    // 장애물이 플레이어에 닿았을 때 알려 줍니다. (GameManager가 듣고 게임 오버를 처리합니다)
    public static event Action<Obstacle> HitPlayer;

    // 장애물이 공격받아 부서졌을 때 알려 줍니다. (ScoreManager가 듣고 점수를 더합니다)
    // 바닥에 떨어져 사라진 경우에는 알리지 않습니다.
    public static event Action<Obstacle> Broken;

    [Header("모양 (여러 장 넣으면 그중 하나를 랜덤으로 사용)")]
    [SerializeField] private Sprite[] sprites;

    [Tooltip("충돌 범위를 그림보다 얼마나 작게 할지 (1 = 그림과 같은 크기). 작을수록 플레이어가 덜 억울하게 맞습니다.")]
    [Range(0.5f, 1f)]
    [SerializeField] private float colliderScale = 0.8f;

    [Header("기울기")]
    [Tooltip("떨어질 때 왼쪽/오른쪽으로 최대 몇 도까지 랜덤하게 기울일지 (0 = 기울이지 않음)")]
    [Range(0f, 90f)]
    [SerializeField] private float maxTiltAngle = 25f;

    [Header("떨어지는 속도 (1초에 몇 칸)")]
    [SerializeField] private float fallSpeedMin = 4f;
    [SerializeField] private float fallSpeedMax = 6f;

    [Header("소리")]
    [SerializeField] private AudioClip breakSound;

    [Header("부서질 때 조각 효과")]
    [Tooltip("조각 색 (여러 개 넣으면 조각마다 랜덤). 통나무는 갈색, 바위는 회색 계열이 어울립니다.")]
    [SerializeField] private Color[] debrisColors =
    {
        new Color(0.42f, 0.26f, 0.14f),
        new Color(0.62f, 0.42f, 0.24f),
    };
    [Tooltip("조각 개수 (0 = 효과 없음)")]
    [SerializeField] private int debrisCount = 10;
    [Tooltip("조각 하나의 크기 (칸)")]
    [SerializeField] private float debrisSize = 0.12f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D box;
    private float landY = float.NegativeInfinity;

    public bool IsBroken { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        // Kinematic: 중력이나 부딪힘에 밀리지 않고, 정해 준 속도로만 움직입니다.
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Trigger: 플레이어를 밀어내지 않고 겹쳤는지만 감지합니다.
        box.isTrigger = true;

        if (sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
        }
        FitColliderToSprite();

        // -maxTiltAngle ~ +maxTiltAngle 사이의 각도로 기울입니다. (충돌 범위도 같이 기울어집니다)
        float tilt = UnityEngine.Random.Range(-maxTiltAngle, maxTiltAngle);
        transform.rotation = Quaternion.Euler(0f, 0f, tilt);

        rb.linearVelocity = Vector2.down * UnityEngine.Random.Range(fallSpeedMin, fallSpeedMax);
    }

    // 장애물 생성기(ObstacleSpawner)가 바닥 높이를 알려 줍니다.
    public void SetLandY(float y)
    {
        landY = y;
    }

    private void FixedUpdate()
    {
        // 그림의 아래쪽 끝이 바닥에 닿으면 사라집니다.
        if (!IsBroken && spriteRenderer.bounds.min.y <= landY)
        {
            Land();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsBroken) return;

        if (other.GetComponentInParent<PlayerController>() != null)
        {
            HitPlayer?.Invoke(this);
        }
    }

    public void Break()
    {
        Break(1f);
    }

    // debrisMultiplier: 조각을 몇 배 많이, 세게 튀길지 (1 = 보통). 스킬 Rising Crescent에서 크게 씁니다.
    public void Break(float debrisMultiplier)
    {
        if (IsBroken) return;
        IsBroken = true;

        if (breakSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatSound(breakSound);
        }

        int count = Mathf.RoundToInt(debrisCount * debrisMultiplier);
        float speedMultiplier = Mathf.Sqrt(Mathf.Max(1f, debrisMultiplier)); // 3배 → 약 1.7배 빠르게
        DebrisPiece.Burst(spriteRenderer.bounds.center, debrisColors, count, debrisSize,
            spriteRenderer.sortingOrder + 1, speedMultiplier);

        Broken?.Invoke(this);
        Destroy(gameObject);
    }

    // 점수나 콤보 없이 조각 효과만 남기고 사라집니다. (피버 타임이 끝날 때 화면 정리용)
    public void Vanish()
    {
        if (IsBroken) return;
        IsBroken = true;

        DebrisPiece.Burst(spriteRenderer.bounds.center, debrisColors, debrisCount, debrisSize,
            spriteRenderer.sortingOrder + 1);

        Destroy(gameObject);
    }

    private void Land()
    {
        IsBroken = true;
        Destroy(gameObject);
    }

    private void FitColliderToSprite()
    {
        if (spriteRenderer.sprite == null) return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        box.size = (Vector2)spriteBounds.size * colliderScale;
        box.offset = spriteBounds.center;
    }
}

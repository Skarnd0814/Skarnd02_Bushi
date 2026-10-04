using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어의 좌우 이동과 점프를 담당하는 스크립트입니다.
// 키보드(개발 테스트용)와 모바일 버튼(나중에 연결) 입력을 모두 받습니다.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("점프")]
    [SerializeField] private float jumpForce = 12f;

    [Header("바닥 감지")]
    [Tooltip("발밑 몇 칸 아래까지 바닥을 미리 찾아볼지 정합니다.")]
    [SerializeField] private float groundCheckDistance = 0.05f;

    [Header("그림 방향")]
    [Tooltip("원본 스프라이트가 오른쪽을 보고 있으면 체크, 왼쪽을 보고 있으면 체크 해제")]
    [SerializeField] private bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;
    private Camera mainCamera;

    // 바닥을 찾을 때 쓰는 검색 조건과 결과 보관함입니다.
    private ContactFilter2D groundFilter;
    private readonly List<RaycastHit2D> groundHits = new List<RaycastHit2D>();

    private float mobileMoveInput;
    private float lastPressedDirection;
    private bool jumpRequested;

    // 다른 스크립트(애니메이션, 공격)가 읽어 갈 수 있는 현재 상태입니다.
    public float MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public int FacingDirection { get; private set; } = 1; // 오른쪽 1, 왼쪽 -1

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        groundFilter = new ContactFilter2D();
        groundFilter.useTriggers = false; // 눈에 안 보이는 감지 영역(Trigger)은 바닥으로 치지 않습니다.
    }

    // Start: 게임이 시작될 때 한 번 실행됩니다.
    // 첫 화면부터 바닥에 서 있는 상태로 시작하게 미리 확인해 둡니다.
    private void Start()
    {
        IsGrounded = CheckGrounded();
    }

    // Update: 매 화면(프레임)마다 실행됩니다. 키 입력은 여기서 읽습니다.
    private void Update()
    {
        MoveInput = Mathf.Clamp(ReadKeyboardMove() + mobileMoveInput, -1f, 1f);

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (MoveInput != 0f)
        {
            FacingDirection = MoveInput > 0f ? 1 : -1;
            bool facingLeft = FacingDirection < 0;
            spriteRenderer.flipX = spriteFacesRight ? facingLeft : !facingLeft;
        }
    }

    // FixedUpdate: 물리 계산 주기마다 실행됩니다. 실제로 몸을 움직이는 건 여기서 합니다.
    private void FixedUpdate()
    {
        IsGrounded = CheckGrounded();

        Vector2 velocity = rb.linearVelocity;
        velocity.x = MoveInput * moveSpeed;

        if (jumpRequested && IsGrounded)
        {
            velocity.y = jumpForce;
        }
        jumpRequested = false;

        rb.linearVelocity = velocity;
        KeepInsideScreen();
    }

    private float ReadKeyboardMove()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return 0f;

        bool left = keyboard.leftArrowKey.isPressed;
        bool right = keyboard.rightArrowKey.isPressed;

        // 방금 누른 키를 기억합니다.
        if (keyboard.leftArrowKey.wasPressedThisFrame) lastPressedDirection = -1f;
        if (keyboard.rightArrowKey.wasPressedThisFrame) lastPressedDirection = 1f;

        // 두 키를 동시에 누르고 있으면 멈추지 않고, 나중에 누른 쪽으로 갑니다.
        if (left && right) return lastPressedDirection;
        if (left) return -1f;
        if (right) return 1f;
        return 0f;
    }

    // 몸 모양 그대로 발밑으로 아주 조금 내려 보내 보고, 위를 향한 면(바닥)에 걸리면 서 있는 것으로 봅니다.
    // 실제로 닿기 직전에도 알아차리기 때문에 착지하는 순간 바로 바닥 상태로 바뀝니다.
    private bool CheckGrounded()
    {
        // 위로 올라가는 중(점프 직후)에는 바닥에 서 있는 것이 아닙니다.
        if (rb.linearVelocity.y > 0.1f) return false;

        bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance);
        foreach (RaycastHit2D hit in groundHits)
        {
            if (hit.normal.y > 0.5f) return true;
        }
        return false;
    }

    // 플레이어가 화면 왼쪽/오른쪽 끝 밖으로 나가지 못하게 막습니다.
    private void KeepInsideScreen()
    {
        float screenHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float bodyHalfWidth = bodyCollider.bounds.extents.x;
        float minX = mainCamera.transform.position.x - screenHalfWidth + bodyHalfWidth;
        float maxX = mainCamera.transform.position.x + screenHalfWidth - bodyHalfWidth;

        Vector2 position = rb.position;
        if (position.x < minX || position.x > maxX)
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
            rb.position = position;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    // ---------- 모바일 버튼에서 호출할 함수 (나중에 연결) ----------

    // 왼쪽 화살표 버튼: -1, 오른쪽 화살표 버튼: 1, 손을 떼면: 0
    public void SetMobileMove(float direction)
    {
        mobileMoveInput = direction;
    }

    public void RequestJump()
    {
        jumpRequested = true;
    }
}

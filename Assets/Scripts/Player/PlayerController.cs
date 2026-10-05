using System;
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

    // 공격하는 동안 true가 됩니다. 땅에서는 제자리에 멈추고, 점프와 방향 전환을 할 수 없습니다.
    public bool MovementLocked { get; set; }

    // 장애물에 맞아 죽으면 true가 됩니다. 더 이상 조작할 수 없습니다.
    public bool IsDead { get; private set; }

    // 공중에서 점프 버튼을 눌렀을 때 알려 줍니다. (공중 스킬 Rising Crescent에서 사용)
    public event Action AirJumpPressed;

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
        // 죽었거나 일시정지 중(게임 속 시간이 멈춤)이면 입력을 받지 않습니다.
        if (IsDead || Time.timeScale == 0f) return;

        MoveInput =Mathf.Clamp(ReadKeyboardMove() + mobileMoveInput, -1f, 1f);

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (MoveInput != 0f && !MovementLocked)
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
        if (IsDead)
        {
            // 죽은 뒤에는 옆으로 미끄러지지 않고, 공중이었다면 바닥으로 떨어지기만 합니다.
            rb.linearVelocity = new Vector2(0f, velocity.y);
            return;
        }

        if (!MovementLocked)
        {
            velocity.x = MoveInput * moveSpeed;
        }
        else if (IsGrounded)
        {
            velocity.x = 0f; // 땅에서 공격하면 제자리에 멈춥니다. 공중에서는 날아가던 방향 그대로 갑니다.
        }

        if (jumpRequested && IsGrounded && !MovementLocked)
        {
            velocity.y = jumpForce;
        }
        bool airJumpPressed = jumpRequested && !IsGrounded && !MovementLocked;
        jumpRequested = false;

        rb.linearVelocity = velocity;
        KeepInsideScreen();

        // 속도를 다 정한 다음에 알려야, 스킬이 바꾼 속도(한 번 더 도약)가 덮어써지지 않습니다.
        if (airJumpPressed) AirJumpPressed?.Invoke();
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

    // GameManager가 게임 오버 때 호출합니다.
    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        MoveInput = 0f;
        mobileMoveInput = 0f;
        jumpRequested = false;

        // 위로 솟구치던 중이었다면 멈추고 바로 떨어지게 합니다.
        rb.linearVelocity = new Vector2(0f, Mathf.Min(rb.linearVelocity.y, 0f));
    }

    // 스킬이 호출합니다. 지금 위치에서 위로 한 번 더 뛰어오릅니다. (공중에서도 가능)
    public void Leap(float upwardSpeed)
    {
        if (IsDead) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, upwardSpeed);
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

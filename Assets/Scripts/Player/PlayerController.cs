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

    [Header("그림 방향")]
    [Tooltip("원본 스프라이트가 오른쪽을 보고 있으면 체크, 왼쪽을 보고 있으면 체크 해제")]
    [SerializeField] private bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D bodyCollider;
    private Camera mainCamera;

    // 바닥에 닿아 있는지 확인할 때 쓰는 접촉 정보 보관함입니다.
    private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];

    private float mobileMoveInput;
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

        float move = 0f;
        if (keyboard.leftArrowKey.isPressed) move -= 1f;
        if (keyboard.rightArrowKey.isPressed) move += 1f;
        return move;
    }

    // 발밑(위쪽을 향한 면)에 무언가 닿아 있으면 바닥에 서 있는 것으로 봅니다.
    private bool CheckGrounded()
    {
        int count = rb.GetContacts(contacts);
        for (int i = 0; i < count; i++)
        {
            if (contacts[i].normal.y > 0.5f) return true;
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

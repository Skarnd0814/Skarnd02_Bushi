using UnityEngine;
using UnityEngine.UI;

// 모바일 조작 버튼 4개를 모아서 플레이어에게 전달하는 스크립트입니다.
// 키보드 조작과 함께 써도 문제없습니다.
public class MobileControls : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerAttack playerAttack;

    [Header("공격 버튼 쿨타임 표시")]
    [Tooltip("쿨타임 동안 어둡게 만들 공격 버튼 그림 (비워 두면 표시하지 않습니다)")]
    [SerializeField] private Image attackButtonImage;
    [SerializeField] private Color cooldownColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("보이기 설정")]
    [Tooltip("체크하면 휴대폰에서만 버튼이 보이고, PC(에디터)에서는 숨겨집니다.")]
    [SerializeField] private bool showOnlyOnMobile = false;

    private bool leftHeld;
    private bool rightHeld;
    private float lastPressedDirection;

    private void Awake()
    {
        if (showOnlyOnMobile && !Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (attackButtonImage == null) return;

        // 쿨타임이 돌고 있으면 공격 버튼을 어둡게 보여 줍니다.
        bool onCooldown = !playerAttack.CooldownDisabled && playerAttack.CooldownRemaining > 0f;
        attackButtonImage.color = onCooldown ? cooldownColor : Color.white;
    }

    public void Press(MobileControlButton.ButtonType type)
    {
        switch (type)
        {
            case MobileControlButton.ButtonType.Left:
                leftHeld = true;
                lastPressedDirection = -1f;
                ApplyMove();
                break;
            case MobileControlButton.ButtonType.Right:
                rightHeld = true;
                lastPressedDirection = 1f;
                ApplyMove();
                break;
            case MobileControlButton.ButtonType.Jump:
                player.RequestJump();
                break;
            case MobileControlButton.ButtonType.Attack:
                playerAttack.RequestAttack();
                break;
        }
    }

    public void Release(MobileControlButton.ButtonType type)
    {
        switch (type)
        {
            case MobileControlButton.ButtonType.Left:
                leftHeld = false;
                ApplyMove();
                break;
            case MobileControlButton.ButtonType.Right:
                rightHeld = false;
                ApplyMove();
                break;
        }
    }

    // ← → 를 동시에 누르고 있으면 키보드처럼 나중에 누른 쪽으로 갑니다.
    private void ApplyMove()
    {
        float direction;
        if (leftHeld && rightHeld) direction = lastPressedDirection;
        else if (leftHeld) direction = -1f;
        else if (rightHeld) direction = 1f;
        else direction = 0f;

        player.SetMobileMove(direction);
    }
}

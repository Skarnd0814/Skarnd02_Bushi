using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

// 모바일 조작 버튼(← → 점프 공격 + 스킬 버튼)을 모아서 플레이어에게 전달하는 스크립트입니다.
// 매 순간 "화면에 닿아 있는 손가락들이 어느 버튼 위에 있는지"를 직접 확인합니다.
// 그래서 손가락을 떼지 않고 ← 에서 → 로 미끄러뜨려도 바로 방향이 바뀝니다.
// 키보드 조작과 함께 써도 문제없습니다.
// 스킬 버튼은 그 스킬을 구매(잠금 해제)했을 때만 화면에 보입니다.
public class MobileControls : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerAttack playerAttack;
    [Tooltip("스킬 버튼을 쓸 때 연결합니다 (Player 오브젝트)")]
    [SerializeField] private PlayerSkills playerSkills;

    [Header("터치 인식")]
    [Tooltip("버튼보다 얼마나 넓게 터치를 인식할지 (Canvas 기준 크기). 버튼 사이 틈을 메워 줍니다.")]
    [SerializeField] private float touchPadding = 15f;

    [Header("공격 버튼 쿨타임 표시")]
    [Tooltip("쿨타임 동안 어둡게 만들 공격 버튼 그림 (비워 두면 표시하지 않습니다)")]
    [SerializeField] private Image attackButtonImage;
    [SerializeField] private Color cooldownColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("보이기 설정")]
    [Tooltip("체크하면 휴대폰에서만 버튼이 보이고, PC(에디터)에서는 숨겨집니다.")]
    [SerializeField] private bool showOnlyOnMobile = false;

    private MobileControlButton[] buttons;
    private readonly List<Vector2> pointerPositions = new List<Vector2>();
    private float lastPressedDirection;

    private void Awake()
    {
        buttons = GetComponentsInChildren<MobileControlButton>(true);

        if (showOnlyOnMobile && !Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }

    // 버튼이 꺼지면(게임 오버 등) 누르고 있던 상태를 모두 풀어 줍니다.
    private void OnDisable()
    {
        if (buttons == null) return;

        foreach (MobileControlButton button in buttons) button.SetPressed(false);
        player.SetMobileMove(0f);
    }

    private void Update()
    {
        CollectPointerPositions();

        // 일시정지 중(게임 속 시간이 멈춤)에는 버튼을 누르지 않은 것으로 봅니다.
        bool canControl = Time.timeScale > 0f;

        UpdateSkillButtonVisibility();

        foreach (MobileControlButton button in buttons)
        {
            // 숨겨진 버튼(구매하지 않은 스킬)은 눌리지 않습니다.
            if (!button.gameObject.activeInHierarchy)
            {
                button.SetPressed(false);
                continue;
            }

            bool held = canControl && IsAnyPointerInside(button);
            if (held == button.IsPressed) continue;

            button.SetPressed(held);
            if (held) OnPressStarted(button.Type); // 방금 손가락이 들어온 순간
        }

        ApplyMove();
        UpdateAttackButtonColor();
    }

    // 지금 화면에 닿아 있는 모든 손가락(그리고 PC 테스트용 마우스)의 위치를 모읍니다.
    private void CollectPointerPositions()
    {
        pointerPositions.Clear();

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.isPressed) pointerPositions.Add(touch.position.ReadValue());
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            pointerPositions.Add(mouse.position.ReadValue());
        }
    }

    private bool IsAnyPointerInside(MobileControlButton button)
    {
        foreach (Vector2 position in pointerPositions)
        {
            if (button.ContainsScreenPoint(position, touchPadding)) return true;
        }
        return false;
    }

    private void OnPressStarted(MobileControlButton.ButtonType type)
    {
        switch (type)
        {
            case MobileControlButton.ButtonType.Left:
                lastPressedDirection = -1f;
                break;
            case MobileControlButton.ButtonType.Right:
                lastPressedDirection = 1f;
                break;
            case MobileControlButton.ButtonType.Jump:
                player.RequestJump();
                break;
            case MobileControlButton.ButtonType.Attack:
                playerAttack.RequestAttack();
                break;
            default:
                int skillNumber = GetSkillNumber(type);
                if (skillNumber > 0 && playerSkills != null) playerSkills.RequestSkill(skillNumber);
                break;
        }
    }

    // 스킬 버튼 종류를 스킬 번호로 바꿉니다. (스킬 버튼이 아니면 0)
    private static int GetSkillNumber(MobileControlButton.ButtonType type)
    {
        switch (type)
        {
            case MobileControlButton.ButtonType.Skill1: return 1;
            case MobileControlButton.ButtonType.Skill2: return 2;
            case MobileControlButton.ButtonType.Skill3: return 3;
            default: return 0;
        }
    }

    // 구매한(또는 PC 테스트로 열린) 스킬의 버튼만 보이게 합니다.
    private void UpdateSkillButtonVisibility()
    {
        foreach (MobileControlButton button in buttons)
        {
            int skillNumber = GetSkillNumber(button.Type);
            if (skillNumber == 0) continue;

            PlayerSkill skill = playerSkills != null ? playerSkills.GetSkill(skillNumber) : null;
            bool visible = skill != null && skill.IsAvailable;
            if (button.gameObject.activeSelf != visible) button.gameObject.SetActive(visible);
        }
    }

    // ← → 를 동시에 누르고 있으면 키보드처럼 나중에 누른 쪽으로 갑니다.
    private void ApplyMove()
    {
        bool leftHeld = false;
        bool rightHeld = false;
        foreach (MobileControlButton button in buttons)
        {
            if (!button.IsPressed) continue;
            if (button.Type == MobileControlButton.ButtonType.Left) leftHeld = true;
            if (button.Type == MobileControlButton.ButtonType.Right) rightHeld = true;
        }

        float direction;
        if (leftHeld && rightHeld) direction = lastPressedDirection;
        else if (leftHeld) direction = -1f;
        else if (rightHeld) direction = 1f;
        else direction = 0f;

        player.SetMobileMove(direction);
    }

    // 쿨타임이 돌고 있으면 공격 버튼을 어둡게 보여 줍니다.
    private void UpdateAttackButtonColor()
    {
        if (attackButtonImage == null) return;

        bool onCooldown = !playerAttack.CooldownDisabled && playerAttack.CooldownRemaining > 0f;
        attackButtonImage.color = onCooldown ? cooldownColor : Color.white;
    }
}

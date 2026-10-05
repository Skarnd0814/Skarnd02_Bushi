using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Player에 붙은 스킬들(CycloneSlashSkill, TripleComboSkill, RisingCrescentSkill)을 한곳에서 관리하는 스크립트입니다.
// - PC 테스트용 키보드 입력: 1, 2, 3 (키패드 숫자도 됨)
// - 모바일 스킬 버튼에서 RequestSkill(번호)를 부릅니다.
// - 한 번에 하나의 스킬만, 공격 중이 아닐 때만 쓸 수 있게 막아 줍니다.
[RequireComponent(typeof(PlayerController))]
public class PlayerSkills : MonoBehaviour
{
    [Header("테스트")]
    [Tooltip("체크하면 PC(유니티 에디터 포함)에서는 구매하지 않아도 모든 스킬을 쓸 수 있습니다.\n휴대폰에서는 이 칸과 상관없이 항상 구매한 스킬만 쓸 수 있습니다.")]
    [SerializeField] private bool unlockAllOnPC = true;

    private PlayerController controller;
    private PlayerAttack attack;
    private PlayerSkill[] skills;

    // 지금 사용 중인 스킬 (없으면 null)
    public PlayerSkill ActiveSkill { get; private set; }
    public bool IsUsingSkill => ActiveSkill != null;

    public bool UnlockAllForTesting => unlockAllOnPC && !Application.isMobilePlatform;

    // 스킬이 시작되거나 끝났을 때 알려 줍니다. (애니메이션 등에서 사용)
    public event Action<PlayerSkill> SkillStarted;
    public event Action<PlayerSkill> SkillEnded;

    // 새 스킬을 시작해도 되는 상태인지 확인합니다.
    public bool CanStartSkill
    {
        get
        {
            if (controller.IsDead || Time.timeScale == 0f) return false;
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
            if (ActiveSkill != null) return false;
            if (attack != null && attack.IsAttacking) return false;
            return true;
        }
    }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttack>();
        skills = GetComponents<PlayerSkill>();
    }

    private void Update()
    {
        // 죽으면 쓰고 있던 스킬을 바로 멈춥니다.
        if (controller.IsDead)
        {
            if (ActiveSkill != null) ActiveSkill.Cancel();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) RequestSkill(1);
        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) RequestSkill(2);
        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) RequestSkill(3);
    }

    // 번호에 맞는 스킬을 찾아 줍니다. (없으면 null)
    public PlayerSkill GetSkill(int skillNumber)
    {
        foreach (PlayerSkill skill in skills)
        {
            if (skill.SkillNumber == skillNumber) return skill;
        }
        return null;
    }

    // ---------- 키보드 / 모바일 버튼에서 호출 ----------

    public bool RequestSkill(int skillNumber)
    {
        PlayerSkill skill = GetSkill(skillNumber);
        return skill != null && skill.TryActivate();
    }

    // ---------- 스킬 스크립트(PlayerSkill)가 호출 ----------

    internal void NotifySkillStarted(PlayerSkill skill)
    {
        ActiveSkill = skill;
        SkillStarted?.Invoke(skill);
    }

    internal void NotifySkillEnded(PlayerSkill skill)
    {
        if (ActiveSkill == skill) ActiveSkill = null;
        SkillEnded?.Invoke(skill);
    }
}

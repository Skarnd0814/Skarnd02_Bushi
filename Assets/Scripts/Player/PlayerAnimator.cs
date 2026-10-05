using UnityEngine;

// PlayerController / PlayerAttack의 상태를 Animator에 전달해서
// IDLE / RUN / JUMP / ATTACK 애니메이션이 자동으로 바뀌게 하는 스크립트입니다.
// 스킬은 화살표(전환 조건) 없이, 스킬이 시작되는 순간 그 스킬의 상태를 직접 재생합니다.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    // Animator 창에서 만든 Parameter 이름과 글자가 정확히 같아야 합니다.
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    [Tooltip("키를 바꿔 누르는 아주 짧은 순간에 IDLE이 끼어들지 않도록 기다려 주는 시간(초)")]
    [SerializeField] private float stopGraceTime = 0.08f;

    [Tooltip("스킬이 끝나면 돌아갈 상태 이름 (Animator 창의 IDLE 상태 이름과 같아야 합니다)")]
    [SerializeField] private string idleStateName = "Player_Idle";

    private Animator animator;
    private PlayerController controller;
    private PlayerAttack attack;
    private PlayerSkills skills;
    private PlayerSkill playingSkill;
    private float lastMovingTime = float.NegativeInfinity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttack>();
        skills = GetComponent<PlayerSkills>();
    }

    private void Update()
    {
        if (controller.IsDead)
        {
            // 죽으면 사망 모션만 나오게 합니다.
            // 다른 신호는 "바닥에 가만히 서 있음"으로 고정해서, 점프/공격 화살표가 사망 모션을 끊지 못하게 합니다.
            animator.SetBool(IsDeadHash, true);
            animator.SetFloat(SpeedHash, 0f);
            animator.SetBool(IsGroundedHash, true);
            animator.SetBool(IsAttackingHash, false);
            playingSkill = null;
            return;
        }

        if (UpdateSkillAnimation()) return;

        float speed = Mathf.Abs(controller.MoveInput);
        if (speed > 0f)
        {
            lastMovingTime = Time.time;
        }
        else if (Time.time - lastMovingTime < stopGraceTime)
        {
            // 방금 전까지 달리고 있었다면 잠깐 동안은 계속 달리는 것으로 알려 줍니다.
            speed = 1f;
        }

        animator.SetFloat(SpeedHash, speed);
        animator.SetBool(IsGroundedHash, controller.IsGrounded);
        animator.SetBool(IsAttackingHash, attack != null && attack.IsAttacking);
    }

    // 스킬을 쓰는 중이면 스킬 애니메이션을 보여 주고 true를 돌려줍니다.
    private bool UpdateSkillAnimation()
    {
        PlayerSkill activeSkill = skills != null ? skills.ActiveSkill : null;

        // 스킬이 막 시작됐거나 막 끝난 순간에만 상태를 바꿉니다.
        if (activeSkill != playingSkill)
        {
            playingSkill = activeSkill;
            PlayState(activeSkill != null ? activeSkill.AnimationStateName : idleStateName);
        }

        if (activeSkill == null) return false;

        // 스킬을 쓰는 동안에는 Any State 화살표(점프, 공격)가 스킬 모션을 끊지 않도록
        // "바닥에 가만히 서 있음, 공격 안 함"으로 알려 둡니다. (공중 스킬도 끝까지 재생됩니다)
        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool(IsGroundedHash, true);
        animator.SetBool(IsAttackingHash, false);
        return true;
    }

    private void PlayState(string stateName)
    {
        if (animator.HasState(0, Animator.StringToHash(stateName)))
        {
            animator.Play(stateName, 0, 0f);
        }
        else
        {
            Debug.LogWarning($"Animator에 '{stateName}' 상태가 없습니다. Animator 창의 상태 이름을 확인하세요.");
        }
    }
}

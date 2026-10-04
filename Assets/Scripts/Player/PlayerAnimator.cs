using UnityEngine;

// PlayerController / PlayerAttack의 상태를 Animator에 전달해서
// IDLE / RUN / JUMP / ATTACK 애니메이션이 자동으로 바뀌게 하는 스크립트입니다.
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

    private Animator animator;
    private PlayerController controller;
    private PlayerAttack attack;
    private float lastMovingTime = float.NegativeInfinity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttack>();
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
            return;
        }

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
}

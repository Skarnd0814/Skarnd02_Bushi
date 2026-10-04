using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 플레이어의 공격(Q키 / 모바일 공격 버튼)을 담당하는 스크립트입니다.
// 공격 범위 안의 장애물을 부수고, 성공/실패에 따라 쿨타임을 다르게 적용합니다.
[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("공격 시간 (초) - 공격 애니메이션 길이에 맞춥니다")]
    [Tooltip("공격 모션 전체 길이")]
    [SerializeField] private float attackDuration = 0.58f;
    [Tooltip("공격을 시작하고 몇 초 뒤부터 장애물을 부술 수 있는지 (칼을 휘두르기 시작하는 순간)")]
    [SerializeField] private float hitStartTime = 0.25f;
    [Tooltip("공격을 시작하고 몇 초 뒤까지 장애물을 부술 수 있는지 (칼을 다 휘두른 순간)")]
    [SerializeField] private float hitEndTime = 0.5f;

    [Header("쿨타임 (초)")]
    [SerializeField] private float successCooldown = 0.5f;
    [SerializeField] private float failCooldown = 3f;

    [Header("공격 범위 (오른쪽을 볼 때 기준, 발밑으로부터의 거리)")]
    [SerializeField] private Vector2 hitBoxOffset = new Vector2(0.6f, 1.3f);
    [SerializeField] private Vector2 hitBoxSize = new Vector2(1.4f, 1.8f);

    [Header("소리 (비워 두면 재생하지 않습니다)")]
    [Tooltip("공격 버튼을 누르는 순간 (칼 휘두르는 소리)")]
    [SerializeField] private AudioClip attackSound;
    [Tooltip("공격 성공: 이번 공격에서 첫 장애물을 부순 순간")]
    [SerializeField] private AudioClip successSound;
    [Tooltip("공격 실패: 판정이 끝났는데 아무것도 못 부쉈을 때")]
    [SerializeField] private AudioClip failSound;

    private PlayerController controller;
    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private bool attackRequested;
    private float attackStartTime;
    private bool isJudged;
    private int destroyedCount;
    private float cooldownEndTime;

    public bool IsAttacking { get; private set; }

    // 피버 타임 동안 true로 바꾸면 쿨타임 없이 공격할 수 있습니다. (STEP 11에서 사용)
    public bool CooldownDisabled { get; set; }

    // 쿨타임 표시 UI에서 사용할 값입니다. (STEP 12에서 사용)
    public float CooldownDuration { get; private set; }
    public float CooldownRemaining => Mathf.Max(0f, cooldownEndTime - Time.time);

    public bool CanAttack => !IsAttacking && (CooldownDisabled || Time.time >= cooldownEndTime);

    // 공격 한 번의 결과가 정해지면 부순 장애물 개수를 알려 줍니다. (점수, 피버 타임에서 사용)
    public event Action<int> AttackJudged;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        hitFilter = new ContactFilter2D();
        hitFilter.useTriggers = true; // 장애물의 감지 영역(Trigger)도 찾을 수 있게 합니다.
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.qKey.wasPressedThisFrame)
        {
            attackRequested = true;
        }

        if (attackRequested)
        {
            attackRequested = false;
            if (CanAttack) StartAttack();
        }

        if (IsAttacking && Time.time - attackStartTime >= attackDuration)
        {
            EndAttack();
        }
    }

    // 장애물을 찾는 일은 물리 계산 주기에 맞춰 합니다.
    private void FixedUpdate()
    {
        if (!IsAttacking || isJudged) return;

        float elapsed = Time.time - attackStartTime;
        if (elapsed >= hitStartTime)
        {
            HitObstaclesInRange();
        }
        if (elapsed >= hitEndTime)
        {
            Judge();
        }
    }

    private void StartAttack()
    {
        IsAttacking = true;
        isJudged = false;
        destroyedCount = 0;
        attackStartTime = Time.time;
        controller.MovementLocked = true;

        PlaySound(attackSound);
    }

    private void EndAttack()
    {
        if (!isJudged) Judge();

        IsAttacking = false;
        controller.MovementLocked = false;
    }

    private void HitObstaclesInRange()
    {
        Physics2D.OverlapBox(GetHitBoxCenter(), hitBoxSize, 0f, hitFilter, hitResults);
        foreach (Collider2D hit in hitResults)
        {
            Obstacle obstacle = hit.GetComponentInParent<Obstacle>();
            if (obstacle != null && !obstacle.IsBroken)
            {
                obstacle.Break();
                destroyedCount++;

                // 성공 소리는 칼에 맞은 순간 바로, 한 번의 공격에 한 번만 재생합니다.
                if (destroyedCount == 1) PlaySound(successSound);
            }
        }
    }

    // 장애물을 하나라도 부쉈으면 성공(짧은 쿨타임), 하나도 못 부쉈으면 실패(긴 쿨타임)입니다.
    private void Judge()
    {
        isJudged = true;
        bool success = destroyedCount > 0;

        CooldownDuration = success ? successCooldown : failCooldown;
        cooldownEndTime = Time.time + CooldownDuration;

        if (!success) PlaySound(failSound);

        Debug.Log(success
            ? $"공격 성공! 장애물 {destroyedCount}개 파괴, 쿨타임 {CooldownDuration}초"
            : $"공격 실패... 쿨타임 {CooldownDuration}초");

        AttackJudged?.Invoke(destroyedCount);
    }

    // 공격/파괴 소리는 설정창의 EFFECT 볼륨을 따르는 스피커로 재생합니다.
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatSound(clip);
        }
    }

    private Vector2 GetHitBoxCenter()
    {
        int facing = controller != null ? controller.FacingDirection : 1;
        return (Vector2)transform.position + new Vector2(hitBoxOffset.x * facing, hitBoxOffset.y);
    }

    // ---------- 모바일 버튼에서 호출할 함수 (나중에 연결) ----------

    public void RequestAttack()
    {
        attackRequested = true;
    }

    // Scene 창에서 Player를 선택하면 공격 범위를 빨간 사각형으로 보여 줍니다.
    private void OnDrawGizmosSelected()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetHitBoxCenter(), hitBoxSize);
    }
}

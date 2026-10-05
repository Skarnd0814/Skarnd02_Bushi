using System.Collections;
using UnityEngine;

// 스킬 3. Rising Crescent
// 점프하는 도중에 점프 버튼을 한 번 더 누르면 발동합니다. (PC 테스트: 숫자 3)
// 캐릭터가 한 번 더 뛰어오르며, 바라보는 방향으로 커다란 초승달 검기를 날립니다.
// 이 검기에 맞은 장애물은 조각이 훨씬 많이, 세게 튑니다.
public class RisingCrescentSkill : PlayerSkill
{
    [Header("도약")]
    [Tooltip("한 번 더 뛰어오르는 힘 (보통 점프 힘은 12)")]
    [SerializeField] private float leapForce = 11f;

    [Header("검기 발사 시간 (스킬을 시작하고 몇 초 뒤) - 칼을 휘두르는 순간에 맞춥니다")]
    [SerializeField] private float fireTime = 0.2f;

    [Header("검기")]
    [SerializeField] private SkillProjectileSettings projectile = new SkillProjectileSettings
    {
        flipSprite = true, // 초승달 그림이 왼쪽을 향해 그려져 있습니다.
        framesPerSecond = 12f,
        speed = 16f,
        scale = 1f,
        hitBoxSize = new Vector2(1f, 3f),
        spawnOffset = new Vector2(0.6f, 0.8f),
        debrisMultiplier = 3f,
    };

    // 한 번 점프하는 동안 한 번만 쓸 수 있습니다. 땅에 닿으면 다시 쓸 수 있습니다.
    private bool usedSinceLanding;

    public override bool CanActivate => base.CanActivate && !usedSinceLanding;

    // Add Component로 처음 붙일 때 이 스킬에 맞는 기본값을 넣어 줍니다.
    private void Reset()
    {
        skillNumber = 3;
        displayName = "Rising Crescent";
        animationStateName = "Player_Skill3";
        duration = 0.55f;
        cooldown = 6f;
    }

    // 공중에서 점프 버튼을 눌렀다는 알림을 듣기 시작하고, 그만 듣습니다.
    private void OnEnable()
    {
        if (Controller != null) Controller.AirJumpPressed += OnAirJumpPressed;
    }

    private void OnDisable()
    {
        if (Controller != null) Controller.AirJumpPressed -= OnAirJumpPressed;
    }

    private void Update()
    {
        if (Controller.IsGrounded && !IsActive) usedSinceLanding = false;
    }

    private void OnAirJumpPressed()
    {
        TryActivate();
    }

    protected override IEnumerator Perform()
    {
        usedSinceLanding = true;
        Controller.Leap(leapForce);

        yield return new WaitUntil(() => Elapsed >= fireTime);
        SkillProjectile.Spawn(projectile, transform, Facing, 0f, EffectSortingOrder);

        // 모션이 끝나기 전에 땅에 내려오면 바로 스킬을 끝내서 땅에서 멈춰 있지 않게 합니다.
        yield return new WaitUntil(() => Elapsed >= duration || Controller.IsGrounded);
    }
}

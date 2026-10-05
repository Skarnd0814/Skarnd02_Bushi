using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스킬 1. Cyclone Slash
// 캐릭터가 빠르게 회전하며 주변을 벱니다.
// 회전하는 동안 머리를 중심으로 한 정사각형 범위 안의 장애물을 모두 부수고, 발밑에 흙먼지가 일어납니다.
public class CycloneSlashSkill : PlayerSkill
{
    [Header("공격 판정 시간 (스킬을 시작하고 몇 초 뒤)")]
    [Tooltip("회전을 시작하는 순간 (이때부터 장애물을 부술 수 있음)")]
    [SerializeField] private float hitStartTime = 0.27f;
    [Tooltip("회전이 끝나는 순간 (이때까지 장애물을 부술 수 있음)")]
    [SerializeField] private float hitEndTime = 0.57f;

    [Header("공격 범위 (머리를 중심으로 한 정사각형)")]
    [Tooltip("캐릭터의 키 (칸). 새 캐릭터는 약 1.44칸입니다.")]
    [SerializeField] private float characterHeight = 1.44f;
    [Tooltip("정사각형 한 변 = 캐릭터 키 × 이 값")]
    [Range(2f, 3f)]
    [SerializeField] private float sizeMultiplier = 2.5f;
    [Tooltip("머리 위치 (발밑으로부터의 거리)")]
    [SerializeField] private Vector2 headOffset = new Vector2(0f, 1.2f);

    [Header("흙먼지 이펙트")]
    [Tooltip("흙먼지 그림들 (Bushi_skill1_effect (1)_0 ~ _6)")]
    [SerializeField] private Sprite[] dustFrames;
    [Tooltip("1초에 몇 장씩 넘길지")]
    [SerializeField] private float dustFramesPerSecond = 14f;
    [Tooltip("흙먼지 크기 배율")]
    [SerializeField] private float dustScale = 1f;
    [Tooltip("흙먼지 위치 (발밑으로부터의 거리)")]
    [SerializeField] private Vector2 dustOffset = new Vector2(0f, 0.1f);

    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    // Add Component로 처음 붙일 때 이 스킬에 맞는 기본값을 넣어 줍니다.
    private void Reset()
    {
        skillNumber = 1;
        displayName = "Cyclone Slash";
        animationStateName = "Player_Skill1";
        duration = 0.63f;
        cooldown = 8f;
    }

    protected override void Awake()
    {
        base.Awake();
        hitFilter = new ContactFilter2D();
        hitFilter.useTriggers = true; // 장애물의 감지 영역(Trigger)도 찾을 수 있게 합니다.
    }

    protected override IEnumerator Perform()
    {
        yield return new WaitUntil(() => Elapsed >= hitStartTime);

        // 회전을 시작하는 순간 발밑에 흙먼지를 일으킵니다. (캐릭터를 따라다닙니다)
        SpriteFlipbook dust = SpriteFlipbook.Spawn("CycloneDust", dustFrames, dustFramesPerSecond, false,
            transform.position + (Vector3)dustOffset, dustScale, EffectSortingOrder);
        if (dust != null) dust.Follow(transform, dustOffset);

        // 회전하는 동안 물리 계산 주기마다 범위 안의 장애물을 부숩니다.
        while (Elapsed < hitEndTime)
        {
            HitObstaclesInRange();
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitUntil(() => Elapsed >= duration);
    }

    private void HitObstaclesInRange()
    {
        Physics2D.OverlapBox(GetAreaCenter(), GetAreaSize(), 0f, hitFilter, hitResults);
        foreach (Collider2D hit in hitResults)
        {
            Obstacle obstacle = hit.GetComponentInParent<Obstacle>();
            if (obstacle != null && !obstacle.IsBroken)
            {
                obstacle.Break();
            }
        }
    }

    private Vector2 GetAreaCenter()
    {
        return (Vector2)transform.position + headOffset;
    }

    private Vector2 GetAreaSize()
    {
        float side = characterHeight * sizeMultiplier;
        return new Vector2(side, side);
    }

    // Scene 창에서 Player를 선택하면 공격 범위를 주황색 사각형으로 보여 줍니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(GetAreaCenter(), GetAreaSize());
    }
}

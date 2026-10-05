using System.Collections;
using UnityEngine;

// 스킬 2. Triple Combo
// 캐릭터가 칼을 3번 연속으로 휘두르고, 휘두를 때마다 바라보는 방향으로 검기를 하나씩 날립니다.
// 검기는 화면 밖으로 나갈 때까지 사라지지 않고, 닿는 장애물을 모두 부숩니다. (점수, 콤보 O)
public class TripleComboSkill : PlayerSkill
{
    [Header("검기 발사 시간 (스킬을 시작하고 몇 초 뒤) - 칼을 휘두르는 순간에 맞춥니다")]
    [SerializeField] private float[] fireTimes = { 0.12f, 0.24f, 0.44f };

    [Header("검기 방향 (바라보는 방향 기준, 수평에서 위쪽으로 몇 도)")]
    [Tooltip("발사 시간과 같은 순서입니다. 0 = 수평, 90 = 바로 위. 칸이 모자라면 마지막 값을 씁니다.")]
    [SerializeField] private float[] shotAngles = { 10f, 25f, 40f };

    [Header("검기")]
    [SerializeField] private SkillProjectileSettings projectile = new SkillProjectileSettings
    {
        framesPerSecond = 12f,
        speed = 12f,
        scale = 1.2f,
        hitBoxSize = new Vector2(0.8f, 1.2f),
        spawnOffset = new Vector2(0.5f, 0.9f),
        debrisMultiplier = 1f,
    };

    // Add Component로 처음 붙일 때 이 스킬에 맞는 기본값을 넣어 줍니다.
    private void Reset()
    {
        skillNumber = 2;
        displayName = "Triple Combo";
        animationStateName = "Player_Skill2";
        duration = 0.8f;
        cooldown = 10f;
    }

    protected override IEnumerator Perform()
    {
        for (int i = 0; i < fireTimes.Length; i++)
        {
            float fireTime = fireTimes[i];
            yield return new WaitUntil(() => Elapsed >= fireTime);

            SkillProjectile.Spawn(projectile, transform, Facing, GetAngle(i), EffectSortingOrder);
        }

        yield return new WaitUntil(() => Elapsed >= duration);
    }

    private float GetAngle(int index)
    {
        if (shotAngles == null || shotAngles.Length == 0) return 0f;
        return shotAngles[Mathf.Min(index, shotAngles.Length - 1)];
    }
}

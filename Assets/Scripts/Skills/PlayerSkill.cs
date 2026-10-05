using System.Collections;
using UnityEngine;

// 모든 스킬이 공통으로 가지는 기능을 모아 둔 "스킬의 기본 틀"입니다.
// - 스킬 번호, 이름, 아이콘, 애니메이션 이름, 길이, 쿨타임, 효과음 칸
// - 구매(잠금 해제) 여부 확인, 쿨타임 계산, 사용 중 이동 잠금
// 이 스크립트는 직접 붙이지 않고, CycloneSlashSkill 같은 실제 스킬 스크립트를 Player에 붙입니다.
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerSkills))]
public abstract class PlayerSkill : MonoBehaviour
{
    [Header("기본 정보")]
    [Tooltip("스킬 번호 (1~3). 키보드 숫자키, 모바일 버튼, 구매 기록 저장에 쓰입니다. 스킬끼리 겹치면 안 됩니다.")]
    [SerializeField] protected int skillNumber = 1;
    [SerializeField] protected string displayName = "Skill";
    [Tooltip("스킬 버튼에 보여 줄 아이콘")]
    [SerializeField] protected Sprite icon;
    [Tooltip("Animator 창에 만든 이 스킬 상태(네모 칸)의 이름. 글자가 정확히 같아야 합니다.")]
    [SerializeField] protected string animationStateName = "Player_Skill1";

    [Header("시간 (초)")]
    [Tooltip("스킬 모션 전체 길이 - 스킬 애니메이션 길이에 맞춥니다")]
    [SerializeField] protected float duration = 0.6f;
    [Tooltip("스킬을 쓴 뒤 다시 쓸 수 있을 때까지 기다리는 시간")]
    [SerializeField] protected float cooldown = 8f;

    [Header("소리 (비워 두면 재생하지 않습니다)")]
    [Tooltip("스킬을 쓰는 순간 재생할 효과음 (설정창의 EFFECT 볼륨을 따릅니다)")]
    [SerializeField] protected AudioClip skillSound;

    protected PlayerController Controller { get; private set; }
    protected PlayerSkills Owner { get; private set; }

    private float startTime;
    private float cooldownEndTime;

    public int SkillNumber => skillNumber;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string AnimationStateName => animationStateName;

    public bool IsActive { get; private set; }

    // 쿨타임 표시 UI(스킬 버튼)에서 사용할 값입니다.
    public float CooldownDuration => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, cooldownEndTime - Time.time);

    // 구매한 스킬이거나, PC 테스트 설정이 켜져 있으면 쓸 수 있는 스킬입니다.
    public bool IsAvailable => SkillUnlocks.IsUnlocked(skillNumber) || (Owner != null && Owner.UnlockAllForTesting);

    // 지금 바로 쓸 수 있는지: 구매함 + 쿨타임 끝 + 다른 행동(공격, 다른 스킬) 중이 아님
    public virtual bool CanActivate => IsAvailable && CooldownRemaining <= 0f && Owner.CanStartSkill;

    // 스킬을 시작하고 몇 초가 지났는지 (일시정지 중에는 멈춰 있습니다)
    protected float Elapsed => Time.time - startTime;

    // 플레이어가 바라보는 방향 (오른쪽 1, 왼쪽 -1)
    protected int Facing => Controller.FacingDirection;

    protected virtual void Awake()
    {
        Controller = GetComponent<PlayerController>();
        Owner = GetComponent<PlayerSkills>();
    }

    public bool TryActivate()
    {
        if (!CanActivate) return false;

        StartCoroutine(Run());
        return true;
    }

    // 죽었을 때처럼 스킬을 중간에 강제로 끝낼 때 사용합니다.
    public void Cancel()
    {
        if (!IsActive) return;

        StopAllCoroutines();
        Finish();
    }

    private IEnumerator Run()
    {
        IsActive = true;
        startTime = Time.time;
        cooldownEndTime = Time.time + cooldown; // 쿨타임은 스킬을 쓰는 순간부터 돕니다.
        Controller.MovementLocked = true;       // 스킬을 쓰는 동안 방향 전환과 점프를 막습니다.
        Owner.NotifySkillStarted(this);

        PlaySound(skillSound);

        yield return Perform();

        Finish();
    }

    private void Finish()
    {
        IsActive = false;
        Controller.MovementLocked = false;
        OnFinished();
        Owner.NotifySkillEnded(this);
    }

    // 각 스킬이 실제로 하는 일을 여기에 적습니다. (시간 순서대로)
    protected abstract IEnumerator Perform();

    // 스킬이 끝나거나 취소될 때 정리할 것이 있으면 여기에 적습니다.
    protected virtual void OnFinished() { }

    // 스킬 소리는 설정창의 EFFECT 볼륨을 따르는 스피커로 재생합니다.
    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatSound(clip);
        }
    }

    // 플레이어 그림보다 한 칸 앞에 그려지도록 순서 번호를 알려 줍니다. (이펙트, 검기에서 사용)
    protected int EffectSortingOrder
    {
        get
        {
            SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
            return playerRenderer != null ? playerRenderer.sortingOrder + 1 : 0;
        }
    }
}

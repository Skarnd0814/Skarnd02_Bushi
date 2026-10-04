using UnityEngine;

// 플레이어 머리 위에 공격 쿨타임을 하얀 게이지로 보여 주는 스크립트입니다.
// 쿨타임이 돌고 있을 때만 보이고, 공격할 수 있게 되면 사라집니다.
public class CooldownBar : MonoBehaviour
{
    [SerializeField] private PlayerAttack playerAttack;

    [Tooltip("줄어들거나 늘어나는 하얀 막대")]
    [SerializeField] private Transform fill;

    [Tooltip("게이지가 가득 찼을 때의 가로 길이 (Fill의 최대 Scale X)")]
    [SerializeField] private float width = 1f;

    [Tooltip("체크: 남은 시간만큼 줄어듭니다. 해제: 0부터 차오릅니다.")]
    [SerializeField] private bool showRemaining = true;

    private SpriteRenderer[] renderers;
    private bool isVisible = true;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        if (playerAttack == null) playerAttack = GetComponentInParent<PlayerAttack>();
        SetVisible(false);
    }

    // LateUpdate: 모든 Update가 끝난 뒤에 실행됩니다. 화면 표시를 갱신하기 좋은 곳입니다.
    private void LateUpdate()
    {
        float remaining = playerAttack.CooldownRemaining;
        float duration = playerAttack.CooldownDuration;

        bool visible = !playerAttack.CooldownDisabled && remaining > 0f && duration > 0f;
        SetVisible(visible);
        if (!visible) return;

        float ratio = remaining / duration;
        if (!showRemaining) ratio = 1f - ratio;

        // 막대의 왼쪽 끝은 그대로 두고, 오른쪽 끝만 줄어들거나 늘어나게 합니다.
        float fillWidth = width * ratio;
        fill.localScale = new Vector3(fillWidth, fill.localScale.y, 1f);
        fill.localPosition = new Vector3(-width / 2f + fillWidth / 2f, fill.localPosition.y, fill.localPosition.z);
    }

    private void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        isVisible = visible;
        foreach (SpriteRenderer spriteRenderer in renderers)
        {
            spriteRenderer.enabled = visible;
        }
    }
}

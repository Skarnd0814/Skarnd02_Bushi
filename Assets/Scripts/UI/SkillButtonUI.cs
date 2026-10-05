using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 스킬 버튼 하나의 "모습"을 담당하는 스크립트입니다. (쿨타임 어둡게 덮기, 남은 초, 누를 때 어두워지기)
// 누르는 처리는 같은 버튼에 붙은 MobileControlButton + MobileControls가 합니다.
// 구매하지 않은 스킬이면 MobileControls가 버튼을 통째로 숨깁니다.
[RequireComponent(typeof(Image))]
public class SkillButtonUI : MonoBehaviour
{
    [SerializeField] private PlayerSkills playerSkills;
    [Tooltip("몇 번 스킬의 버튼인지 (1~3)")]
    [SerializeField] private int skillNumber = 1;

    [Header("화면 표시 (비워 두면 그 부분은 표시하지 않습니다)")]
    [Tooltip("쿨타임 동안 버튼을 덮을 어두운 Image. 버튼과 같은 그림, Filled 설정이 자동으로 들어갑니다.")]
    [SerializeField] private Image cooldownOverlay;
    [Tooltip("쿨타임 남은 초를 보여 줄 글자")]
    [SerializeField] private TMP_Text cooldownText;
    [Tooltip("손가락으로 누르고 있는 동안 버튼 색 (흰색 = 변화 없음)")]
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private Image buttonImage;
    private MobileControlButton controlButton;
    private PlayerSkill skill;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        controlButton = GetComponent<MobileControlButton>();

        if (cooldownOverlay != null)
        {
            // 덮개를 버튼과 같은 모양으로 만들고, 남은 시간만큼 시계처럼 줄어들게 설정합니다.
            cooldownOverlay.sprite = buttonImage.sprite;
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            cooldownOverlay.fillClockwise = false;
            cooldownOverlay.raycastTarget = false;
            cooldownOverlay.fillAmount = 0f;
        }
        if (cooldownText != null) cooldownText.raycastTarget = false;
    }

    private void Start()
    {
        skill = playerSkills != null ? playerSkills.GetSkill(skillNumber) : null;
        if (skill == null)
        {
            Debug.LogWarning($"{name}: {skillNumber}번 스킬을 찾지 못했습니다. Player Skills 칸과 Player에 붙은 스킬 스크립트를 확인하세요.");
        }
    }

    private void Update()
    {
        bool pressed = controlButton != null && controlButton.IsPressed;
        buttonImage.color = pressed ? pressedColor : Color.white;

        if (skill == null) return;

        float remaining = skill.CooldownRemaining;
        bool onCooldown = remaining > 0f;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = onCooldown ? remaining / Mathf.Max(0.01f, skill.CooldownDuration) : 0f;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(onCooldown);
            if (onCooldown) cooldownText.text = Mathf.CeilToInt(remaining).ToString();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

// 모바일 조작 버튼 하나에 붙는 스크립트입니다. (← → 점프 공격)
// 손가락이 버튼 위에 있는지는 MobileControls가 매 순간 직접 확인하고, 이 스크립트는
// "어떤 버튼인지"와 "눌렸을 때의 모습"만 담당합니다.
[RequireComponent(typeof(Image))]
public class MobileControlButton : MonoBehaviour
{
    public enum ButtonType { Left, Right, Jump, Attack }

    [SerializeField] private ButtonType buttonType;

    [Tooltip("누르고 있는 동안 보여 줄 그림 (Controller_Highlight)")]
    [SerializeField] private Sprite pressedSprite;

    private Image image;
    private RectTransform rectTransform;
    private Sprite normalSprite;

    public ButtonType Type => buttonType;
    public bool IsPressed { get; private set; }

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = (RectTransform)transform;
        normalSprite = image.sprite;
    }

    public void SetPressed(bool pressed)
    {
        if (IsPressed == pressed) return;
        IsPressed = pressed;

        if (pressedSprite != null) image.sprite = pressed ? pressedSprite : normalSprite;
    }

    // 화면의 한 점(손가락 위치)이 이 버튼 안에 있는지 확인합니다.
    // padding만큼 버튼보다 조금 넓게 봐서, 버튼 가장자리를 눌러도 잘 인식되게 합니다.
    public bool ContainsScreenPoint(Vector2 screenPoint, float padding)
    {
        // Canvas가 Screen Space - Overlay라서 카메라는 null을 넘깁니다.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out Vector2 localPoint))
        {
            return false;
        }

        Rect area = rectTransform.rect;
        area.xMin -= padding;
        area.xMax += padding;
        area.yMin -= padding;
        area.yMax += padding;
        return area.Contains(localPoint);
    }
}

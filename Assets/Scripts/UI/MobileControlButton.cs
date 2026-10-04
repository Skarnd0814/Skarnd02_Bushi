using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 모바일 조작 버튼 하나에 붙는 스크립트입니다. (← → 점프 공격)
// 일반 Button은 "손을 뗄 때" 눌린 것으로 처리하지만, 이 버튼은 "닿는 순간" 바로 반응하고
// 누르고 있는 동안을 알 수 있어서 이동 버튼에 알맞습니다.
[RequireComponent(typeof(Image))]
public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ButtonType { Left, Right, Jump, Attack }

    [SerializeField] private ButtonType buttonType;
    [SerializeField] private MobileControls controls;

    [Tooltip("누르고 있는 동안 보여 줄 그림 (Controller_Highlight)")]
    [SerializeField] private Sprite pressedSprite;

    private Image image;
    private Sprite normalSprite;
    private bool isPressed;

    private void Awake()
    {
        image = GetComponent<Image>();
        normalSprite = image.sprite;
    }

    // 손가락(마우스)이 버튼에 닿는 순간
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed) return;
        isPressed = true;

        if (pressedSprite != null) image.sprite = pressedSprite;
        controls.Press(buttonType);
    }

    // 손가락을 뗀 순간
    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    // 누른 채로 손가락이 버튼 밖으로 미끄러져 나간 순간
    public void OnPointerExit(PointerEventData eventData)
    {
        Release();
    }

    // 버튼이 꺼지면(게임 오버 화면 등) 누르고 있던 상태를 풀어 줍니다.
    private void OnDisable()
    {
        Release();
    }

    private void Release()
    {
        if (!isPressed) return;
        isPressed = false;

        image.sprite = normalSprite;
        controls.Release(buttonType);
    }
}

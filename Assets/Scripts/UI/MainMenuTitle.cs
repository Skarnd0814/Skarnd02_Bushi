using UnityEngine;
using UnityEngine.UI;

// 메인 메뉴의 게임 타이틀(BUSHI 그림)에 붙이는 "이름표" 스크립트입니다.
// 로딩 화면(TitleLoader)이 이 이름표를 찾아서, 큰 타이틀을 정확히 이 자리·이 크기로 옮겨 줍니다.
// 옮기는 동안에는 타이틀이 두 개로 보이지 않도록 이 그림을 잠깐 숨깁니다.
[RequireComponent(typeof(Image))]
public class MainMenuTitle : MonoBehaviour
{
    public RectTransform Rect => (RectTransform)transform;

    public void SetVisible(bool visible)
    {
        GetComponent<Image>().enabled = visible;
    }
}

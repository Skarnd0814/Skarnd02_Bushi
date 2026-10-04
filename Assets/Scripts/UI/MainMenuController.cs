using UnityEngine;
using UnityEngine.UI;

// 메인 메뉴의 버튼들이 눌렸을 때 무슨 일을 할지 정하는 스크립트입니다.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsPopup settingsPopup;

    private void Awake()
    {
        settingsButton.onClick.AddListener(settingsPopup.Open);
    }
}

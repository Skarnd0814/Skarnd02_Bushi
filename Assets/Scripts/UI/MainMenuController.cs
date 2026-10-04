using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메인 메뉴의 버튼들이 눌렸을 때 무슨 일을 할지 정하는 스크립트입니다.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsPopup settingsPopup;

    [Header("START를 누르면 이동할 씬 이름")]
    [SerializeField] private string inGameSceneName = "InGameScene";

    private void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(settingsPopup.Open);
    }

    private void StartGame()
    {
        // 두 번 눌러서 씬을 두 번 불러오는 일이 없도록 버튼을 잠급니다.
        startButton.interactable = false;
        SceneManager.LoadScene(inGameSceneName);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 중 일시정지 버튼과 일시정지 창(계속하기 / 다시 하기 / 메인 메뉴)을 처리하는 스크립트입니다.
// 일시정지는 Time.timeScale(게임 속 시간이 흐르는 빠르기)을 0으로 만들어서 모든 움직임을 멈춥니다.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("일시정지 버튼 (게임 화면 오른쪽 위)")]
    [SerializeField] private Button pauseButton;

    [Header("일시정지 창")]
    [Tooltip("일시정지 때 나타날 화면 전체 (어두운 판과 그 안의 모든 것)")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        menuRoot.SetActive(false);

        pauseButton.onClick.AddListener(Pause);
        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private void Update()
    {
        // PC 테스트용: Esc 키로 일시정지 / 계속하기
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    // 휴대폰에서 홈 버튼을 누르거나 전화가 와서 게임이 뒤로 가면 자동으로 일시정지합니다.
    private void OnApplicationPause(bool paused)
    {
        if (paused) Pause();
    }

    // 씬을 떠날 때 시간이 멈춘 채로 남지 않도록 되돌립니다. (안전장치)
    private void OnDestroy()
    {
        if (IsPaused) Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (IsPaused || gameManager.IsGameOver) return;

        IsPaused = true;
        Time.timeScale = 0f;
        menuRoot.SetActive(true);
        ClearSelectedButton();
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        menuRoot.SetActive(false);
        ClearSelectedButton();
    }

    // 마우스로 누른 버튼은 "선택된 상태"로 남아서, 나중에 Enter 같은 키를 누르면 다시 눌린 것으로 처리될 수 있습니다.
    // 그래서 누른 뒤에는 선택을 풀어 줍니다. (하이라이트 그림이 남아 있는 것도 막아 줍니다)
    private void ClearSelectedButton()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void Restart()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }

    private void LoadScene(string sceneName)
    {
        resumeButton.interactable = false;
        restartButton.interactable = false;
        mainMenuButton.interactable = false;

        // ⚠️ 시간을 다시 흐르게 한 뒤에 씬을 바꿔야 합니다. 안 그러면 새 씬도 멈춘 채로 시작합니다.
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}

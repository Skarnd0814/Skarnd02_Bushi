using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 앱을 켜면 가장 먼저 나오는 로딩(타이틀) 화면을 담당하는 스크립트입니다. TitleScene에서만 씁니다.
// 1) 검은 화면에서 배경 + 큰 타이틀 + 제작자 이름이 서서히 나타납니다.
// 2) 그동안 메인 메뉴를 뒤에서 미리 불러 둡니다. (Additive: 지금 화면을 지우지 않고 겹쳐서 불러오기)
// 3) 제작자 이름이 사라집니다.
// 4) 타이틀이 작아지면서 메인 메뉴 타이틀 자리로 이동하고, 동시에 로딩 배경이 사라지며 메인 메뉴가 드러납니다.
// 5) 다 끝나면 로딩 화면(TitleScene)을 통째로 지웁니다.
public class TitleLoader : MonoBehaviour
{
    [Header("다음 씬")]
    [SerializeField] private string nextSceneName = "MainMenuScene";

    [Header("화면 연결")]
    [Tooltip("로딩 화면 전체 (처음에 검은 화면에서 서서히 나타나게 할 때 사용)")]
    [SerializeField] private CanvasGroup screenGroup;
    [Tooltip("로딩 배경 (마지막에 서서히 사라짐)")]
    [SerializeField] private CanvasGroup backgroundGroup;
    [Tooltip("가운데의 큰 타이틀")]
    [SerializeField] private RectTransform title;
    [Tooltip("제작자 이름 (타이틀보다 먼저 사라짐)")]
    [SerializeField] private CanvasGroup creatorGroup;
    [Tooltip("로딩 카메라의 소리 듣는 귀. 메인 메뉴가 열리는 순간 꺼서 귀가 두 개가 되지 않게 합니다.")]
    [SerializeField] private AudioListener loadingListener;

    [Header("시간 (초)")]
    [Tooltip("처음에 검은 화면에서 나타나는 시간")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [Tooltip("로딩 화면을 최소한 이만큼은 보여 줍니다 (메인 메뉴를 더 빨리 불러와도 기다림)")]
    [SerializeField] private float minShowTime = 2f;
    [Tooltip("제작자 이름이 사라지는 시간")]
    [SerializeField] private float creatorFadeDuration = 0.5f;
    [Tooltip("타이틀이 메인 메뉴 자리로 이동하면서 배경이 사라지는 시간")]
    [SerializeField] private float moveDuration = 0.9f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 메인 메뉴에도 귀(AudioListener)가 있으므로, 메인 메뉴가 열리는 순간 로딩 화면의 귀를 끕니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == nextSceneName && loadingListener != null) loadingListener.enabled = false;
    }

    private IEnumerator Start()
    {
        float startTime = Time.unscaledTime;

        // 메인 메뉴를 뒤에서 미리 불러오기 시작합니다. 로딩 화면이 맨 앞에 있어서 아직 보이지 않습니다.
        AsyncOperation loading = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);

        // 1) 검은 화면에서 서서히 나타나기
        yield return Fade(screenGroup, 0f, 1f, fadeInDuration);

        // 2) 메인 메뉴를 다 불러오고, 최소 시간이 지날 때까지 기다리기
        while (!loading.isDone || Time.unscaledTime - startTime < minShowTime)
        {
            yield return null;
        }

        // 이제부터 새로 만들어지는 것들은 메인 메뉴에 속하도록 메인 메뉴를 "현재 씬"으로 정합니다.
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

        MainMenuTitle target = FindFirstObjectByType<MainMenuTitle>();
        if (target != null) target.SetVisible(false);

        // 3) 제작자 이름 사라지기
        yield return Fade(creatorGroup, 1f, 0f, creatorFadeDuration);

        // 4) 타이틀 이동·축소 + 배경 사라지기 (동시에)
        yield return MoveTitleToMenu(target);

        if (target != null) target.SetVisible(true);

        // 5) 로딩 화면(이 씬) 지우기
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    private IEnumerator MoveTitleToMenu(MainMenuTitle target)
    {
        // Overlay 캔버스에서는 "월드 위치 = 화면 픽셀 위치"라서, 두 씬의 캔버스가 달라도 같은 기준으로 비교할 수 있습니다.
        Vector3 fromPosition = GetWorldCenter(title);
        Vector3 toPosition = fromPosition;
        float toScale = 1f;

        if (target != null)
        {
            toPosition = GetWorldCenter(target.Rect);
            toScale = GetWorldWidth(target.Rect) / GetWorldWidth(title);
        }

        for (float time = 0f; time < moveDuration; time += Time.unscaledDeltaTime)
        {
            // SmoothStep: 천천히 출발해서 빨라졌다가 천천히 멈추는 부드러운 움직임
            float t = Mathf.SmoothStep(0f, 1f, time / moveDuration);
            title.position = Vector3.Lerp(fromPosition, toPosition, t);
            title.localScale = Vector3.one * Mathf.Lerp(1f, toScale, t);
            if (backgroundGroup != null) backgroundGroup.alpha = 1f - t;
            yield return null;
        }

        title.position = toPosition;
        title.localScale = Vector3.one * toScale;
        if (backgroundGroup != null) backgroundGroup.alpha = 0f;
    }

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        group.alpha = to;
    }

    private static Vector3 GetWorldCenter(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }

    private static float GetWorldWidth(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return corners[2].x - corners[0].x;
    }
}

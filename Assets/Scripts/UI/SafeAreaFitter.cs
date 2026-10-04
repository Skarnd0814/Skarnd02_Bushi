using UnityEngine;

// 휴대폰의 노치, 펀치홀(카메라 구멍), 둥근 모서리에 UI가 가려지지 않도록
// 이 오브젝트의 영역을 화면의 "안전 영역(Safe Area)" 안으로 줄여 주는 스크립트입니다.
// 이 오브젝트 안에 넣은 UI들은 모두 안전 영역 안에 배치됩니다.
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 화면을 돌리면 안전 영역도 바뀌므로, 바뀌었을 때마다 다시 맞춥니다.
    private void Update()
    {
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        if (Screen.safeArea == lastSafeArea && screenSize == lastScreenSize) return;

        lastSafeArea = Screen.safeArea;
        lastScreenSize = screenSize;
        Apply();
    }

    private void Apply()
    {
        Rect safeArea = Screen.safeArea;

        // 안전 영역을 0~1 비율로 바꿔서 기준점(Anchor)으로 씁니다.
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}

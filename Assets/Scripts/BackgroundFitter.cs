using UnityEngine;

// 배경 그림이 어떤 화면 비율(16:9, 19.5:9, 20:9 ...)에서도 화면을 빈틈없이 덮도록
// 크기를 자동으로 맞춰 주는 스크립트입니다. 그림 비율은 그대로 유지하고, 남는 부분은 잘려 보입니다.
// ExecuteAlways: Play를 누르지 않은 편집 화면에서도 실행되어, 맞춰진 모습을 미리 볼 수 있습니다.
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private float lastAspect;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    // 화면을 돌리거나 창 크기를 바꾸면 비율이 바뀌므로, 바뀌었을 때마다 다시 맞춥니다.
    private void LateUpdate()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || spriteRenderer.sprite == null) return;

        if (Mathf.Approximately(mainCamera.aspect, lastAspect)) return;
        lastAspect = mainCamera.aspect;
        Fit();
    }

    private void Fit()
    {
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        float viewHeight = mainCamera.orthographicSize * 2f;
        float viewWidth = viewHeight * mainCamera.aspect;

        // 가로와 세로 중 더 많이 키워야 하는 쪽에 맞추면 화면 전체가 덮입니다.
        float scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}

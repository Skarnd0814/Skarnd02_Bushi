using TMPro;
using UnityEngine;

// 장애물을 부순 자리에 "+130" 같은 점수 글자를 띄우는 스크립트입니다.
// 글자는 위로 떠오르면서 서서히 사라집니다.
public class ScorePopupSpawner : MonoBehaviour
{
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private ScoreManager scoreManager;

    [Header("글자 모양")]
    [SerializeField] private TMP_FontAsset font;
    [Tooltip("게임 세계 기준 글자 크기 (5 = 약 0.5칸)")]
    [SerializeField] private float fontSize = 5f;
    [SerializeField] private Color normalColor = Color.white;
    [Tooltip("피버 타임(점수 2배)일 때 글자 색")]
    [SerializeField] private Color feverColor = new Color(1f, 0.55f, 0.2f);
    [Tooltip("숫자가 클수록 다른 그림보다 앞에 보입니다")]
    [SerializeField] private int sortingOrder = 30;

    [Header("움직임")]
    [Tooltip("1초에 몇 칸 떠오를지")]
    [SerializeField] private float riseSpeed = 1.5f;
    [Tooltip("몇 초 뒤에 사라질지")]
    [SerializeField] private float lifetime = 0.7f;

    private void OnEnable()
    {
        comboManager.ObstacleScored += OnObstacleScored;
    }

    private void OnDisable()
    {
        comboManager.ObstacleScored -= OnObstacleScored;
    }

    private void OnObstacleScored(Vector3 position, int points)
    {
        GameObject popupObject = new GameObject("ScorePopup");
        popupObject.transform.position = position;

        // TextMeshPro(UGUI가 아닌 것): Canvas 없이 게임 세계에 바로 쓰는 글자입니다.
        TextMeshPro text = popupObject.AddComponent<TextMeshPro>();
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.sortingOrder = sortingOrder;
        text.text = $"+{points}";
        text.color = scoreManager.DestroyScoreMultiplier > 1f ? feverColor : normalColor;

        ScorePopup popup = popupObject.AddComponent<ScorePopup>();
        popup.Init(text, riseSpeed, lifetime);
    }
}

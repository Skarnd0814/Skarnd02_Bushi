using TMPro;
using UnityEngine;

// 떠오르면서 사라지는 점수 글자 하나입니다. ScorePopupSpawner가 만들어 붙입니다.
public class ScorePopup : MonoBehaviour
{
    private TextMeshPro text;
    private Color baseColor;
    private float riseSpeed;
    private float lifetime;
    private float age;

    public void Init(TextMeshPro popupText, float speed, float life)
    {
        text = popupText;
        baseColor = popupText.color;
        riseSpeed = speed;
        lifetime = life;
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // 처음 절반은 또렷하게, 나머지 절반 동안 서서히 투명해집니다.
        float t = age / lifetime;
        Color color = baseColor;
        color.a = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
        text.color = color;
    }
}

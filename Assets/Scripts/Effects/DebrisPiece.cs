using UnityEngine;

// 장애물이 부서질 때 사방으로 튀는 작은 네모 조각 하나입니다.
// 따로 그림이나 프리팹을 만들 필요 없이, Burst()를 부르면 코드가 조각들을 만들어 줍니다.
public class DebrisPiece : MonoBehaviour
{
    private const float Gravity = 20f;

    // 모든 조각이 함께 쓰는 하얀 네모 그림 (크기 1칸). 색은 조각마다 따로 칠합니다.
    private static Sprite squareSprite;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector2 velocity;
    private float angularVelocity;
    private float lifetime;
    private float age;

    // position 위치에서 조각 count개를 사방(주로 위쪽)으로 튀깁니다.
    // speedMultiplier: 조각이 튀는 속도 배율 (1 = 보통)
    public static void Burst(Vector3 position, Color[] colors, int count, float size, int sortingOrder,
        float speedMultiplier = 1f)
    {
        if (colors == null || colors.Length == 0 || count <= 0) return;

        if (squareSprite == null)
        {
            Texture2D texture = Texture2D.whiteTexture; // 4x4 크기의 하얀 그림
            squareSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject piece = new GameObject("Debris");
            piece.transform.position = position;
            piece.transform.localScale = Vector3.one * size * Random.Range(0.7f, 1.3f);

            SpriteRenderer renderer = piece.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = colors[Random.Range(0, colors.Length)];

            // 20도 ~ 160도: 오른쪽 위부터 왼쪽 위까지 부채꼴로 튀어 오릅니다.
            float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad;
            float speed = Random.Range(3f, 7f) * speedMultiplier;

            DebrisPiece debris = piece.AddComponent<DebrisPiece>();
            debris.spriteRenderer = renderer;
            debris.baseColor = renderer.color;
            debris.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            debris.angularVelocity = Random.Range(-720f, 720f);
            debris.lifetime = Random.Range(0.4f, 0.7f);
        }
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 중력 때문에 점점 아래로 휘어지며 날아가고, 빙글빙글 돌면서 서서히 투명해집니다.
        velocity.y -= Gravity * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);

        float t = age / lifetime;
        Color color = baseColor;
        color.a = 1f - t * t;
        spriteRenderer.color = color;
    }
}

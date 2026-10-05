using UnityEngine;

// 피버 타임에 장애물을 부수면 튀어나오는 코인 하나입니다.
// 위로 튀어 올랐다가 중력 때문에 떨어지면서 서서히 사라집니다. (빙글빙글 도는 그림은 SpriteFlipbook이 담당)
// CoinPopupSpawner가 코드로 만들어 붙입니다.
[RequireComponent(typeof(SpriteRenderer))]
public class FlyingCoin : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector2 velocity;
    private float gravity;
    private float lifetime;
    private float age;

    public void Launch(Vector2 startVelocity, float gravityStrength, float life)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        velocity = startVelocity;
        gravity = gravityStrength;
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

        velocity.y -= gravity * Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);

        // 처음 60%는 또렷하게, 나머지 동안 서서히 투명해집니다.
        float t = age / lifetime;
        Color color = spriteRenderer.color;
        color.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
        spriteRenderer.color = color;
    }
}

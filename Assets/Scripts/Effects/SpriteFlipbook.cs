using UnityEngine;

// 그림 여러 장을 순서대로 넘겨 움직이는 이펙트(흙먼지 등)를 보여 주는 스크립트입니다.
// 따로 프리팹이나 Animator를 만들 필요 없이, Spawn()을 부르면 코드가 이펙트 오브젝트를 만들어 줍니다.
// 한 번만 재생하면 스스로 사라지고, loop를 켜면 계속 반복합니다. (검기처럼 날아가는 것에 사용)
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbook : MonoBehaviour
{
    private Sprite[] frames;
    private float secondsPerFrame;
    private bool loop;
    private float age;

    private SpriteRenderer spriteRenderer;
    private Transform followTarget;
    private Vector3 followOffset;

    public SpriteRenderer Renderer => spriteRenderer;

    // position 위치에 이펙트를 만들고 바로 재생합니다. 그림이 비어 있으면 아무것도 만들지 않습니다.
    public static SpriteFlipbook Spawn(string name, Sprite[] frames, float framesPerSecond, bool loop,
        Vector3 position, float scale, int sortingOrder)
    {
        if (frames == null || frames.Length == 0) return null;

        GameObject effect = new GameObject(name);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;
        renderer.sprite = frames[0];

        SpriteFlipbook flipbook = effect.AddComponent<SpriteFlipbook>();
        flipbook.spriteRenderer = renderer;
        flipbook.frames = frames;
        flipbook.secondsPerFrame = 1f / Mathf.Max(1f, framesPerSecond);
        flipbook.loop = loop;
        return flipbook;
    }

    // 플레이어 발밑처럼, 정해 준 대상을 따라다니게 합니다.
    public void Follow(Transform target, Vector3 offset)
    {
        followTarget = target;
        followOffset = offset;
    }

    // 재생 위치를 앞으로 건너뜁니다. (여러 개를 동시에 만들 때 서로 다른 장면부터 보이게 할 때 사용)
    public void SkipTime(float seconds)
    {
        age += seconds;
    }

    private void LateUpdate()
    {
        age += Time.deltaTime;

        int index = Mathf.FloorToInt(age / secondsPerFrame);
        if (index >= frames.Length)
        {
            if (!loop)
            {
                Destroy(gameObject);
                return;
            }
            index %= frames.Length;
        }
        spriteRenderer.sprite = frames[index];

        if (followTarget != null)
        {
            transform.position = followTarget.position + followOffset;
        }
    }
}

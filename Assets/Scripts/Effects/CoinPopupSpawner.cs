using TMPro;
using UnityEngine;

// 피버 타임에 장애물을 부수면, 부순 자리에서 코인이 터져 나오고 "Coin +1" 글자가 떠오르게 하는 스크립트입니다.
// 점수 글자("+260")는 ScorePopupSpawner가 따로 띄우고, 이 글자는 그 바로 아래에 나옵니다.
// 지금은 보여 주기만 합니다. (얻은 코인을 저장하는 기능은 다음 단계에서 추가)
public class CoinPopupSpawner : MonoBehaviour
{
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private FeverManager feverManager;

    [Header("코인 규칙")]
    [Tooltip("피버 타임에 장애물 하나를 부술 때 얻는 코인 수")]
    [SerializeField] private int coinsPerObstacle = 1;

    [Header("글자 모양")]
    [Tooltip("비워 두면 TextMeshPro 기본 폰트를 씁니다. 점수 글자와 같은 폰트를 넣으세요.")]
    [SerializeField] private TMP_FontAsset font;
    [Tooltip("게임 세계 기준 글자 크기 (점수 글자는 5)")]
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private Color textColor = new Color(1f, 0.85f, 0.2f);
    [Tooltip("부순 자리로부터 글자가 나올 위치 (아래로 내려서 점수 글자와 겹치지 않게)")]
    [SerializeField] private Vector2 textOffset = new Vector2(0f, -0.5f);
    [Tooltip("1초에 몇 칸 떠오를지 (점수 글자와 같게 하면 나란히 올라갑니다)")]
    [SerializeField] private float textRiseSpeed = 1.5f;
    [Tooltip("몇 초 뒤에 사라질지")]
    [SerializeField] private float textLifetime = 0.7f;
    [Tooltip("숫자가 클수록 다른 그림보다 앞에 보입니다")]
    [SerializeField] private int textSortingOrder = 31;

    [Header("튀어나오는 코인")]
    [Tooltip("코인이 도는 그림들 (Coin_Split_0 ~ _7)")]
    [SerializeField] private Sprite[] coinFrames;
    [Tooltip("1초에 몇 장씩 넘길지 (클수록 빨리 돕니다)")]
    [SerializeField] private float coinFramesPerSecond = 14f;
    [Tooltip("장애물 하나를 부술 때 튀어나오는 코인 그림 개수 (보이는 개수일 뿐, 얻는 코인 수와는 별개)")]
    [SerializeField] private int coinCount = 4;
    [Tooltip("코인 크기 배율 (0.3 = 약 0.4칸)")]
    [SerializeField] private float coinScale = 0.3f;
    [Tooltip("튀어 오르는 속도 (가장 느릴 때 ~ 가장 빠를 때)")]
    [SerializeField] private float launchSpeedMin = 4f;
    [SerializeField] private float launchSpeedMax = 7f;
    [Tooltip("코인을 아래로 끌어당기는 힘")]
    [SerializeField] private float gravity = 15f;
    [Tooltip("코인이 사라질 때까지의 시간(초)")]
    [SerializeField] private float coinLifetime = 0.8f;
    [Tooltip("숫자가 클수록 다른 그림보다 앞에 보입니다 (글자보다 작게)")]
    [SerializeField] private int coinSortingOrder = 29;

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
        if (feverManager == null || !feverManager.IsFever) return;

        SpawnText(position);
        SpawnCoins(position);
    }

    private void SpawnText(Vector3 position)
    {
        GameObject popupObject = new GameObject("CoinPopup");
        popupObject.transform.position = position + (Vector3)textOffset;

        // TextMeshPro(UGUI가 아닌 것): Canvas 없이 게임 세계에 바로 쓰는 글자입니다.
        TextMeshPro text = popupObject.AddComponent<TextMeshPro>();
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.sortingOrder = textSortingOrder;
        text.text = $"Coin +{coinsPerObstacle}";
        text.color = textColor;

        // 점수 글자와 같은 방식으로 떠오르며 사라집니다.
        ScorePopup popup = popupObject.AddComponent<ScorePopup>();
        popup.Init(text, textRiseSpeed, textLifetime);
    }

    private void SpawnCoins(Vector3 position)
    {
        if (coinFrames == null || coinFrames.Length == 0) return;

        for (int i = 0; i < coinCount; i++)
        {
            SpriteFlipbook coin = SpriteFlipbook.Spawn("Coin", coinFrames, coinFramesPerSecond, true,
                position, coinScale, coinSortingOrder);

            // 코인마다 도는 순간을 다르게 해서 똑같이 돌지 않게 합니다.
            coin.SkipTime(Random.Range(0f, coinFrames.Length / coinFramesPerSecond));

            // 50도 ~ 130도: 위쪽으로 부채꼴 모양으로 튀어 오릅니다.
            float angle = Random.Range(50f, 130f) * Mathf.Deg2Rad;
            float speed = Random.Range(launchSpeedMin, launchSpeedMax);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            coin.gameObject.AddComponent<FlyingCoin>().Launch(velocity, gravity, coinLifetime);
        }
    }
}

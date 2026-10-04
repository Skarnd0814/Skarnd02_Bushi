using UnityEngine;

// 장애물 하나하나에 붙는 스크립트입니다.
// 지금은 "공격받으면 부서지는" 기능만 있고, 떨어지기 / 플레이어와 부딪히기는 STEP 8에서 추가합니다.
public class Obstacle : MonoBehaviour
{
    [Header("소리")]
    [SerializeField] private AudioClip breakSound;

    public bool IsBroken { get; private set; }

    public void Break()
    {
        if (IsBroken) return;
        IsBroken = true;

        if (breakSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCombatSound(breakSound);
        }

        Destroy(gameObject);
    }
}

using UnityEngine;
using UnityEngine.UI;

// 이 스크립트를 붙인 버튼은 눌릴 때마다 클릭음을 냅니다.
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => AudioManager.Instance?.PlayButtonClick());
    }
}

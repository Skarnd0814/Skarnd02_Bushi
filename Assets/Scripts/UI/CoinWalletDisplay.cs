using TMPro;
using UnityEngine;

// 지금 보유한 코인 수를 글자로 보여 주는 스크립트입니다. 코인 숫자 글자(TextMeshPro)에 붙입니다.
// 코인이 늘거나 줄면(피버 때 획득, 상점에서 구매) 그 순간 숫자가 바로 바뀝니다.
[RequireComponent(typeof(TMP_Text))]
public class CoinWalletDisplay : MonoBehaviour
{
    private TMP_Text coinText;

    private void Awake()
    {
        coinText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        CoinWallet.Changed += Refresh;
        Refresh(CoinWallet.Coins);
    }

    private void OnDisable()
    {
        CoinWallet.Changed -= Refresh;
    }

    private void Refresh(int coins)
    {
        coinText.text = coins.ToString();
    }
}

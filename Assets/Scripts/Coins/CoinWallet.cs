using System;
using UnityEngine;

// 플레이어가 모은 코인(게임 재화)을 휴대폰에 저장하고 알려 주는 "지갑"입니다.
// 어느 씬에서든 CoinWallet.Coins로 읽을 수 있고, 게임을 껐다 켜도 남아 있습니다. (PlayerPrefs)
// 나중에 상점에서 스킬을 살 때는 CoinWallet.TrySpend(가격)을 부르면 됩니다.
public static class CoinWallet
{
    // 코인을 저장하는 이름표입니다.
    public const string CoinsKey = "Coins";

    // 코인 수가 바뀔 때마다 바뀐 뒤의 총 코인 수를 알려 줍니다. (화면 표시 등에서 사용)
    public static event Action<int> Changed;

    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    // 코인을 더합니다. 바로 휴대폰에 기록하지는 않고, Save()를 부를 때 한꺼번에 기록합니다.
    // (피버 때 1초에 수십 번 더해지므로 매번 기록하면 느려질 수 있습니다)
    public static void Add(int amount)
    {
        if (amount <= 0) return;

        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        Changed?.Invoke(Coins);
    }

    // 코인이 충분하면 빼고 true, 모자라면 아무것도 하지 않고 false를 돌려줍니다. (상점에서 사용 예정)
    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Coins < amount) return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - amount);
        Save();
        Changed?.Invoke(Coins);
        return true;
    }

    // 지금까지의 코인을 휴대폰에 확실히 기록합니다.
    public static void Save()
    {
        PlayerPrefs.Save();
    }
}

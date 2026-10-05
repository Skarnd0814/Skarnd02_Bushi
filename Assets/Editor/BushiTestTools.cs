using UnityEditor;
using UnityEngine;

// 개발 중 테스트를 쉽게 하기 위한 "에디터 전용" 메뉴입니다. (게임 빌드에는 들어가지 않습니다)
// 유니티 위쪽 메뉴 Bushi > 테스트 에서 사용합니다. Play 중에도, Play 전에도 누를 수 있습니다.
public static class BushiTestTools
{
    [MenuItem("Bushi/테스트/코인 +500")]
    private static void AddCoins()
    {
        CoinWallet.Add(500);
        CoinWallet.Save();
        Debug.Log($"[테스트] 코인 +500 → 보유 코인 {CoinWallet.Coins}");
    }

    [MenuItem("Bushi/테스트/코인 0으로 초기화")]
    private static void ResetCoins()
    {
        CoinWallet.TrySpend(CoinWallet.Coins);
        Debug.Log("[테스트] 보유 코인을 0으로 만들었습니다.");
    }

    [MenuItem("Bushi/테스트/스킬 구매 기록 초기화")]
    private static void LockAllSkills()
    {
        for (int skillNumber = 1; skillNumber <= 3; skillNumber++) SkillUnlocks.Lock(skillNumber);
        Debug.Log("[테스트] 스킬 1~3의 구매 기록을 지웠습니다. (최고 점수·볼륨·코인은 그대로)");
    }
}

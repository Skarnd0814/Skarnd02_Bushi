using System;
using UnityEngine;

// 스킬을 "구매했는지(잠금 해제했는지)"를 휴대폰에 저장하고 알려 주는 곳입니다.
// 나중에 상점을 만들면, 재화로 스킬을 산 순간 SkillUnlocks.Unlock(스킬 번호)만 부르면 됩니다.
// 저장된 값은 게임을 껐다 켜도 남아 있습니다. (PlayerPrefs)
public static class SkillUnlocks
{
    private const string KeyPrefix = "SkillUnlocked_";

    // 스킬 잠금 상태가 바뀌면 스킬 번호와 함께 알려 줍니다. (상점 UI 등에서 사용)
    public static event Action<int> Changed;

    public static bool IsUnlocked(int skillNumber)
    {
        return PlayerPrefs.GetInt(KeyPrefix + skillNumber, 0) == 1;
    }

    public static void Unlock(int skillNumber)
    {
        PlayerPrefs.SetInt(KeyPrefix + skillNumber, 1);
        PlayerPrefs.Save();
        Changed?.Invoke(skillNumber);
    }

    // 테스트용: 구매 기록을 지워 다시 잠급니다.
    public static void Lock(int skillNumber)
    {
        PlayerPrefs.DeleteKey(KeyPrefix + skillNumber);
        PlayerPrefs.Save();
        Changed?.Invoke(skillNumber);
    }
}

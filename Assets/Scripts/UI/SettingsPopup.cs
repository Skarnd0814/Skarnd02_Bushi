using UnityEngine;
using UnityEngine.UI;

// 설정창 팝업을 열고 닫으며, 슬라이더 3개를 AudioManager의 볼륨과 연결하는 스크립트입니다.
public class SettingsPopup : MonoBehaviour
{
    [Header("볼륨 슬라이더")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider combatSlider;

    [Header("닫기 버튼")]
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        // 슬라이더를 움직이면 AudioManager의 볼륨을 바꾸도록 연결합니다.
        bgmSlider.onValueChanged.AddListener(volume => AudioManager.Instance?.SetBgmVolume(volume));
        sfxSlider.onValueChanged.AddListener(volume => AudioManager.Instance?.SetSfxVolume(volume));
        combatSlider.onValueChanged.AddListener(volume => AudioManager.Instance?.SetCombatVolume(volume));

        closeButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        // 팝업을 열 때마다 슬라이더 위치를 저장된 볼륨에 맞춥니다.
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
        {
            bgmSlider.SetValueWithoutNotify(audio.BgmVolume);
            sfxSlider.SetValueWithoutNotify(audio.SfxVolume);
            combatSlider.SetValueWithoutNotify(audio.CombatVolume);
        }
    }

    public void Close()
    {
        // 바꾼 볼륨을 디스크에 저장하고 팝업을 숨깁니다.
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 전체의 소리(BGM, 버튼 효과음, 공격/파괴 효과음)를 관리하는 스크립트입니다.
// 씬이 바뀌어도 사라지지 않고, 게임 안에 딱 하나만 존재합니다.
public class AudioManager : MonoBehaviour
{
    // 다른 스크립트에서 AudioManager.Instance 로 이 관리자를 찾을 수 있습니다.
    public static AudioManager Instance { get; private set; }

    [Header("BGM 목록 (위에서부터 순서대로 번갈아 재생)")]
    [SerializeField] private AudioClip[] bgmClips;

    [Header("효과음")]
    [SerializeField] private AudioClip buttonClickClip;

    // PlayerPrefs(게임을 꺼도 남는 저장 공간)에 볼륨을 저장할 때 쓰는 이름표입니다.
    private const string BgmVolumeKey = "Volume_BGM";
    private const string SfxVolumeKey = "Volume_SFX";
    private const string CombatVolumeKey = "Volume_Combat";
    private const float DefaultVolume = 0.5f;

    // 소리를 내는 스피커 3개입니다. 종류별로 따로 두어야 볼륨을 따로 조절할 수 있습니다.
    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource combatSource;

    private int currentBgmIndex = -1;
    private bool isBgmPlaylistOn;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public float CombatVolume { get; private set; }

    private void Awake()
    {
        // 이미 AudioManager가 있으면(예: 메인 메뉴로 다시 돌아온 경우) 새로 생긴 것은 지웁니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = CreateSource();
        sfxSource = CreateSource();
        combatSource = CreateSource();

        // 저장된 볼륨을 불러옵니다. 저장된 값이 없으면 0.5(50%)를 씁니다.
        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, DefaultVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultVolume);
        CombatVolume = PlayerPrefs.GetFloat(CombatVolumeKey, DefaultVolume);
        bgmSource.volume = BgmVolume;
        sfxSource.volume = SfxVolume;
        combatSource.volume = CombatVolume;
    }

    // 씬이 새로 열릴 때마다 알림을 받습니다.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 곧 지워질 복제본(두 번째 AudioManager)은 아무것도 하지 않습니다.
        if (Instance != this) return;

        if (!isBgmPlaylistOn) PlayBgmPlaylist();
    }

    // 게임 오버로 BGM이 멈춘 뒤 "다시 하기"나 "메인 메뉴"로 씬이 바뀌면 BGM을 다시 틉니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;

        if (!isBgmPlaylistOn) PlayBgmPlaylist();
    }

    private void Update()
    {
        if (Instance != this) return;

        // 지금 곡이 끝나면 다음 곡을 재생합니다.
        if (isBgmPlaylistOn && !bgmSource.isPlaying)
        {
            PlayNextBgm();
        }
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        return source;
    }

    // ---------- BGM ----------

    public void PlayBgmPlaylist()
    {
        if (bgmClips == null || bgmClips.Length == 0) return;

        isBgmPlaylistOn = true;
        currentBgmIndex = -1;
        PlayNextBgm();
    }

    public void StopBgm()
    {
        isBgmPlaylistOn = false;
        bgmSource.Stop();
    }

    private void PlayNextBgm()
    {
        // 0 → 1 → 0 → 1 ... 처럼 목록의 끝에 닿으면 처음으로 돌아갑니다.
        currentBgmIndex = (currentBgmIndex + 1) % bgmClips.Length;
        AudioClip clip = bgmClips[currentBgmIndex];

        if (clip == null)
        {
            Debug.LogWarning($"AudioManager: BGM 목록 {currentBgmIndex}번 칸이 비어 있습니다.");
            StopBgm();
            return;
        }

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // ---------- 효과음 ----------

    public void PlayButtonClick()
    {
        if (buttonClickClip != null)
        {
            sfxSource.PlayOneShot(buttonClickClip);
        }
    }

    // 상점 구매음처럼 버튼·화면(UI)과 관련된 소리는 이 함수로 재생합니다. (설정창의 SFX 볼륨을 따릅니다)
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 캐릭터 공격, 장애물 파괴, 게임 오버 소리는 이 함수로 재생합니다.
    public void PlayCombatSound(AudioClip clip)
    {
        if (clip != null)
        {
            combatSource.PlayOneShot(clip);
        }
    }

    // ---------- 볼륨 (설정창 슬라이더에서 호출) ----------

    public void SetBgmVolume(float volume)
    {
        BgmVolume = volume;
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BgmVolumeKey, volume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = volume;
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
    }

    public void SetCombatVolume(float volume)
    {
        CombatVolume = volume;
        combatSource.volume = volume;
        PlayerPrefs.SetFloat(CombatVolumeKey, volume);
    }
}

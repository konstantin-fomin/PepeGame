// AudioManager.cs
// Version: 2026-05-26 v1.3 (Runtime volume control + fullscreen)

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // ================= AUDIO SOURCES =================

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource specialLoopSource;

    // ================= MUSIC =================

    [Header("Music")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    private AudioClip currentMusic;

    // ================= SFX =================

    [Header("SFX - Click")]
    [SerializeField] private AudioClip clickClip;
    [Range(0f, 1f)][SerializeField] private float clickVolume = 0.5f;
    [SerializeField] private AudioClip criticalClickClip;
    [Range(0f, 1f)][SerializeField] private float criticalClickVolume = 0.7f;

    [Header("SFX - Buy")]
    [SerializeField] private AudioClip buyClip;
    [Range(0f, 1f)][SerializeField] private float buyVolume = 0.7f;

    [Header("SFX - Error")]
    [SerializeField] private AudioClip errorClip;
    [Range(0f, 1f)][SerializeField] private float errorVolume = 0.6f;

    [Header("SFX - Rank Up")]
    [SerializeField] private AudioClip rankUpClip;
    [Range(0f, 1f)][SerializeField] private float rankUpVolume = 1f;

    [Header("SFX - Start Scene")]
    [SerializeField] private AudioClip startSceneClip;
    [Range(0f, 1f)][SerializeField] private float startSceneVolume = 1.0f;

    [Header("Special Start / End")]
    [SerializeField] private AudioClip specialStartClip;
    [Range(0f, 1f)][SerializeField] private float specialStartVolume = 0.8f;

    [SerializeField] private AudioClip specialEndClip;
    [Range(0f, 1f)][SerializeField] private float specialEndVolume = 0.6f;

    [Header("Special Loop")]
    [SerializeField] private AudioClip specialLoopClip;
    [Range(0f, 1f)][SerializeField] private float specialLoopVolume = 0.4f;

    [Header("SFX - Menu")]
    [SerializeField] private AudioClip menuHoverClip;
    [Range(0f, 1f)][SerializeField] private float menuHoverVolume = 0.4f;
    [SerializeField] private AudioClip menuClickClip;
    [Range(0f, 1f)][SerializeField] private float menuClickVolume = 0.7f;

    // ================= RUNTIME VOLUME =================

    private float musicVolumeMultiplier = 1f;
    private float sfxVolumeMultiplier = 1f;

    // ================= UNITY =================

    private void Start()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        sfxVolumeMultiplier = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    // ================= VOLUME API =================

    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        if (musicSource != null)
            musicSource.volume = musicVolume * musicVolumeMultiplier;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolumeMultiplier = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public float GetMusicVolume() =>
        PlayerPrefs.GetFloat("MusicVolume", 1f);

    public float GetSFXVolume() =>
        PlayerPrefs.GetFloat("SFXVolume", 1f);

    // ================= FULLSCREEN =================

    public static void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
    }

    public static bool GetFullscreen() =>
        PlayerPrefs.GetInt("Fullscreen", 0) == 1;

    // ================= MUSIC API =================

    public void PlayMusicForRank(RankData rank)
    {
        if (rank == null || rank.backgroundMusic == null || musicSource == null)
            return;

        if (currentMusic == rank.backgroundMusic)
            return;

        currentMusic = rank.backgroundMusic;

        musicSource.Stop();
        musicSource.clip = currentMusic;
        musicSource.volume = musicVolume * musicVolumeMultiplier;
        musicSource.loop = true;
        musicSource.Play();
    }

    // ================= SFX API =================

    public void PlayClick() => PlayOneShot(clickClip, clickVolume);
    public void PlayCriticalClick() => PlayOneShot(criticalClickClip, criticalClickVolume);
    public void PlayBuy() => PlayOneShot(buyClip, buyVolume);
    public void PlayError() => PlayOneShot(errorClip, errorVolume);
    public void PlayRankUp() => PlayOneShot(rankUpClip, rankUpVolume);

    public void PlayMenuHover() => PlayOneShot(menuHoverClip, menuHoverVolume);
    public void PlayMenuClick() => PlayOneShot(menuClickClip, menuClickVolume);

    public void PlayStartScene() => PlayOneShot(startSceneClip, startSceneVolume);

    public void PlaySpecialStart()
    {
        PlayOneShot(specialStartClip, specialStartVolume);
    }

    public void PlaySpecialEnd()
    {
        PlayOneShot(specialEndClip, specialEndVolume);
    }

    // ================= SPECIAL LOOP =================

    public void StartSpecialLoop()
    {
        if (specialLoopSource == null || specialLoopClip == null)
            return;

        specialLoopSource.clip = specialLoopClip;
        specialLoopSource.volume = specialLoopVolume;
        specialLoopSource.loop = true;
        specialLoopSource.Play();
    }

    public void StopSpecialLoop()
    {
        if (specialLoopSource == null)
            return;

        specialLoopSource.Stop();
    }

    // ================= INTERNAL =================

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volume * sfxVolumeMultiplier);
    }
}

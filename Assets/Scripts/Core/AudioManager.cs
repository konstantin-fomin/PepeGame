// AudioManager.cs
// Version: 2026-05-31 v1.8 (mute toggle + persisted audio settings + change event)

using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // ================= AUDIO SOURCES =================

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource specialLoopSource;

    // ================= MUSIC =================

    [Header("Music")]
    [Range(0f, 1f)]
    [SerializeField] private float masterMusicVolume = 0.5f;
    [SerializeField] private float crossfadeDuration = 1.5f;
    [SerializeField] private AudioClip menuMusicClip;
    [Range(0f, 1f)]
    [SerializeField] private float menuMusicVolume = 1f;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private AudioClip currentMusic;
    private float currentTrackVolume = 1f;
    private Coroutine crossfadeCoroutine;

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

    [Header("SFX - Popup")]
    [SerializeField] private AudioClip popupClip;
    [Range(0f, 1f)][SerializeField] private float popupVolume = 0.7f;
    [SerializeField] private AudioClip popupSwoshClip;
    [Range(0f, 1f)][SerializeField] private float popupSwoshVolume = 0.5f;

    [Header("SFX - Toast")]
    [SerializeField] private AudioClip toastClip;
    [Range(0f, 1f)][SerializeField] private float toastVolume = 0.5f;

    // ================= RUNTIME VOLUME =================

    private float musicVolumeMultiplier = 1f;
    private float sfxVolumeMultiplier = 1f;

    // Mute support: remember the last audible level so unmute restores it.
    private float lastNonZeroMusicVolume = 0.7f;
    private float lastNonZeroSfxVolume = 0.7f;
    private bool isMuted = false;

    public event Action OnAudioSettingsChanged;

    public bool IsMuted => isMuted;
    public float MusicVolume => musicVolumeMultiplier;
    public float SfxVolume => sfxVolumeMultiplier;

    // PlayerPrefs keys (reusing the pre-existing MusicVolume/SFXVolume keys).
    private const string PK_MUSIC = "MusicVolume";
    private const string PK_SFX = "SFXVolume";
    private const string PK_LAST_MUSIC = "LastMusicVolume";
    private const string PK_LAST_SFX = "LastSFXVolume";
    private const string PK_MUTED = "IsMuted";

    // ================= UNITY =================

    private void Start()
    {
        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;

        if (musicSourceA != null) musicSourceA.loop = true;
        if (musicSourceB != null) musicSourceB.loop = true;

        LoadAudioSettings();
    }

    // ================= VOLUME API =================

    public void SetMusicVolume(float value)
    {
        musicVolumeMultiplier = Mathf.Clamp01(value);
        if (musicVolumeMultiplier > 0f) lastNonZeroMusicVolume = musicVolumeMultiplier;

        ApplyMusicVolumeToSource();
        AutoDetectMuteState();
        SaveAudioSettings();
        OnAudioSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolumeMultiplier = Mathf.Clamp01(value);
        if (sfxVolumeMultiplier > 0f) lastNonZeroSfxVolume = sfxVolumeMultiplier;

        ApplySfxVolumeToSource();
        AutoDetectMuteState();
        SaveAudioSettings();
        OnAudioSettingsChanged?.Invoke();
    }

    public void ToggleMute()
    {
        if (!isMuted)
        {
            if (musicVolumeMultiplier > 0f) lastNonZeroMusicVolume = musicVolumeMultiplier;
            if (sfxVolumeMultiplier > 0f) lastNonZeroSfxVolume = sfxVolumeMultiplier;

            SetMusicVolume(0f);
            SetSFXVolume(0f);
            isMuted = true;
        }
        else
        {
            float restoreMusic = lastNonZeroMusicVolume > 0f ? lastNonZeroMusicVolume : 0.7f;
            float restoreSfx = lastNonZeroSfxVolume > 0f ? lastNonZeroSfxVolume : 0.7f;

            SetMusicVolume(restoreMusic);
            SetSFXVolume(restoreSfx);
            isMuted = false;
        }

        SaveAudioSettings();
        OnAudioSettingsChanged?.Invoke();
    }

    // Sliders dragged to/from zero implicitly toggle the mute flag.
    private void AutoDetectMuteState()
    {
        if (musicVolumeMultiplier > 0f && sfxVolumeMultiplier > 0f) isMuted = false;
        else if (musicVolumeMultiplier <= 0f && sfxVolumeMultiplier <= 0f) isMuted = true;
    }

    private void ApplyMusicVolumeToSource()
    {
        float targetVol = masterMusicVolume * currentTrackVolume * musicVolumeMultiplier;
        if (activeMusicSource != null && activeMusicSource.isPlaying)
            activeMusicSource.volume = targetVol;
    }

    private void ApplySfxVolumeToSource()
    {
        if (specialLoopSource != null && specialLoopSource.isPlaying)
            specialLoopSource.volume = specialLoopVolume * sfxVolumeMultiplier;
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(PK_MUSIC, musicVolumeMultiplier);
        PlayerPrefs.SetFloat(PK_SFX, sfxVolumeMultiplier);
        PlayerPrefs.SetFloat(PK_LAST_MUSIC, lastNonZeroMusicVolume);
        PlayerPrefs.SetFloat(PK_LAST_SFX, lastNonZeroSfxVolume);
        PlayerPrefs.SetInt(PK_MUTED, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        musicVolumeMultiplier = Mathf.Clamp01(PlayerPrefs.GetFloat(PK_MUSIC, 1f));
        sfxVolumeMultiplier = Mathf.Clamp01(PlayerPrefs.GetFloat(PK_SFX, 1f));
        lastNonZeroMusicVolume = PlayerPrefs.GetFloat(PK_LAST_MUSIC, 0.7f);
        lastNonZeroSfxVolume = PlayerPrefs.GetFloat(PK_LAST_SFX, 0.7f);
        isMuted = PlayerPrefs.GetInt(PK_MUTED, 0) == 1;

        if (isMuted)
        {
            musicVolumeMultiplier = 0f;
            sfxVolumeMultiplier = 0f;
        }

        ApplyMusicVolumeToSource();
        ApplySfxVolumeToSource();

        OnAudioSettingsChanged?.Invoke();
    }

    public float GetMusicVolume() =>
        PlayerPrefs.GetFloat(PK_MUSIC, 1f);

    public float GetSFXVolume() =>
        PlayerPrefs.GetFloat(PK_SFX, 1f);

    private float ComputeMusicVolume(float trackVolume)
    {
        return masterMusicVolume * trackVolume * musicVolumeMultiplier;
    }

    // ================= FULLSCREEN =================

    public static void SetFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
    }

    public static bool GetFullscreen() =>
        PlayerPrefs.GetInt("Fullscreen", 0) == 1;

    // ================= MUSIC API =================

    public void PlayMenuMusic()
    {
        CrossfadeToClip(menuMusicClip, menuMusicVolume);
    }

    public void PlayMusicForRank(RankData rank)
    {
        if (rank == null || rank.backgroundMusic == null)
            return;

        CrossfadeToClip(rank.backgroundMusic, rank.backgroundMusicVolume);
    }

    public void StopMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        if (activeMusicSource != null) activeMusicSource.Stop();
        if (inactiveMusicSource != null) inactiveMusicSource.Stop();
        currentMusic = null;
    }

    private void CrossfadeToClip(AudioClip clip, float trackVolume)
    {
        if (clip == null) return;
        if (clip == currentMusic) return;

        currentMusic = clip;
        currentTrackVolume = trackVolume;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(clip, trackVolume));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float trackVolume)
    {
        float targetVol = ComputeMusicVolume(trackVolume);

        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.loop = true;
        inactiveMusicSource.Play();

        float t = 0f;
        float startVol = activeMusicSource.isPlaying ? activeMusicSource.volume : 0f;

        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / crossfadeDuration);

            if (activeMusicSource.isPlaying)
                activeMusicSource.volume = Mathf.Lerp(startVol, 0f, progress);

            inactiveMusicSource.volume = Mathf.Lerp(0f, targetVol, progress);

            yield return null;
        }

        activeMusicSource.Stop();
        activeMusicSource.volume = 0f;
        inactiveMusicSource.volume = targetVol;

        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;

        crossfadeCoroutine = null;
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

    public void PlayPopup() => PlayOneShot(popupClip, popupVolume);
    public void PlayPopupSwosh() => PlayOneShot(popupSwoshClip, popupSwoshVolume);
    public void PlayToast() => PlayOneShot(toastClip, toastVolume);

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
        specialLoopSource.volume = specialLoopVolume * sfxVolumeMultiplier;
        specialLoopSource.loop = true;
        specialLoopSource.Play();
    }

    public void StopSpecialLoop()
    {
        if (specialLoopSource == null)
            return;

        specialLoopSource.Stop();
    }

    // ================= PUBLIC SFX =================

    public void PlaySfxClip(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volume * sfxVolumeMultiplier);
    }

    public AudioClip StartSceneClip => startSceneClip;

    // ================= INTERNAL =================

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, volume * sfxVolumeMultiplier);
    }
}

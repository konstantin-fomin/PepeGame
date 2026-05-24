// AudioManager.cs
// Version: 2026-05-24 v1.2 (Separated loop lifecycle from one-shots)

using UnityEngine;

public class AudioManager : MonoBehaviour
{
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

    [Header("SFX - Buy")]
    [SerializeField] private AudioClip buyClip;
    [Range(0f, 1f)][SerializeField] private float buyVolume = 0.7f;

    [Header("SFX - Error")]
    [SerializeField] private AudioClip errorClip;
    [Range(0f, 1f)][SerializeField] private float errorVolume = 0.6f;

    [Header("SFX - Rank Up")]
    [SerializeField] private AudioClip rankUpClip;
    [Range(0f, 1f)][SerializeField] private float rankUpVolume = 1f;

    [Header("Special Start / End")]
    [SerializeField] private AudioClip specialStartClip;
    [Range(0f, 1f)][SerializeField] private float specialStartVolume = 0.8f;

    [SerializeField] private AudioClip specialEndClip;
    [Range(0f, 1f)][SerializeField] private float specialEndVolume = 0.6f;

    [Header("Special Loop")]
    [SerializeField] private AudioClip specialLoopClip;
    [Range(0f, 1f)][SerializeField] private float specialLoopVolume = 0.4f;

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
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    // ================= SFX API =================

    public void PlayClick() => PlayOneShot(clickClip, clickVolume);
    public void PlayBuy() => PlayOneShot(buyClip, buyVolume);
    public void PlayError() => PlayOneShot(errorClip, errorVolume);
    public void PlayRankUp() => PlayOneShot(rankUpClip, rankUpVolume);

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

        sfxSource.PlayOneShot(clip, volume);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backButton;

    private bool subscribed;

    private void OnEnable()
    {
        Subscribe();
        SyncFromAudio();
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = AudioManager.GetFullscreen();
    }

    private void OnDisable()
    {
        if (subscribed && audioManager != null)
            audioManager.OnAudioSettingsChanged -= SyncFromAudio;
        subscribed = false;
    }

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(audioManager.SetSFXVolume);
        fullscreenToggle.onValueChanged.AddListener(AudioManager.SetFullscreen);
        backButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.BackFromSettings());

        Subscribe();
        SyncFromAudio();
    }

    private void Subscribe()
    {
        if (subscribed || audioManager == null) return;
        audioManager.OnAudioSettingsChanged += SyncFromAudio;
        subscribed = true;
    }

    // Pull current volumes into the sliders without re-triggering onValueChanged
    // (prevents an event feedback loop between the slider and AudioManager).
    private void SyncFromAudio()
    {
        if (audioManager == null) return;
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(audioManager.MusicVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
    }
}

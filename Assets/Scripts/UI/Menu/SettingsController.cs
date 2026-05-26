using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backButton;

    private void OnEnable()
    {
        musicSlider.value    = audioManager.GetMusicVolume();
        sfxSlider.value      = audioManager.GetSFXVolume();
        fullscreenToggle.isOn = AudioManager.GetFullscreen();
    }

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(audioManager.SetSFXVolume);
        fullscreenToggle.onValueChanged.AddListener(AudioManager.SetFullscreen);
        backButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.BackFromSettings());
    }
}

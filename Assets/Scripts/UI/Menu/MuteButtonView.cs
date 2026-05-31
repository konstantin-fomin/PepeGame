// MuteButtonView.cs
// Version: 2026-05-31 v1.0
// Purpose: Main-menu mute toggle. Swaps icon sprite on mute state; falls back to a
//          color tint when no sound-on/off sprites are assigned (art not yet authored).

using UnityEngine;
using UnityEngine.UI;

public class MuteButtonView : MonoBehaviour
{
    [SerializeField] private Button muteButton;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    // Tint fallback used when sprites are missing (so state is still visible).
    [SerializeField] private Color onTint  = Color.white;
    [SerializeField] private Color offTint = new Color(0.55f, 0.55f, 0.55f, 1f);

    private bool subscribed;

    private void Start()
    {
        if (muteButton != null)
        {
            muteButton.onClick.RemoveListener(OnMuteClicked);
            muteButton.onClick.AddListener(OnMuteClicked);
        }

        Subscribe();
        UpdateIcon();
    }

    private void OnDestroy()
    {
        if (subscribed && AudioManager.Instance != null)
            AudioManager.Instance.OnAudioSettingsChanged -= UpdateIcon;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed || AudioManager.Instance == null) return;
        AudioManager.Instance.OnAudioSettingsChanged += UpdateIcon;
        subscribed = true;
    }

    private void OnMuteClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ToggleMute();
    }

    private void UpdateIcon()
    {
        if (buttonIcon == null || AudioManager.Instance == null) return;

        bool muted = AudioManager.Instance.IsMuted;
        Sprite target = muted ? soundOffSprite : soundOnSprite;

        if (target != null)
        {
            // Dedicated sprites already distinguish the state — no tint/darkening.
            buttonIcon.sprite = target;
            buttonIcon.color = Color.white;
        }
        else
        {
            // Tint fallback only when no art is assigned.
            buttonIcon.color = muted ? offTint : onTint;
        }
    }
}

// LanguageSelectorView.cs
// Version: 2026-05-31 v1.1 (active/inactive background sprite + label tint)
// Purpose: RU | EN toggle. Active language = pressed sprite + yellow label,
//          inactive = normal sprite + grey label. Routes clicks to LocalizationManager.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LanguageSelectorView : MonoBehaviour
{
    [SerializeField] private Button ruButton;
    [SerializeField] private Button enButton;
    [SerializeField] private TMP_Text ruLabel;
    [SerializeField] private TMP_Text enLabel;

    [Header("Button sprites")]
    [SerializeField] private Sprite buttonNormalSprite;   // leng_button
    [SerializeField] private Sprite buttonPressedSprite;  // leng_button_pressed (active language)

    private static readonly Color ActiveColor   = new Color(0.8392f, 0.7216f, 0.2902f, 1f); // #D6B84A
    private static readonly Color InactiveColor = new Color(0.9020f, 0.8784f, 0.8235f, 1f); // #E6E0D2

    private bool subscribed;

    private void OnEnable()
    {
        Subscribe();
        UpdateVisual();
    }

    private void Start()
    {
        if (ruButton != null)
        {
            ruButton.onClick.RemoveListener(OnRuClicked);
            ruButton.onClick.AddListener(OnRuClicked);
        }
        if (enButton != null)
        {
            enButton.onClick.RemoveListener(OnEnClicked);
            enButton.onClick.AddListener(OnEnClicked);
        }
        Subscribe();
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (subscribed && LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateVisual;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (subscribed) return;
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateVisual;
            subscribed = true;
        }
    }

    private void OnRuClicked() => SetLanguage(LocalizationManager.Language.RU);
    private void OnEnClicked() => SetLanguage(LocalizationManager.Language.EN);

    private void SetLanguage(LocalizationManager.Language lang)
    {
        if (LocalizationManager.Instance == null) return;
        LocalizationManager.Instance.SetLanguage(lang);
        Subscribe();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (LocalizationManager.Instance == null) return;
        bool ruActive = LocalizationManager.Instance.CurrentLanguage == LocalizationManager.Language.RU;

        ApplyButton(ruButton, ruLabel, ruActive);
        ApplyButton(enButton, enLabel, !ruActive);
    }

    private void ApplyButton(Button btn, TMP_Text label, bool active)
    {
        if (label != null)
            label.color = active ? ActiveColor : InactiveColor;

        if (btn == null) return;
        Image img = btn.image;
        if (img == null) return;

        Sprite s = active ? buttonPressedSprite : buttonNormalSprite;
        img.sprite = s;
        // Visible when a sprite is set; transparent (but still clickable) otherwise.
        img.color = (s != null) ? Color.white : new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = true;
    }
}

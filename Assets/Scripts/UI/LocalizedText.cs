// LocalizedText.cs
// Version: 2026-05-31 v1.0
// Purpose: Binds a TMP_Text to a localization key; refreshes on language change.

using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] public string localizationKey;

    private TMP_Text target;
    private bool subscribed;

    private void Awake()
    {
        target = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Subscribe();
        UpdateText();
    }

    private void Start()
    {
        Subscribe();
        UpdateText();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (subscribed) return;
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            subscribed = true;
        }
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        subscribed = false;
    }

    public void UpdateText()
    {
        if (target == null) target = GetComponent<TMP_Text>();
        if (target == null) return;
        if (string.IsNullOrEmpty(localizationKey)) return;
        if (LocalizationManager.Instance == null) return;

        target.text = LocalizationManager.Instance.Get(localizationKey);
    }
}

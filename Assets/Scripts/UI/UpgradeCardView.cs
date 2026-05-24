// UpgradeCardView.cs
// Version: 2026-05-24 v1.3 (Special duration in effect text)
// Purpose: UI view for upgrade card + money overlay

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text priceText;

    [Header("Icon")]
    [SerializeField] private Image iconImage;

    [Header("Interaction")]
    [SerializeField] private Button button;

    [Header("Overlay (No Money)")]
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject lockIcon;

    // ================= VALIDATION =================

    private void Awake()
    {
        bool valid = true;

        if (titleText == null)       { Debug.LogError("[UpgradeCard] titleText not assigned", this);       valid = false; }
        if (descriptionText == null) { Debug.LogError("[UpgradeCard] descriptionText not assigned", this); valid = false; }
        if (effectText == null)      { Debug.LogError("[UpgradeCard] effectText not assigned", this);      valid = false; }
        if (priceText == null)       { Debug.LogError("[UpgradeCard] priceText not assigned", this);       valid = false; }
        if (iconImage == null)       { Debug.LogError("[UpgradeCard] iconImage not assigned", this);       valid = false; }
        if (button == null)          { Debug.LogError("[UpgradeCard] button not assigned", this);          valid = false; }

        if (!valid) enabled = false;
    }

    // ================= PUBLIC API =================

    public void SetUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            Clear();
            return;
        }

        titleText.text = upgrade.title.ToUpper();
        descriptionText.text = upgrade.description;
        effectText.text = BuildEffectText(upgrade);
        priceText.text = upgrade.basePrice.ToString();
    }

    public void SetIcon(Sprite sprite)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
    }

    /// <summary>
    /// Отвечает ТОЛЬКО за кликабельность
    /// </summary>
    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }

    /// <summary>
    /// Затемнение ТОЛЬКО если не хватает денег
    /// </summary>
    public void SetNoMoneyOverlay(bool show)
    {
        if (darkOverlay != null)
            darkOverlay.SetActive(show);

        if (lockIcon != null)
            lockIcon.SetActive(show);
    }

    public void SetClickAction(UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    // ================= INTERNAL =================

    private void Clear()
    {
        titleText.text = "—";
        descriptionText.text = "";
        effectText.text = "";
        priceText.text = "";

        iconImage.enabled = false;
        button.interactable = false;

        if (darkOverlay != null)
            darkOverlay.SetActive(true);
    }

    private string BuildEffectText(Upgrade upgrade)
    {
        string bonus = "";

        if (upgrade.clickBonus > 0)
            bonus = $"+{upgrade.clickBonus} KPI / click";
        else if (upgrade.passiveBonus > 0)
            bonus = $"+{upgrade.passiveBonus} KPI / sec";

        if (upgrade.specialDuration > 0f)
        {
            string duration = $"{Mathf.RoundToInt(upgrade.specialDuration)}s";
            return string.IsNullOrEmpty(bonus) ? duration : $"{bonus} • {duration}";
        }

        return bonus;
    }
}

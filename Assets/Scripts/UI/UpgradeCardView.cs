// UpgradeCardView.cs
// Version: 2026-01-16
// Purpose: UI view for upgrade card

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

    // ================= PUBLIC API =================

    public void SetUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            Clear();
            return;
        }

        titleText.text = upgrade.title;
        descriptionText.text = upgrade.description;

        effectText.text = BuildEffectText(upgrade);
        priceText.text = upgrade.basePrice.ToString();
    }

    public void SetIcon(Sprite sprite)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
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
    }

    private string BuildEffectText(Upgrade upgrade)
    {
        if (upgrade.clickBonus > 0)
            return $"+{upgrade.clickBonus} KPI / click";

        if (upgrade.passiveBonus > 0)
            return $"+{upgrade.passiveBonus} KPI / sec";

        return "";
    }
}

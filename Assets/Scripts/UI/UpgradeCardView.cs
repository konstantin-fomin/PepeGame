// UpgradeCardView.cs
// Version: 2026-05-30 v1.5 (completed overlay layout enforced in code)
// Purpose: UI view for upgrade card + money overlay + completed state

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

    [Header("Completed Overlay")]
    [SerializeField] private GameObject completedOverlay;
    [SerializeField] private TMP_Text completedTitleText;
    [SerializeField] private TMP_Text completedDescriptionText;
    [SerializeField] private TMP_Text completedMaxText;

    [Header("Content Groups")]
    [SerializeField] private GameObject iconContainer;
    [SerializeField] private GameObject priceButton;

    private bool isCompleted = false;

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

        ConfigureCompletedOverlayLayout();
    }

    /// <summary>
    /// Карточка использует VerticalLayoutGroup, который иначе раскладывает
    /// CompletedOverlay как элемент стека и ломает позицию текста (MAX уезжает
    /// за карточку). Принудительно выводим оверлей из-под layout group и
    /// растягиваем на всю карточку — единообразно для всех префабов карточек.
    /// </summary>
    private void ConfigureCompletedOverlayLayout()
    {
        if (completedOverlay == null) return;

        RectTransform rt = completedOverlay.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        LayoutElement le = completedOverlay.GetComponent<LayoutElement>();
        if (le == null)
            le = completedOverlay.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    // ================= PUBLIC API =================

    public void SetUpgrade(Upgrade upgrade)
    {
        if (upgrade == null)
        {
            Clear();
            return;
        }

        ExitCompletedState();

        titleText.text = upgrade.title.ToUpper();
        descriptionText.text = upgrade.description;
        effectText.text = BuildEffectText(upgrade);
        priceText.text = NumberFormatter.Format(upgrade.basePrice);
    }

    public void SetCompleted()
    {
        isCompleted = true;

        // Hide normal content
        titleText.text = "";
        descriptionText.text = "";
        effectText.text = "";
        priceText.text = "";
        iconImage.enabled = false;

        if (iconContainer != null)
            iconContainer.SetActive(false);

        if (priceButton != null)
            priceButton.SetActive(false);

        // Disable interaction
        button.interactable = false;

        // Hide no-money overlay
        if (darkOverlay != null)
            darkOverlay.SetActive(false);

        if (lockIcon != null)
            lockIcon.SetActive(false);

        // Show completed overlay
        if (completedOverlay != null)
            completedOverlay.SetActive(true);

        if (completedTitleText != null)
            completedTitleText.text = "ПЛАН ВЫПОЛНЕН";

        if (completedDescriptionText != null)
            completedDescriptionText.text = "Больше задач не завезли.";

        if (completedMaxText != null)
            completedMaxText.text = "MAX";
    }

    public bool IsCompleted => isCompleted;

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
        if (isCompleted) return;
        button.interactable = value;
    }

    /// <summary>
    /// Затемнение ТОЛЬКО если не хватает денег
    /// </summary>
    public void SetNoMoneyOverlay(bool show)
    {
        if (isCompleted) return;

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

    private void ExitCompletedState()
    {
        if (!isCompleted) return;
        isCompleted = false;

        if (completedOverlay != null)
            completedOverlay.SetActive(false);

        if (iconContainer != null)
            iconContainer.SetActive(true);

        if (priceButton != null)
            priceButton.SetActive(true);
    }

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
        // Special cards: "+3 click • +8/sec • 10s"
        if (upgrade.specialDuration > 0f)
        {
            var parts = new System.Collections.Generic.List<string>();

            if (upgrade.clickBonus > 0)
                parts.Add($"+{NumberFormatter.Format(upgrade.clickBonus)} click");

            if (upgrade.passiveBonus > 0)
                parts.Add($"+{NumberFormatter.Format(upgrade.passiveBonus)}/sec");

            parts.Add($"{Mathf.RoundToInt(upgrade.specialDuration)}s");

            return string.Join(" • ", parts.ToArray());
        }

        // Click / Passive cards
        if (upgrade.clickBonus > 0)
            return $"+{NumberFormatter.Format(upgrade.clickBonus)} KPI / click";

        if (upgrade.passiveBonus > 0)
            return $"+{NumberFormatter.Format(upgrade.passiveBonus)} KPI / sec";

        return "";
    }
}

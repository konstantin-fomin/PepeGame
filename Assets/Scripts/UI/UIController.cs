// UIController.cs
// Version: 2026-01-16 v2.5 (No-money overlay support)

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    // ================= CORE =================
    [Header("Core")]
    [SerializeField] private GameManager gameManager;

    // ================= KPI =================
    [Header("KPI")]
    [SerializeField] private TMP_Text kpiText;
    [SerializeField] private TMP_Text kpiPerSecondText;

    // ================= RANK =================
    [Header("Rank")]
    [SerializeField] private TMP_Text rankText;

    // ================= PROGRESS =================
    [Header("Progress")]
    [SerializeField] private Slider rankProgressBar;
    [SerializeField] private TMP_Text rankProgressText;

    // ================= UPGRADE CARDS =================
    [Header("Upgrade Cards")]
    [SerializeField] private UpgradeCardView clickCard;
    [SerializeField] private UpgradeCardView passiveCard;
    [SerializeField] private UpgradeCardView specialCard;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[UI] GameManager reference missing!");
            enabled = false;
        }
    }

    private void Update()
    {
        RefreshUI();
    }

    // ================= UI REFRESH =================

    private void RefreshUI()
    {
        RefreshKpi();
        RefreshRank();
        RefreshProgress();

        RefreshCard(SlotType.Click, clickCard);
        RefreshCard(SlotType.Passive, passiveCard);
        RefreshCard(SlotType.Special, specialCard);
    }

    // ================= KPI =================

    private void RefreshKpi()
    {
        kpiText.text = $"KPI: {gameManager.CurrentKpi}";
        kpiPerSecondText.text = $"+{gameManager.KpiPerSecond} KPI / сек";
    }

    // ================= RANK =================

    private void RefreshRank()
    {
        rankText.text = gameManager.CurrentRank.rankName;
    }

    // ================= PROGRESS =================

    private void RefreshProgress()
    {
        int currentXp = gameManager.CurrentExperience;
        int requiredXp = gameManager.ExperienceToNextRank;

        if (requiredXp <= 0)
        {
            rankProgressBar.value = 1f;
            rankProgressText.text = "MAX";
            return;
        }

        float progress01 = Mathf.Clamp01((float)currentXp / requiredXp);
        rankProgressBar.value = progress01;
        rankProgressText.text = $"{currentXp} / {requiredXp}";
    }

    // ================= UPGRADE CARD =================

    private void RefreshCard(SlotType slotType, UpgradeCardView card)
    {
        Upgrade upgrade = gameManager.GetCurrentUpgrade(slotType);

        card.SetUpgrade(upgrade);

        // ❗ карточка ВСЕГДА кликабельна, если есть апгрейд
        card.SetInteractable(upgrade != null);

        // 🔒 затемнение ТОЛЬКО если апгрейд есть, но денег не хватает
        bool noMoney =
            upgrade != null &&
            !gameManager.CanBuyUpgrade(slotType);

        card.SetNoMoneyOverlay(noMoney);

        card.SetClickAction(() =>
        {
            gameManager.TryBuyUpgrade(slotType);
        });
    }
}

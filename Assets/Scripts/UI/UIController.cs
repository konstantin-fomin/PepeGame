// UIController.cs
// Version: 2026-01-12 v2.0 (Experience UI)
// Author: ChatGPT + Kostya

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

    // ================= SLOTS =================
    [Header("Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private TMP_Text slot1Text;

    [SerializeField] private Button slot2Button;
    [SerializeField] private TMP_Text slot2Text;

    [SerializeField] private Button slot3Button;
    [SerializeField] private TMP_Text slot3Text;

    // ================= UNITY =================

    private void Awake()
    {
        Debug.Log("[UI] Awake");

        if (gameManager == null)
        {
            Debug.LogError("[UI] GameManager reference missing!");
            enabled = false;
            return;
        }

        BindButtons();
    }

    private void Update()
    {
        RefreshUI();
    }

    // ================= BUTTON BINDING =================

    private void BindButtons()
    {
        slot1Button.onClick.RemoveAllListeners();
        slot2Button.onClick.RemoveAllListeners();
        slot3Button.onClick.RemoveAllListeners();

        slot1Button.onClick.AddListener(() => OnUpgradeClicked(SlotType.Click));
        slot2Button.onClick.AddListener(() => OnUpgradeClicked(SlotType.Passive));
        slot3Button.onClick.AddListener(() => OnUpgradeClicked(SlotType.Special));
    }

    private void OnUpgradeClicked(SlotType slotType)
    {
        gameManager.TryBuyUpgrade(slotType);
    }

    // ================= UI REFRESH =================

    private void RefreshUI()
    {
        RefreshKpi();
        RefreshRank();
        RefreshProgress();
        RefreshSlots();
    }

    // ================= KPI UI =================

    private void RefreshKpi()
    {
        kpiText.text = $"KPI: {gameManager.CurrentKpi}";
        kpiPerSecondText.text = $"+{gameManager.KpiPerSecond} KPI / сек";
    }

    // ================= RANK UI =================

    private void RefreshRank()
    {
        rankText.text = gameManager.CurrentRank.rankName;
    }

    // ================= PROGRESS UI =================

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

    // ================= SLOTS UI =================

    private void RefreshSlots()
    {
        RefreshSlot(
            SlotType.Click,
            slot1Button,
            slot1Text
        );

        RefreshSlot(
            SlotType.Passive,
            slot2Button,
            slot2Text
        );

        RefreshSlot(
            SlotType.Special,
            slot3Button,
            slot3Text
        );
    }

    private void RefreshSlot(
        SlotType slotType,
        Button button,
        TMP_Text text
    )
    {
        Upgrade upg = gameManager.GetCurrentUpgrade(slotType);

        if (upg == null)
        {
            text.text = "—";
            button.interactable = false;
            return;
        }

        string label =
            $"{upg.title}\n" +
            $"{upg.description}\n" +
            $"Цена: {upg.basePrice}";

        // ===== SPECIAL TIMER =====
        if (upg.specialDuration > 0 &&
            gameManager.TryGetActiveSpecial(upg.id, out float timeLeft))
        {
            label += $"\n⏱ {Mathf.CeilToInt(timeLeft)} сек";
        }

        text.text = label;
        button.interactable = gameManager.CanBuyUpgrade(slotType);
    }
}

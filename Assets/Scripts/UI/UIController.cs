using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private GameManager gameManager;

    [Header("KPI")]
    [SerializeField] private TMP_Text kpiText;
    [SerializeField] private TMP_Text kpiPerSecondText;
    [SerializeField] private TMP_Text rankText;

    [Header("Progress")]
    [SerializeField] private Slider rankProgress;
    [SerializeField] private TMP_Text rankProgressText;

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
            Debug.LogError("[UI] GameManager NOT SET");
            return;
        }

        // 👇 ВАЖНО: подписываемся НАЖАТИЯМИ ТОЛЬКО ТУТ
        slot1Button.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Click Slot 1");
            gameManager.TryBuyUpgrade(SlotType.Click);
        });

        slot2Button.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Click Slot 2");
            gameManager.TryBuyUpgrade(SlotType.Passive);
        });

        slot3Button.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Click Slot 3");
            gameManager.TryBuyUpgrade(SlotType.Special);
        });
    }

    private void Update()
    {
        RefreshTop();
        RefreshProgress();

        RefreshSlot(SlotType.Click, slot1Button, slot1Text);
        RefreshSlot(SlotType.Passive, slot2Button, slot2Text);
        RefreshSlot(SlotType.Special, slot3Button, slot3Text);
    }

    // ================= TOP =================

    private void RefreshTop()
    {
        kpiText.text = $"KPI: {gameManager.CurrentKpi}";
        kpiPerSecondText.text = $"+{gameManager.KpiPerSecond} KPI / сек";
        rankText.text = gameManager.CurrentRank.rankName;
    }

    // ================= PROGRESS =================

    private void RefreshProgress()
    {
        int current = gameManager.KpiInCurrentRank;
        int target = gameManager.KpiToNextRank;

        if (target <= 0)
        {
            rankProgress.gameObject.SetActive(false);
            rankProgressText.text = "";
            return;
        }

        rankProgress.gameObject.SetActive(true);
        rankProgress.maxValue = target;
        rankProgress.value = current;

        rankProgressText.text = $"{current} / {target}";
    }

    // ================= SLOT =================

    private void RefreshSlot(
        SlotType slotType,
        Button button,
        TMP_Text text)
    {
        Upgrade upg = gameManager.GetCurrentUpgrade(slotType);

        if (upg == null)
        {
            text.text = "—";
            button.interactable = false;
            return;
        }

        // ===== ACTIVE SPECIAL =====
        if (slotType == SlotType.Special &&
            upg.specialDuration > 0 &&
            gameManager.TryGetActiveSpecial(upg.id, out float timeLeft))
        {
            text.text =
                $"{upg.title}\n" +
                $"{upg.description}\n" +
                $"⏳ {Mathf.CeilToInt(timeLeft)} сек";

            button.interactable = false;
            return;
        }

        // ===== NORMAL =====
        text.text =
            $"{upg.title}\n" +
            $"{upg.description}\n" +
            $"Цена: {upg.basePrice}";

        button.interactable = gameManager.CanBuyUpgrade(slotType);
    }
}

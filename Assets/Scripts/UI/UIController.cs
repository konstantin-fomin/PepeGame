// UIController.cs
// Version: 2026-05-30 v4.0 (career completion)

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

    // ================= CARD ANIMATORS =================
    [Header("Card Animators")]
    [SerializeField] private CardBuyAnimator clickAnimator;
    [SerializeField] private CardBuyAnimator passiveAnimator;
    [SerializeField] private CardBuyAnimator specialAnimator;

    // ================= RANK UP =================
    [Header("Rank Up")]
    [SerializeField] private RankUpPopupView rankUpPopup;

    public RankUpPopupView RankUpPopup => rankUpPopup;

    // ================= CAREER COMPLETED =================
    [Header("Career Completed")]
    [SerializeField] private CareerCompletedScreenView careerCompletedScreen;

    // ================= SPECIAL EFFECTS =================

    [Header("Pause")]
    [SerializeField] private Button pauseButton;

    [Header("Special Effects")]
    [SerializeField] private Image specialTimerBar;
    [SerializeField] private Image specialVignette;
    [SerializeField] private Color vignetteColor = new Color(0.8f, 0.5f, 0.1f, 1f);

    private void Awake()
    {
        bool valid = true;

        if (gameManager == null)     { Debug.LogError("[UI] gameManager not assigned", this);     valid = false; }
        if (kpiText == null)         { Debug.LogError("[UI] kpiText not assigned", this);         valid = false; }
        if (kpiPerSecondText == null) { Debug.LogError("[UI] kpiPerSecondText not assigned", this); valid = false; }
        if (rankText == null)        { Debug.LogError("[UI] rankText not assigned", this);        valid = false; }
        if (rankProgressBar == null) { Debug.LogError("[UI] rankProgressBar not assigned", this); valid = false; }
        if (rankProgressText == null) { Debug.LogError("[UI] rankProgressText not assigned", this); valid = false; }
        if (clickCard == null)       { Debug.LogError("[UI] clickCard not assigned", this);       valid = false; }
        if (passiveCard == null)     { Debug.LogError("[UI] passiveCard not assigned", this);     valid = false; }
        if (specialCard == null)     { Debug.LogError("[UI] specialCard not assigned", this);     valid = false; }

        if (!valid) enabled = false;
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged += RefreshUI;
            gameManager.OnRankUp += rankUpPopup.Show;
            gameManager.OnCareerCompleted += HandleCareerCompleted;
        }

        if (careerCompletedScreen != null)
        {
            careerCompletedScreen.OnStayCeo += HandleStayCeo;
            careerCompletedScreen.OnNewCareerConfirmed += HandleNewCareer;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= RefreshUI;
            gameManager.OnRankUp -= rankUpPopup.Show;
            gameManager.OnCareerCompleted -= HandleCareerCompleted;
        }

        if (careerCompletedScreen != null)
        {
            careerCompletedScreen.OnStayCeo -= HandleStayCeo;
            careerCompletedScreen.OnNewCareerConfirmed -= HandleNewCareer;
        }
    }

    private void Start()
    {
        clickCard.SetClickAction(() =>
        {
            int cost = gameManager.GetCurrentUpgrade(SlotType.Click)?.basePrice ?? 0;
            bool canBuy = gameManager.CanBuyUpgrade(SlotType.Click);
            gameManager.TryBuyUpgrade(SlotType.Click);
            if (canBuy) clickAnimator?.PlayBuyAnimation(cost);
            else        clickAnimator?.PlayErrorAnimation();
        });

        passiveCard.SetClickAction(() =>
        {
            int cost = gameManager.GetCurrentUpgrade(SlotType.Passive)?.basePrice ?? 0;
            bool canBuy = gameManager.CanBuyUpgrade(SlotType.Passive);
            gameManager.TryBuyUpgrade(SlotType.Passive);
            if (canBuy) passiveAnimator?.PlayBuyAnimation(cost);
            else        passiveAnimator?.PlayErrorAnimation();
        });

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() =>
                MenuNavigationController.Instance.ShowPause());

        specialCard.SetClickAction(() =>
        {
            int cost = gameManager.GetCurrentUpgrade(SlotType.Special)?.basePrice ?? 0;
            bool canBuy = gameManager.CanBuyUpgrade(SlotType.Special);
            gameManager.TryBuyUpgrade(SlotType.Special);
            if (canBuy) specialAnimator?.PlayBuyAnimation(cost);
            else        specialAnimator?.PlayErrorAnimation();
        });

        RefreshUI();
    }

    private void Update()
    {
        UpdateSpecialEffects();
    }

    // ================= CAREER COMPLETED =================

    private void HandleCareerCompleted()
    {
        if (gameManager.HasSeenCareerCompletedScreen) return;
        if (careerCompletedScreen == null) return;

        var stats = StatsTracker.Instance;
        long totalKpi = stats != null ? stats.TotalKpiEarned : 0;
        float totalTime = stats != null ? stats.TotalPlaytimeSeconds : 0f;
        int totalClicks = stats != null ? stats.TotalClicks : 0;

        careerCompletedScreen.Show(totalKpi, totalTime, totalClicks);
    }

    private void HandleStayCeo()
    {
        gameManager.MarkCareerScreenSeen();
    }

    private void HandleNewCareer()
    {
        gameManager.ResetProgress();
        MenuNavigationController.Instance.ShowMainMenu(false);
        MenuNavigationController.Instance.StartGameWithZoom(false);
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
        kpiText.text = NumberFormatter.Format(gameManager.CurrentKpi);
        kpiPerSecondText.text = $"+{NumberFormatter.Format(gameManager.KpiPerSecond)} KPI / сек";
    }

    // ================= RANK =================

    private void RefreshRank()
    {
        if (gameManager.CurrentRank == null) return;
        rankText.text = $"<b>{gameManager.CurrentRank.rankName.ToUpper()}</b>";
    }

    // ================= PROGRESS =================

    private void RefreshProgress()
    {
        if (gameManager.IsCareerCompleted)
        {
            rankProgressBar.value = 1f;
            rankProgressText.text = "MAX";
            return;
        }

        long currentXp = gameManager.CurrentExperience;
        long requiredXp = gameManager.ExperienceToNextRank;

        if (requiredXp <= 0)
        {
            rankProgressBar.value = 1f;
            rankProgressText.text = "MAX";
            return;
        }

        float progress01 = Mathf.Clamp01((float)currentXp / requiredXp);
        rankProgressBar.value = progress01;
        rankProgressText.text = $"<color=#FFB800>({NumberFormatter.Format(currentXp)} / {NumberFormatter.Format(requiredXp)})</color>";
    }

    // ================= UPGRADE CARD =================

    private void RefreshCard(SlotType slotType, UpgradeCardView card)
    {
        Upgrade upgrade = gameManager.GetCurrentUpgrade(slotType);

        if (upgrade == null)
        {
            card.SetCompleted();
            return;
        }

        card.SetUpgrade(upgrade);
        card.SetIcon(upgrade.icon);
        card.SetInteractable(true);

        bool noMoney = !gameManager.CanBuyUpgrade(slotType);
        card.SetNoMoneyOverlay(noMoney);
    }

    // ================= SPECIAL EFFECTS =================

    private void UpdateSpecialEffects()
    {
        if (gameManager == null) return;

        bool hasSpecial = gameManager.HasActiveSpecial;
        float progress = hasSpecial ? gameManager.GetSpecialProgress01() : 0f;

        UpdateTimerBar(hasSpecial, progress);
        UpdateVignette(hasSpecial, progress);
    }

    private bool wasSpecialActive = false;

    private void UpdateTimerBar(bool hasSpecial, float progress)
    {
        RectTransform rt = specialTimerBar.rectTransform;

        if (!hasSpecial)
        {
            specialTimerBar.gameObject.SetActive(false);
            rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
            wasSpecialActive = false;
            return;
        }

        if (!wasSpecialActive)
        {
            rt.anchorMax = new Vector2(1f, rt.anchorMax.y);
            wasSpecialActive = true;
        }

        specialTimerBar.gameObject.SetActive(true);
        float t = Mathf.Clamp01(progress);
        rt.anchorMax = new Vector2(Mathf.Max(t, 0.01f), rt.anchorMax.y);
        specialTimerBar.color = Color.Lerp(Color.red, Color.green, t);
    }

    private void UpdateVignette(bool hasSpecial, float progress)
    {
        if (specialVignette == null) return;

        specialVignette.gameObject.SetActive(hasSpecial);

        if (!hasSpecial) return;

        float remaining = gameManager.GetSpecialRemainingTime();

        float pulseFreq = remaining <= 3f
            ? Mathf.Lerp(8f, 2f, remaining / 3f)
            : 2f;

        float oscillation = Mathf.Abs(Mathf.Sin(Time.time * pulseFreq * Mathf.PI));

        float fadeOut = Mathf.Clamp01(progress * 5f);
        float alpha = oscillation * 0.3f * fadeOut;

        Color c = vignetteColor;
        c.a = alpha;
        specialVignette.color = c;
    }
}

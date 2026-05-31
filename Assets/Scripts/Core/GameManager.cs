// GameManager.cs
// Version: 2026-05-30 v5.1 (long economy: KPI/XP widened from int to long)

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ================= RANKS =================

    [Header("Ranks")]
    [SerializeField] private List<RankData> ranks;
    [SerializeField] private RankData currentRank;

    private Dictionary<SlotType, SlotBranch> branches;

    // ================= EXPERIENCE =================

    [Header("Experience")]
    [SerializeField] private long currentExperience;

    public long CurrentExperience => currentExperience;
    public long ExperienceToNextRank =>
        currentRank != null ? currentRank.requiredKpi : 0;

    // ================= KPI =================

    [Header("KPI")]
    [SerializeField] private long currentKpi;
    [SerializeField] private long kpiPerClick = 1;
    [SerializeField] private long kpiPerSecond = 0;

    public long CurrentKpi => currentKpi;
    public long KpiPerClick => kpiPerClick;
    public long KpiPerSecond => kpiPerSecond;
    public RankData CurrentRank => currentRank;
    public List<RankData> Ranks => ranks;
    public int CurrentRankIndex => ranks.IndexOf(currentRank);

    private float passiveTimer;

    // ================= MULTIPLIERS =================

    private float _flavorClickMult   = 1f;
    private float _activeClickMult   = 1f;
    private float _flavorPassiveMult  = 1f;
    private float _activePassiveMult  = 1f;

    public float clickKpiMultiplier   => _flavorClickMult * _activeClickMult;
    public float passiveKpiMultiplier => _flavorPassiveMult * _activePassiveMult;

    [HideInInspector] public float xpMultiplier         = 1f;
    [HideInInspector] public float cardDiscountMultiplier = 1f;

    public void SetFlavorClickMult(float v)   => _flavorClickMult = Mathf.Max(1f, v);
    public void SetFlavorPassiveMult(float v)  => _flavorPassiveMult = Mathf.Max(1f, v);
    public void SetActiveClickMult(float v)   => _activeClickMult = Mathf.Max(1f, v);
    public void SetActivePassiveMult(float v)  => _activePassiveMult = Mathf.Max(1f, v);
    public void ResetFlavorMults()  { _flavorClickMult = 1f; _flavorPassiveMult = 1f; }
    public void ResetActiveMults()  { _activeClickMult = 1f; _activePassiveMult = 1f; }

    // ================= CRITICAL CLICK =================

    [Header("Critical Bug Fix")]
    [SerializeField, Range(0f, 0.5f)] private float criticalChance = 0.05f;
    [SerializeField, Min(1f)] private float criticalMultiplier = 5f;

    // ================= AUDIO =================

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    // ================= SPECIALS =================

    private class ActiveSpecial
    {
        public Upgrade upgrade;
        public float remainingTime;
    }

    private readonly List<ActiveSpecial> activeSpecials = new();

    public bool HasActiveSpecial => activeSpecials.Count > 0;

    public float GetSpecialProgress01()
    {
        if (activeSpecials.Count == 0) return 0f;
        ActiveSpecial s = activeSpecials[0];
        if (s.upgrade.specialDuration <= 0f) return 0f;
        return Mathf.Clamp01(s.remainingTime / s.upgrade.specialDuration);
    }

    public float GetSpecialRemainingTime()
    {
        if (activeSpecials.Count == 0) return 0f;
        return activeSpecials[activeSpecials.Count - 1].remainingTime;
    }

    public void StartSpecialLoopIfNeeded()
    {
        if (activeSpecials.Count > 0)
            audioManager?.StartSpecialLoop();
    }

    // ================= EVENTS =================

    public event Action OnStateChanged;
    public event Action<RankData, RankData> OnRankUp;
    public event Action OnCareerCompleted;

    public struct WorkClickResult
    {
        public long kpiEarned;
        public long xpEarned;
        public bool isCritical;
        public float criticalMultiplier;

        public WorkClickResult(
            long kpiEarned,
            long xpEarned,
            bool isCritical,
            float criticalMultiplier)
        {
            this.kpiEarned = kpiEarned;
            this.xpEarned = xpEarned;
            this.isCritical = isCritical;
            this.criticalMultiplier = criticalMultiplier;
        }
    }

    // ================= STATE =================

    private bool isLoading = false;

    [SerializeField, HideInInspector]
    private OfflineProgressResult pendingOfflineResult;
    public OfflineProgressResult PendingOfflineResult => pendingOfflineResult;

    public void ClearPendingOfflineResult()
    {
        pendingOfflineResult = default;
    }

    // ================= TUTORIAL =================

    [SerializeField, HideInInspector]
    private bool hasSeenTutorial;
    public bool HasSeenTutorial => hasSeenTutorial;

    public void MarkTutorialSeen()
    {
        hasSeenTutorial = true;
        SaveGame();
    }

    // ================= CAREER COMPLETION =================

    [SerializeField, HideInInspector]
    private bool isCareerCompleted;
    [SerializeField, HideInInspector]
    private bool hasSeenCareerCompletedScreen;
    [SerializeField, HideInInspector]
    private long careerCompletedAtUtcTicks;

    public bool IsCareerCompleted => isCareerCompleted;
    public bool HasSeenCareerCompletedScreen => hasSeenCareerCompletedScreen;

    public void MarkCareerScreenSeen()
    {
        hasSeenCareerCompletedScreen = true;
        SaveGame();
    }

    private void CheckCareerCompletion()
    {
        if (isCareerCompleted) return;
        if (isLoading) return;

        int rankIndex = ranks.IndexOf(currentRank);
        if (rankIndex < ranks.Count - 1) return;

        if (currentRank.requiredKpi > 0 && currentExperience < currentRank.requiredKpi)
            return;

        // Win condition: reach CEO and hit the XP target. Buying out every
        // upgrade branch is NOT required.
        isCareerCompleted = true;
        careerCompletedAtUtcTicks = DateTime.UtcNow.Ticks;
        SaveGame();
        OnCareerCompleted?.Invoke();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
        CheckCareerCompletion();
    }

    // ================= SAVE =================

    private const string SAVE_KEY = "GAME_SAVE_V2";

    [Serializable]
    private class SaveData
    {
        public long experience;
        public long kpi;
        public long kpiPerClick;
        public long kpiPerSecond;

        public string currentRankId;
        public string currentRankName;
        public long lastSaveUtcTicks;

        public bool hasSeenTutorial;
        public bool isCareerCompleted;
        public bool hasSeenCareerCompletedScreen;
        public long careerCompletedAtUtcTicks;

        public List<BranchSave> branches = new();
        public List<SpecialSave> specials = new();
    }

    [Serializable]
    private class BranchSave
    {
        public SlotType slotType;
        public int index;
    }

    [Serializable]
    private class SpecialSave
    {
        public string upgradeId;
        public float remainingTime;
    }

    // ================= UNITY =================

    private void Start()
    {
        LoadGame();

        if (pendingOfflineResult.wasApplied)
            SaveGame();

        OnStateChanged?.Invoke();
    }

    private void OnValidate()
    {
        criticalChance = Mathf.Clamp(criticalChance, 0f, 0.5f);
        criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
    }

    private void Update()
    {
        TickPassiveIncome();
        TickSpecials();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // ================= EXPERIENCE =================

    private void AddExperience(long amount)
    {
        if (amount <= 0) return;

        currentExperience += amount;
        CheckRankUp();
    }

    private void CheckRankUp()
    {
        int index = ranks.IndexOf(currentRank);
        if (index < 0 || index >= ranks.Count - 1) return;

        if (currentExperience >= currentRank.requiredKpi)
        {
            currentExperience -= currentRank.requiredKpi;
            SetRank(ranks[index + 1]);
        }
    }

    private void SetRank(RankData newRank)
    {
        RankData previousRank = currentRank;

        currentRank = newRank;
        InitFromRank(newRank);

        // Cancel active specials silently — no end sound on rank-up
        foreach (var s in activeSpecials)
        {
            kpiPerClick -= s.upgrade.clickBonus;
            kpiPerSecond -= s.upgrade.passiveBonus;
        }
        activeSpecials.Clear();
        ResetActiveMults();
        audioManager?.StopSpecialLoop();

        audioManager?.PlayRankUp();
        audioManager?.PlayMusicForRank(newRank);

        if (!isLoading && previousRank != null && previousRank != newRank)
            OnRankUp?.Invoke(previousRank, newRank);
    }

    // ================= KPI =================

    private void AddKpi(long amount)
    {
        if (amount <= 0) return;
        currentKpi += amount;
    }

    public void AddKpiPublic(long amount)
    {
        AddKpi(amount);
        NotifyStateChanged();
    }

    public void AddXpPublic(long amount)
    {
        AddExperience(amount);
        NotifyStateChanged();
    }

    public WorkClickResult WorkClick()
    {
        float safeCriticalChance = Mathf.Clamp(criticalChance, 0f, 0.5f);
        float safeCriticalMultiplier = Mathf.Max(1f, criticalMultiplier);

        bool isCritical =
            safeCriticalChance > 0f &&
            safeCriticalMultiplier > 1f &&
            UnityEngine.Random.value < safeCriticalChance;

        float kpiMultiplierForClick = isCritical ? safeCriticalMultiplier : 1f;
        long kpiEarned = (long)Math.Round(
            (double)kpiPerClick * clickKpiMultiplier * kpiMultiplierForClick
        );

        AddKpi(kpiEarned);

        long xpEarned = (long)Math.Round((double)kpiPerClick * xpMultiplier);
        AddExperience(xpEarned);
        StatsTracker.Instance?.AddKpi(kpiEarned);
        NotifyStateChanged();

        return new WorkClickResult(
            kpiEarned,
            xpEarned,
            isCritical,
            kpiMultiplierForClick
        );
    }

    private void TickPassiveIncome()
    {
        if (kpiPerSecond <= 0) return;

        passiveTimer += Time.deltaTime;
        if (passiveTimer >= 1f)
        {
            passiveTimer -= 1f;
            long earned = (long)Math.Round((double)kpiPerSecond * passiveKpiMultiplier);
            AddKpi(earned);
            long xpEarned = (long)Math.Round((double)kpiPerSecond * xpMultiplier);
            AddExperience(xpEarned);
            StatsTracker.Instance?.AddKpi(earned);
            NotifyStateChanged();
        }
    }

    // ================= UPGRADES =================

    private void InitFromRank(RankData rank)
    {
        branches = new Dictionary<SlotType, SlotBranch>();
        foreach (var branchConfig in rank.slotBranches)
            branches[branchConfig.slotType] = new SlotBranch(branchConfig);
    }

    public Upgrade GetCurrentUpgrade(SlotType slotType)
    {
        if (branches == null) return null;
        return branches.ContainsKey(slotType)
            ? branches[slotType].GetCurrentUpgrade()
            : null;
    }

    public bool CanBuyUpgrade(SlotType slotType)
    {
        Upgrade upg = GetCurrentUpgrade(slotType);
        if (upg == null) return false;
        long price = (long)Math.Round((double)upg.basePrice * cardDiscountMultiplier);
        return currentKpi >= price;
    }

    public void TryBuyUpgrade(SlotType slotType)
    {
        if (branches == null || !branches.ContainsKey(slotType))
        {
            audioManager?.PlayError();
            return;
        }

        Upgrade upg = branches[slotType].GetCurrentUpgrade();
        long price = upg != null ? (long)Math.Round((double)upg.basePrice * cardDiscountMultiplier) : 0;
        if (upg == null || currentKpi < price)
        {
            audioManager?.PlayError();
            return;
        }

        currentKpi -= price;
        ApplyUpgrade(upg);
        branches[slotType].MarkPurchased();

        StatsTracker.Instance?.AddUpgrade();
        audioManager?.PlayBuy();
        NotifyStateChanged();
    }

    private void ApplyUpgrade(Upgrade upg)
    {
        kpiPerClick += upg.clickBonus;
        kpiPerSecond += upg.passiveBonus;

        if (upg.specialDuration > 0)
        {
            bool wasEmpty = activeSpecials.Count == 0;

            activeSpecials.Add(new ActiveSpecial
            {
                upgrade = upg,
                remainingTime = upg.specialDuration
            });

            audioManager?.PlaySpecialStart();

            if (wasEmpty)
                audioManager?.StartSpecialLoop();
        }
    }

    // ================= SPECIALS =================

    private void TickSpecials()
    {
        bool changed = false;

        for (int i = activeSpecials.Count - 1; i >= 0; i--)
        {
            var s = activeSpecials[i];
            s.remainingTime -= Time.deltaTime;

            if (s.remainingTime <= 0f)
            {
                kpiPerClick -= s.upgrade.clickBonus;
                kpiPerSecond -= s.upgrade.passiveBonus;
                activeSpecials.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
        {
            if (activeSpecials.Count == 0)
            {
                audioManager?.StopSpecialLoop();
                audioManager?.PlaySpecialEnd();
            }

            NotifyStateChanged();
        }
    }

    // ================= DEBUG =================

    [ContextMenu("Test Rank Up")]
    private void TestRankUp()
    {
        int currentIndex = ranks.IndexOf(currentRank);
        if (currentIndex < ranks.Count - 1)
            SetRank(ranks[currentIndex + 1]);
    }

    // ================= RESET =================

    public void ResetProgress()
    {
        ResetProgress(true);
    }

    // saveAfterReset=false leaves no save on disk → true "first launch" state
    // (main menu will show no "Continue", tutorial shows on next new game).
    public void ResetProgress(bool saveAfterReset)
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);

        currentExperience = 0;
        currentKpi = 0;
        kpiPerClick = 1;
        kpiPerSecond = 0;
        passiveTimer = 0f;

        activeSpecials.Clear();
        audioManager?.StopSpecialLoop();

        ResetFlavorMults();
        ResetActiveMults();

        pendingOfflineResult = default;
        hasSeenTutorial = false;
        isCareerCompleted = false;
        hasSeenCareerCompletedScreen = false;
        careerCompletedAtUtcTicks = 0;

        currentRank = ranks[0];
        InitFromRank(currentRank);

        StatsTracker.Instance?.ResetStats();
        audioManager?.PlayMusicForRank(currentRank);

        if (saveAfterReset)
            SaveGame();

        OnStateChanged?.Invoke();
    }

    // ================= SAVE / LOAD =================

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            experience = currentExperience,
            kpi = currentKpi,
            kpiPerClick = kpiPerClick,
            kpiPerSecond = kpiPerSecond,
            currentRankId = currentRank.rankId,
            currentRankName = currentRank.rankName,
            lastSaveUtcTicks = DateTime.UtcNow.Ticks,
            hasSeenTutorial = hasSeenTutorial,
            isCareerCompleted = isCareerCompleted,
            hasSeenCareerCompletedScreen = hasSeenCareerCompletedScreen,
            careerCompletedAtUtcTicks = careerCompletedAtUtcTicks
        };

        foreach (var pair in branches)
            data.branches.Add(new BranchSave
            {
                slotType = pair.Key,
                index = pair.Value.GetCurrentIndex()
            });

        foreach (var s in activeSpecials)
            data.specials.Add(new SpecialSave
            {
                upgradeId = s.upgrade.id,
                remainingTime = s.remainingTime
            });

        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        isLoading = true;
        pendingOfflineResult = default;

        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            InitFromRank(currentRank);
            isLoading = false;
            return;
        }

        SaveData data;
        try
        {
            data = JsonUtility.FromJson<SaveData>(
                PlayerPrefs.GetString(SAVE_KEY)
            );
            if (data == null)
                throw new Exception("SaveData is null after parse");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Save] Corrupted save, starting fresh. Error: {e.Message}");
            PlayerPrefs.DeleteKey(SAVE_KEY);
            InitFromRank(currentRank);
            isLoading = false;
            return;
        }

        // --- Rank ---

        RankData loadedRank = null;
        if (!string.IsNullOrEmpty(data.currentRankId))
            loadedRank = ranks.Find(r => r.rankId == data.currentRankId);
        if (loadedRank == null && !string.IsNullOrEmpty(data.currentRankName))
            loadedRank = ranks.Find(r => r.rankName == data.currentRankName);

        if (loadedRank == null)
        {
            Debug.LogWarning($"[Save] Rank '{data.currentRankName}' not found. Starting new game.");
            InitFromRank(currentRank);
            isLoading = false;
            return;
        }

        currentRank = loadedRank;
        InitFromRank(currentRank);

        // --- Stats ---

        currentExperience = data.experience;
        currentKpi = data.kpi;
        kpiPerClick = data.kpiPerClick;
        kpiPerSecond = data.kpiPerSecond;
        hasSeenTutorial = data.hasSeenTutorial;
        isCareerCompleted = data.isCareerCompleted;
        hasSeenCareerCompletedScreen = data.hasSeenCareerCompletedScreen;
        careerCompletedAtUtcTicks = data.careerCompletedAtUtcTicks;

        // Repair saves written before the int->long fix: an overflowed
        // accumulator could be stored as a negative value. KPI/XP are never
        // negative in this economy, so clamp them back to a valid floor.
        if (currentExperience < 0) currentExperience = 0;
        if (currentKpi < 0) currentKpi = 0;
        if (kpiPerClick < 1) kpiPerClick = 1;
        if (kpiPerSecond < 0) kpiPerSecond = 0;

        // --- Branch indices ---

        if (data.branches != null)
        {
            foreach (var bs in data.branches)
            {
                if (branches.ContainsKey(bs.slotType))
                    branches[bs.slotType].SetIndex(bs.index);
            }
        }

        // --- Active specials ---

        activeSpecials.Clear();

        if (data.specials != null)
        {
            foreach (var ss in data.specials)
            {
                Upgrade upg = FindUpgradeById(ss.upgradeId);
                if (upg != null && ss.remainingTime > 0f)
                {
                    activeSpecials.Add(new ActiveSpecial
                    {
                        upgrade = upg,
                        remainingTime = ss.remainingTime
                    });
                }
            }
        }

        // --- Offline progress ---

        if (data.lastSaveUtcTicks > 0)
        {
            DateTime lastSave = new DateTime(data.lastSaveUtcTicks, DateTimeKind.Utc);
            double offlineSeconds = (DateTime.UtcNow - lastSave).TotalSeconds;

            offlineSeconds = System.Math.Min(offlineSeconds, 4 * 3600.0);

            long earnedKpi = (long)((double)kpiPerSecond * offlineSeconds * 0.6);

            bool isCeo = ranks.IndexOf(currentRank) >= ranks.Count - 1;
            long earnedXp = 0;
            if (!isCeo && kpiPerSecond > 0)
                earnedXp = (long)((double)kpiPerSecond * xpMultiplier * offlineSeconds * 0.6);

            if (earnedKpi > 0)
                currentKpi += earnedKpi;

            if (earnedXp > 0)
                AddExperience(earnedXp);

            bool shouldShow = offlineSeconds >= 60.0 && (earnedKpi > 0 || earnedXp > 0);

            pendingOfflineResult = new OfflineProgressResult
            {
                offlineSeconds = offlineSeconds,
                earnedKpi = earnedKpi,
                earnedXp = earnedXp,
                wasApplied = earnedKpi > 0 || earnedXp > 0,
                shouldShowPopup = shouldShow
            };
        }

        isLoading = false;
    }

    // ================= HELPERS =================

    private Upgrade FindUpgradeById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var rank in ranks)
        {
            foreach (var branchConfig in rank.slotBranches)
            {
                foreach (var upgrade in branchConfig.upgrades)
                {
                    if (upgrade.id == id)
                        return upgrade;
                }
            }
        }

        return null;
    }
}

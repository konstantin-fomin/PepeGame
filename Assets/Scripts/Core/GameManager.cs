// GameManager.cs
// Version: 2026-05-26 v3.0 (offline progress)

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
    [SerializeField] private int currentExperience;

    public int CurrentExperience => currentExperience;
    public int ExperienceToNextRank =>
        currentRank != null ? currentRank.requiredKpi : 0;

    // ================= KPI =================

    [Header("KPI")]
    [SerializeField] private int currentKpi;
    [SerializeField] private int kpiPerClick = 1;
    [SerializeField] private int kpiPerSecond = 0;

    public int CurrentKpi => currentKpi;
    public int KpiPerClick => kpiPerClick;
    public int KpiPerSecond => kpiPerSecond;
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

    // --- Public API for Special state (single source of truth) ---

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

    /// <summary>
    /// Запускает звук лупа, если есть активные specials.
    /// Вызывается при входе в игру из меню.
    /// </summary>
    public void StartSpecialLoopIfNeeded()
    {
        if (activeSpecials.Count > 0)
            audioManager?.StartSpecialLoop();
    }

    // ================= EVENTS =================

    public event Action OnStateChanged;
    public event Action<RankData, RankData> OnRankUp;

    public struct WorkClickResult
    {
        public int kpiEarned;
        public int xpEarned;
        public bool isCritical;
        public float criticalMultiplier;

        public WorkClickResult(
            int kpiEarned,
            int xpEarned,
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
    public int PendingOfflineKpi { get; private set; } = 0;

    // ================= SAVE =================

    private const string SAVE_KEY = "GAME_SAVE_V2";

    [Serializable]
    private class SaveData
    {
        public int experience;
        public int kpi;
        public int kpiPerClick;
        public int kpiPerSecond;

        public string currentRankId;
        public string currentRankName;
        public long lastSaveUtcTicks;

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

    private void AddExperience(int amount)
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

        audioManager?.PlayRankUp();
        audioManager?.PlayMusicForRank(newRank);

        if (!isLoading && previousRank != null && previousRank != newRank)
            OnRankUp?.Invoke(previousRank, newRank);
    }

    // ================= KPI =================

    private void AddKpi(int amount)
    {
        if (amount <= 0) return;
        currentKpi += amount;
    }

    public void AddKpiPublic(int amount)
    {
        AddKpi(amount);
        OnStateChanged?.Invoke();
    }

    public void AddXpPublic(int amount)
    {
        AddExperience(amount);
        OnStateChanged?.Invoke();
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
        int kpiEarned = Mathf.RoundToInt(
            kpiPerClick * clickKpiMultiplier * kpiMultiplierForClick
        );

        AddKpi(kpiEarned);

        int xpEarned = Mathf.RoundToInt(kpiPerClick * xpMultiplier);
        AddExperience(xpEarned);
        StatsTracker.Instance?.AddKpi(kpiEarned);
        OnStateChanged?.Invoke();

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
            int earned = Mathf.RoundToInt(kpiPerSecond * passiveKpiMultiplier);
            AddKpi(earned);
            int xpEarned = Mathf.RoundToInt(kpiPerSecond * xpMultiplier);
            AddExperience(xpEarned);
            StatsTracker.Instance?.AddKpi(earned);
            OnStateChanged?.Invoke();
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
        int price = Mathf.RoundToInt(upg.basePrice * cardDiscountMultiplier);
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
        int price = upg != null ? Mathf.RoundToInt(upg.basePrice * cardDiscountMultiplier) : 0;
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
        OnStateChanged?.Invoke();
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

            OnStateChanged?.Invoke();
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

        currentRank = ranks[0];
        InitFromRank(currentRank);

        StatsTracker.Instance?.ResetStats();
        audioManager?.PlayMusicForRank(currentRank);
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
            lastSaveUtcTicks = DateTime.UtcNow.Ticks
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

        // Special loop sound starts in StartSpecialLoopIfNeeded(),
        // called from MenuNavigationController.StartGame()
        // to avoid playing in the menu.

        // --- Offline progress ---

        if (data.lastSaveUtcTicks > 0)
        {
            DateTime lastSave = new DateTime(data.lastSaveUtcTicks, DateTimeKind.Utc);
            double offlineSeconds = (DateTime.UtcNow - lastSave).TotalSeconds;

            offlineSeconds = Mathf.Min((float)offlineSeconds, 4 * 3600f);

            int earned = Mathf.FloorToInt((float)(kpiPerSecond * offlineSeconds * 0.6f));

            if (earned > 0)
            {
                currentKpi += earned;
                PendingOfflineKpi = earned;
            }
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

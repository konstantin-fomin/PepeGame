// GameManager.cs
// Version: 2026-05-24 v2.8 (Synchronized special lifecycle)

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

    private float passiveTimer;

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
        ActiveSpecial s = activeSpecials[activeSpecials.Count - 1];
        if (s.upgrade.specialDuration <= 0f) return 0f;
        return Mathf.Clamp01(s.remainingTime / s.upgrade.specialDuration);
    }

    public float GetSpecialRemainingTime()
    {
        if (activeSpecials.Count == 0) return 0f;
        return activeSpecials[activeSpecials.Count - 1].remainingTime;
    }

    // ================= EVENTS =================

    public event Action OnStateChanged;

    // ================= SAVE =================

    private const string SAVE_KEY = "GAME_SAVE_V2";

    [Serializable]
    private class SaveData
    {
        public int experience;
        public int kpi;
        public int kpiPerClick;
        public int kpiPerSecond;

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

        if (audioManager != null && currentRank != null)
            audioManager.PlayMusicForRank(currentRank);

        OnStateChanged?.Invoke();
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
        currentRank = newRank;
        InitFromRank(newRank);

        audioManager?.PlayRankUp();
        audioManager?.PlayMusicForRank(newRank);
    }

    // ================= KPI =================

    private void AddKpi(int amount)
    {
        if (amount <= 0) return;
        currentKpi += amount;
    }

    public void WorkClick()
    {
        AddKpi(kpiPerClick);
        AddExperience(kpiPerClick);
        OnStateChanged?.Invoke();
    }

    private void TickPassiveIncome()
    {
        if (kpiPerSecond <= 0) return;

        passiveTimer += Time.deltaTime;
        if (passiveTimer >= 1f)
        {
            passiveTimer -= 1f;
            AddKpi(kpiPerSecond);
            AddExperience(kpiPerSecond);
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
        return branches.ContainsKey(slotType)
            ? branches[slotType].GetCurrentUpgrade()
            : null;
    }

    public bool CanBuyUpgrade(SlotType slotType)
    {
        Upgrade upg = GetCurrentUpgrade(slotType);
        return upg != null && currentKpi >= upg.basePrice;
    }

    public void TryBuyUpgrade(SlotType slotType)
    {
        if (!branches.ContainsKey(slotType))
        {
            audioManager?.PlayError();
            return;
        }

        Upgrade upg = branches[slotType].GetCurrentUpgrade();
        if (upg == null || currentKpi < upg.basePrice)
        {
            audioManager?.PlayError();
            return;
        }

        currentKpi -= upg.basePrice;
        ApplyUpgrade(upg);
        branches[slotType].MarkPurchased();

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

        currentRank = ranks[0];
        InitFromRank(currentRank);

        audioManager?.PlayMusicForRank(currentRank);
        SaveGame();
        OnStateChanged?.Invoke();
    }

    // ================= SAVE / LOAD =================

    private void SaveGame()
    {
        SaveData data = new SaveData
        {
            experience = currentExperience,
            kpi = currentKpi,
            kpiPerClick = kpiPerClick,
            kpiPerSecond = kpiPerSecond,
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
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            InitFromRank(currentRank);
            return;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(
            PlayerPrefs.GetString(SAVE_KEY)
        );

        // --- Rank ---

        RankData loadedRank = ranks.Find(r => r.rankName == data.currentRankName);

        if (loadedRank == null)
        {
            Debug.LogWarning($"[Save] Rank '{data.currentRankName}' not found. Starting new game.");
            InitFromRank(currentRank);
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

        // Если загрузились с активными specials — запустить луп
        if (activeSpecials.Count > 0)
            audioManager?.StartSpecialLoop();
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

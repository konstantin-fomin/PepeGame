// GameManager.cs
// Version: 2026-01-16 v2.4 (Rank up sound added)
// Author: ChatGPT + Kostya

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

    [Header("Experience (Career Progress)")]
    [SerializeField] private int currentExperience;

    public int CurrentExperience => currentExperience;
    public int ExperienceToNextRank =>
        currentRank != null ? currentRank.requiredKpi : 0;

    // ================= KPI (CURRENCY) =================

    [Header("KPI (Currency)")]
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
        Debug.Log("[GM] Started with rank: " + currentRank.rankName);
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
        if (amount <= 0)
            return;

        currentExperience += amount;
        CheckRankUp();
    }

    private void CheckRankUp()
    {
        int index = ranks.IndexOf(currentRank);
        if (index < 0 || index >= ranks.Count - 1)
            return;

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

        // 🔊 RANK UP SOUND
        audioManager?.PlayRankUp();

        Debug.Log("[GM] Rank changed to: " + newRank.rankName);
    }

    // ================= KPI =================

    private void AddKpi(int amount)
    {
        if (amount <= 0)
            return;

        currentKpi += amount;
    }

    public void WorkClick()
    {
        AddKpi(kpiPerClick);
        AddExperience(kpiPerClick);
    }

    private void TickPassiveIncome()
    {
        if (kpiPerSecond <= 0)
            return;

        passiveTimer += Time.deltaTime;

        if (passiveTimer >= 1f)
        {
            passiveTimer -= 1f;
            AddKpi(kpiPerSecond);
            AddExperience(kpiPerSecond);
        }
    }

    // ================= UPGRADES =================

    private void InitFromRank(RankData rank)
    {
        branches = new Dictionary<SlotType, SlotBranch>();

        foreach (var branchConfig in rank.slotBranches)
        {
            branches[branchConfig.slotType] =
                new SlotBranch(branchConfig);
        }
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

        SlotBranch branch = branches[slotType];
        Upgrade upg = branch.GetCurrentUpgrade();

        if (upg == null)
        {
            audioManager?.PlayError();
            return;
        }

        if (currentKpi < upg.basePrice)
        {
            audioManager?.PlayError();
            return;
        }

        currentKpi -= upg.basePrice;

        ApplyUpgrade(upg);
        branch.MarkPurchased();

        audioManager?.PlayBuy();

        Debug.Log("[GM] Bought upgrade: " + upg.title);
    }

    private void ApplyUpgrade(Upgrade upg)
    {
        kpiPerClick += upg.clickBonus;
        kpiPerSecond += upg.passiveBonus;

        if (upg.specialDuration > 0)
        {
            activeSpecials.Add(new ActiveSpecial
            {
                upgrade = upg,
                remainingTime = upg.specialDuration
            });

            audioManager?.PlaySpecialStart();
        }
    }

    // ================= SPECIALS =================

    private void TickSpecials()
    {
        for (int i = activeSpecials.Count - 1; i >= 0; i--)
        {
            var s = activeSpecials[i];
            s.remainingTime -= Time.deltaTime;

            if (s.remainingTime <= 0f)
            {
                kpiPerClick -= s.upgrade.clickBonus;
                kpiPerSecond -= s.upgrade.passiveBonus;
                activeSpecials.RemoveAt(i);

                audioManager?.PlaySpecialEnd();

                Debug.Log("[GM] Special ended: " + s.upgrade.title);
            }
        }
    }

    public bool TryGetActiveSpecial(string upgradeId, out float remainingTime)
    {
        foreach (var s in activeSpecials)
        {
            if (s.upgrade.id == upgradeId)
            {
                remainingTime = s.remainingTime;
                return true;
            }
        }

        remainingTime = 0f;
        return false;
    }

    // ================= RESET =================

    public void ResetProgress()
    {
        Debug.Log("[GM] RESET GAME");

        PlayerPrefs.DeleteKey(SAVE_KEY);

        currentExperience = 0;
        currentKpi = 0;
        kpiPerClick = 1;
        kpiPerSecond = 0;
        passiveTimer = 0f;

        activeSpecials.Clear();

        currentRank = ranks[0];
        InitFromRank(currentRank);

        SaveGame();
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
        {
            data.branches.Add(new BranchSave
            {
                slotType = pair.Key,
                index = pair.Value.GetCurrentIndex()
            });
        }

        foreach (var s in activeSpecials)
        {
            data.specials.Add(new SpecialSave
            {
                upgradeId = s.upgrade.id,
                remainingTime = s.remainingTime
            });
        }

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

        currentExperience = data.experience;
        currentKpi = data.kpi;
        kpiPerClick = data.kpiPerClick;
        kpiPerSecond = data.kpiPerSecond;

        currentRank = ranks.Find(r => r.rankName == data.currentRankName);
        InitFromRank(currentRank);

        foreach (var b in data.branches)
        {
            if (branches.ContainsKey(b.slotType))
                branches[b.slotType].SetIndex(b.index);
        }

        activeSpecials.Clear();

        foreach (var s in data.specials)
        {
            Upgrade upg = FindUpgradeById(s.upgradeId);
            if (upg != null)
            {
                activeSpecials.Add(new ActiveSpecial
                {
                    upgrade = upg,
                    remainingTime = s.remainingTime
                });
            }
        }

        long nowTicks = DateTime.UtcNow.Ticks;
        TimeSpan delta = new TimeSpan(nowTicks - data.lastSaveUtcTicks);

        int offlineSeconds = Mathf.Max(0, (int)delta.TotalSeconds);
        if (offlineSeconds > 0)
        {
            AddKpi(offlineSeconds * kpiPerSecond);
            AddExperience(offlineSeconds * kpiPerSecond);
        }
    }

    private Upgrade FindUpgradeById(string id)
    {
        foreach (var rank in ranks)
            foreach (var branch in rank.slotBranches)
                foreach (var upg in branch.upgrades)
                    if (upg.id == id)
                        return upg;

        return null;
    }
}

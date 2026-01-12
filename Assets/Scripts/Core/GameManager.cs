using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Ranks")]
    [SerializeField] private List<RankData> ranks;
    [SerializeField] private RankData currentRank;

    private Dictionary<SlotType, SlotBranch> branches;

    [Header("KPI")]
    [SerializeField] private int currentKpi;
    [SerializeField] private int kpiPerClick = 1;
    [SerializeField] private int kpiPerSecond = 0;

    private float passiveTimer;

    // ================= SPECIAL RUNTIME =================

    private class ActiveSpecial
    {
        public Upgrade upgrade;
        public float remainingTime;
    }

    private readonly List<ActiveSpecial> activeSpecials = new();

    // ================= PUBLIC API =================

    public int CurrentKpi => currentKpi;
    public int KpiPerClick => kpiPerClick;
    public int KpiPerSecond => kpiPerSecond;
    public RankData CurrentRank => currentRank;

    public int KpiInCurrentRank =>
        currentKpi - GetRankStartKpi(currentRank);

    public int KpiToNextRank =>
        currentRank.requiredKpi;

    // ================= UNITY =================

    private void Start()
    {
        InitFromRank(currentRank);
    }

    private void Update()
    {
        TickPassiveIncome();
        TickSpecials();
    }

    // ================= KPI =================

    private void TickPassiveIncome()
    {
        if (kpiPerSecond <= 0)
            return;

        passiveTimer += Time.deltaTime;

        if (passiveTimer >= 1f)
        {
            passiveTimer -= 1f;
            AddKpi(kpiPerSecond);
        }
    }

    private void AddKpi(int amount)
    {
        currentKpi += amount;
        CheckRankUp();
    }

    public void WorkClick()
    {
        AddKpi(kpiPerClick);
    }

    // ================= RANK =================

    private void CheckRankUp()
    {
        int index = ranks.IndexOf(currentRank);
        if (index < 0 || index >= ranks.Count - 1)
            return;

        if (KpiInCurrentRank >= currentRank.requiredKpi)
            SetRank(ranks[index + 1]);
    }

    private void SetRank(RankData newRank)
    {
        currentRank = newRank;
        InitFromRank(currentRank);
    }

    private int GetRankStartKpi(RankData rank)
    {
        int sum = 0;

        foreach (var r in ranks)
        {
            if (r == rank)
                break;

            sum += r.requiredKpi;
        }

        return sum;
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
            return;

        SlotBranch branch = branches[slotType];
        Upgrade upg = branch.GetCurrentUpgrade();

        if (upg == null || currentKpi < upg.basePrice)
            return;

        currentKpi -= upg.basePrice;
        ApplyUpgrade(upg);
        branch.MarkPurchased();
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
        }
    }

    // ================= SPECIAL =================

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

    // ================= RESET (FOR TESTING) =================

    public void ResetProgress()
    {
        currentKpi = 0;
        kpiPerClick = 1;
        kpiPerSecond = 0;
        passiveTimer = 0f;

        activeSpecials.Clear();

        if (ranks != null && ranks.Count > 0)
            currentRank = ranks[0];

        InitFromRank(currentRank);

        Debug.Log("Progress reset");
    }
}

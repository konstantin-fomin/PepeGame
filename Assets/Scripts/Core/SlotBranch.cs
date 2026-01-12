using UnityEngine;

public class SlotBranch
{
    private readonly SlotBranchConfig config;
    private int currentIndex;

    public SlotType SlotType => config.slotType;

    public SlotBranch(SlotBranchConfig config)
    {
        this.config = config;
        currentIndex = 0;
    }

    public Upgrade GetCurrentUpgrade()
    {
        if (currentIndex < 0 || currentIndex >= config.upgrades.Count)
            return null;

        return config.upgrades[currentIndex];
    }

    public void MarkPurchased()
    {
        currentIndex++;
    }

    public bool IsFinished()
    {
        return currentIndex >= config.upgrades.Count;
    }

    // ===== ÄËß SAVE / LOAD =====

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public void SetIndex(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, config.upgrades.Count);
    }
}

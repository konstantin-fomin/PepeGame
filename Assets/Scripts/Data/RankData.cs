using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Rank Data")]
public class RankData : ScriptableObject
{
    [Header("Rank Info")]
    public string rankName;
    public string rankUpMessage;
    public Sprite backgroundSprite;
    public string environmentScene;

    [Tooltip("KPI required to unlock the NEXT rank")]
    public int requiredKpi;

    [Header("Audio")]
    [Tooltip("Background music for this rank")]
    public AudioClip backgroundMusic;

    [Header("Upgrade Branches")]
    public List<SlotBranchConfig> slotBranches;
}

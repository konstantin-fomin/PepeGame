using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Slot Branch")]
public class SlotBranchConfig : ScriptableObject
{
    public SlotType slotType;
    public List<Upgrade> upgrades;
}

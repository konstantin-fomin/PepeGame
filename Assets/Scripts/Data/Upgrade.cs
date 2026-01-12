using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string id;
    public string title;
    [TextArea]
    public string description;

    public int basePrice;

    [Header("Bonuses")]
    public int clickBonus;
    public int passiveBonus;

    [Header("Special")]
    public float specialDuration; // 0 = не special
}

// Upgrade.cs
// Version: 2026-01-16 v1.1

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

    [Header("Visual")]
    public Sprite icon; // иконка апгрейда
}

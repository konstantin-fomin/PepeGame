using UnityEngine;
using System;
using System.Collections.Generic;

public enum OfficeEventType
{
    ClickKpiMultiplier = 0,
    PassiveKpiMultiplier = 1,
    InstantKpiReward = 3,
    NoEffectFlavor = 6
}

[Serializable]
public class OfficeEventEffect
{
    public OfficeEventType type;
    public float value;
}

[CreateAssetMenu(menuName = "PepeIdle/OfficeEvent")]
public class OfficeEventData : ScriptableObject
{
    public string id;
    public string title;
    public string description;
    public string effectDescription;
    public string buttonText;

    public List<OfficeEventEffect> effects;

    public float durationSeconds      = 15f;
    public float popupLifetimeSeconds = 12f;

    [Range(1, 20)]
    public int weight = 5;

    public string minRankId = "intern";
    public string maxRankId = "";

    public bool canRepeat = true;
}

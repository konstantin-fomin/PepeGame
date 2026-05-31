using UnityEngine;

[CreateAssetMenu(menuName = "PepeIdle/ActiveOfficeEventData")]
public class ActiveOfficeEventData : ScriptableObject
{
    [SerializeField] public string id;
    [SerializeField] public string title;
    [SerializeField] public string description;
    [SerializeField] public string effectDescription;
    [SerializeField] public string buttonText;

    // Localization base key (CSV ID of the title row, e.g. "LOC_0287").
    // The four display fields occupy consecutive ids: title, description, effect, button.
    [SerializeField] public string locId;

    [SerializeField] public OfficeEventType effectType;
    [SerializeField] public float effectValue;
    [SerializeField] public float durationSeconds;
    [SerializeField] public float popupLifetimeSeconds;
    [SerializeField, Range(1, 20)] public int weight = 5;
    [SerializeField] public string minRankId = "intern";
}

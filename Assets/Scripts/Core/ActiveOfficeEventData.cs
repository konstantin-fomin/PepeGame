using UnityEngine;

[CreateAssetMenu(menuName = "PepeIdle/ActiveOfficeEventData")]
public class ActiveOfficeEventData : ScriptableObject
{
    [SerializeField] public string id;
    [SerializeField] public string title;
    [SerializeField] public string description;
    [SerializeField] public string effectDescription;
    [SerializeField] public string buttonText;
    [SerializeField] public OfficeEventType effectType;
    [SerializeField] public float effectValue;
    [SerializeField] public float durationSeconds;
    [SerializeField] public float popupLifetimeSeconds;
    [SerializeField, Range(1, 20)] public int weight = 5;
    [SerializeField] public string minRankId = "intern";
}

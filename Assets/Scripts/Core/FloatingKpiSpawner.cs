// FloatingKpiSpawner.cs
// Version: 2026-01-12_v1

using UnityEngine;

public class FloatingKpiSpawner : MonoBehaviour
{
    [SerializeField] private FloatingKpi prefab;
    [SerializeField] private RectTransform container;

    public void Spawn(Vector2 screenPosition, int amount)
    {
        FloatingKpi instance = Instantiate(prefab, container);
        instance.gameObject.SetActive(true);

        RectTransform rt = instance.GetComponent<RectTransform>();
        rt.position = screenPosition;

        instance.Init(amount);
    }
}

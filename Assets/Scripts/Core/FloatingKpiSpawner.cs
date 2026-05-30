// FloatingKpiSpawner.cs
// Version: 2026-01-12_v2 (Object pooling)

using System.Collections.Generic;
using UnityEngine;

public class FloatingKpiSpawner : MonoBehaviour
{
    [SerializeField] private FloatingKpi prefab;
    [SerializeField] private RectTransform container;
    [SerializeField] private int poolSize = 10;

    private readonly List<FloatingKpi> pool = new();

    private void Awake()
    {
        if (prefab == null)     { Debug.LogError("[FloatingKpiSpawner] prefab not assigned", this); enabled = false; return; }
        if (container == null)  { Debug.LogError("[FloatingKpiSpawner] container not assigned", this); enabled = false; return; }

        for (int i = 0; i < poolSize; i++)
            pool.Add(CreateInstance());
    }

    public void Spawn(Vector2 screenPosition, long amount)
    {
        Spawn(screenPosition, amount, false);
    }

    public void Spawn(Vector2 screenPosition, long amount, bool isCritical)
    {
        FloatingKpi instance = GetFromPool();

        instance.gameObject.SetActive(true);

        RectTransform rt = instance.GetComponent<RectTransform>();
        rt.position = screenPosition;

        instance.Init(amount, isCritical);
    }

    // ================= POOL =================

    private FloatingKpi GetFromPool()
    {
        foreach (var item in pool)
        {
            if (!item.gameObject.activeSelf)
                return item;
        }

        FloatingKpi newInstance = CreateInstance();
        pool.Add(newInstance);
        return newInstance;
    }

    private FloatingKpi CreateInstance()
    {
        FloatingKpi instance = Instantiate(prefab, container);
        instance.gameObject.SetActive(false);
        return instance;
    }
}

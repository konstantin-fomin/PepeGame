// FloatingKpi.cs
// Version: 2026-01-12_v1

using UnityEngine;
using TMPro;

public class FloatingKpi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float lifetime = 0.8f;

    private float timer;

    public void Init(int amount)
    {
        text.text = $"+{amount} KPI";
        timer = lifetime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }
}

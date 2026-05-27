// WorkZoneClick.cs
// Version: 2026-05-26 v1.7 (StatsTracker integration)

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class WorkZoneClick : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private FloatingKpiSpawner floatingKpiSpawner;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Visual")]
    private LaptopScreenLightController laptopScreenLight;
    [SerializeField, Min(1f)] private float criticalPulseStrength = 1.35f;

    [Header("Floating KPI")]
    [SerializeField, Tooltip("Delay before showing +KPI visual (seconds)")]
    private float floatingKpiDelay = 0.15f;

    private Collider2D zoneCollider;
    private Camera mainCamera;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        laptopScreenLight = FindObjectOfType<LaptopScreenLightController>();
    }

    public void OnPointerClick(InputValue value)
    {
        if (RankUpPopupView.IsShowing) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        Vector3 screenPosWithZ = new Vector3(
            screenPos.x,
            screenPos.y,
            -mainCamera.transform.position.z
        );

        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != zoneCollider)
            return;

        GameManager.WorkClickResult clickResult = gameManager.WorkClick();
        StatsTracker.Instance?.AddClick();

        if (clickResult.isCritical)
            audioManager?.PlayCriticalClick();
        else
            audioManager?.PlayClick();

        if (clickResult.isCritical)
            laptopScreenLight?.TriggerLightPulse(criticalPulseStrength);
        else
            laptopScreenLight?.TriggerLightPulse();

        StartCoroutine(SpawnFloatingKpiWithDelay(
            screenPos,
            clickResult.kpiEarned,
            clickResult.isCritical
        ));
    }

    // ================= INTERNAL =================

    private IEnumerator SpawnFloatingKpiWithDelay(
        Vector2 screenPos,
        int amount,
        bool isCritical)
    {
        yield return new WaitForSeconds(floatingKpiDelay);

        floatingKpiSpawner.Spawn(screenPos, amount, isCritical);
    }
}

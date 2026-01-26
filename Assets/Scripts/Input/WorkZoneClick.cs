// WorkZoneClick.cs
// Version: 2026-01-26 v1.6 (Delayed floating KPI visual)

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
    [SerializeField] private LaptopScreenLightController laptopScreenLight;

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

    public void OnPointerClick(InputValue value)
    {
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

        // === GAME LOGIC (—–¿«”) ===
        gameManager.WorkClick();

        // === SOUND (—–¿«”) ===
        audioManager?.PlayClick();

        // === SCREEN LIGHT (—–¿«”) ===
        laptopScreenLight?.TriggerLightPulse();

        // === VISUAL (+KPI) — Ã» –Œ«¿ƒ≈–∆ Œ… ===
        StartCoroutine(SpawnFloatingKpiWithDelay(
            screenPos,
            gameManager.KpiPerClick
        ));
    }

    // ================= INTERNAL =================

    private IEnumerator SpawnFloatingKpiWithDelay(Vector2 screenPos, int amount)
    {
        yield return new WaitForSeconds(floatingKpiDelay);

        floatingKpiSpawner.Spawn(screenPos, amount);
    }
}

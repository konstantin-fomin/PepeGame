// WorkZoneClick.cs
// Version: 2026-05-30 v1.9 (lazy screen-light lookup across additive env scenes)

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
        if (OfflineProgressPopupView.IsShowing) return;
        if (FirstLaunchTutorialPopupView.IsShowing) return;
        if (CareerCompletedScreenView.IsShowing) return;
        if (MenuNavigationController.Instance != null &&
            !MenuNavigationController.Instance.IsGameActive) return;

        var pointer = Pointer.current;
        if (pointer == null) return;
        Vector2 screenPos = pointer.position.ReadValue();

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
            TriggerScreenLightPulse(criticalPulseStrength);
        else
            TriggerScreenLightPulse(1f);

        StartCoroutine(SpawnFloatingKpiWithDelay(
            screenPos,
            clickResult.kpiEarned,
            clickResult.isCritical
        ));
    }

    // ================= INTERNAL =================

    // The screen light lives in an additively-loaded environment scene that
    // isn't present at Start, and gets destroyed/recreated on rank change.
    // Resolve it lazily: re-find whenever the cached reference is null
    // (Unity's == null is true for destroyed objects too).
    private void TriggerScreenLightPulse(float strength)
    {
        if (laptopScreenLight == null)
            laptopScreenLight = FindObjectOfType<LaptopScreenLightController>();

        laptopScreenLight?.TriggerLightPulse(strength);
    }

    private IEnumerator SpawnFloatingKpiWithDelay(
        Vector2 screenPos,
        long amount,
        bool isCritical)
    {
        yield return new WaitForSeconds(floatingKpiDelay);

        floatingKpiSpawner.Spawn(screenPos, amount, isCritical);
    }
}

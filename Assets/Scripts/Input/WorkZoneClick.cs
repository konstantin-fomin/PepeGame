// WorkZoneClick.cs
// Version: 2026-01-12 v1.4 (AudioManager click sound)

using UnityEngine;
using UnityEngine.InputSystem;

public class WorkZoneClick : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private FloatingKpiSpawner floatingKpiSpawner;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

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

        // === GAME LOGIC ===
        gameManager.WorkClick();

        floatingKpiSpawner.Spawn(
            screenPos,
            gameManager.KpiPerClick
        );

        // === SOUND ===
        if (audioManager != null)
            audioManager.PlayClick();
    }
}

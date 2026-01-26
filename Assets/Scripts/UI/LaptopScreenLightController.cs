// LaptopScreenLightController.cs
// Version: 2026-01-26 v1.2 (Delayed pulse start)

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaptopScreenLightController : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light2D screenLight;

    [Header("Pulse Settings")]
    [SerializeField] private float maxIntensity = 1.2f;
    [SerializeField] private float fadeInDuration = 0.08f;
    [SerializeField] private float fadeOutDuration = 0.2f;
    [SerializeField] private float holdBeforeFadeOut = 0.05f;

    [Header("Timing")]
    [SerializeField] private float pulseDelay = 0.15f; // 🔹 задержка старта света

    private float currentIntensity;
    private float fadeOutTimer;
    private float delayTimer;

    private bool isActive;
    private bool waitingForPulse;

    private void Awake()
    {
        if (screenLight == null)
            screenLight = GetComponent<Light2D>();

        currentIntensity = 0f;
        screenLight.intensity = 0f;
    }

    private void Update()
    {
        // === ОЖИДАНИЕ ЗАДЕРЖКИ ===
        if (waitingForPulse)
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0f)
            {
                waitingForPulse = false;
                isActive = true;
                fadeOutTimer = holdBeforeFadeOut;
            }

            return;
        }

        // === САМ ИМПУЛЬС (НЕ МЕНЯЛИ ЛОГИКУ) ===
        if (!isActive)
            return;

        if (fadeOutTimer > 0f)
        {
            fadeOutTimer -= Time.deltaTime;

            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                maxIntensity,
                Time.deltaTime / fadeInDuration
            );
        }
        else
        {
            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                0f,
                Time.deltaTime / fadeOutDuration
            );

            if (currentIntensity <= 0f)
            {
                currentIntensity = 0f;
                isActive = false;
            }
        }

        screenLight.intensity = currentIntensity;
    }

    // ================= PUBLIC API =================

    public void TriggerLightPulse()
    {
        // при каждом клике перезапускаем задержку
        waitingForPulse = true;
        delayTimer = pulseDelay;
    }
}

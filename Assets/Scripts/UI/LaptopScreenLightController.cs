// LaptopScreenLightController.cs
// Version: 2026-01-26 v1.1

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

    private float currentIntensity;
    private float fadeOutTimer;
    private bool isActive;

    private void Awake()
    {
        if (screenLight == null)
            screenLight = GetComponent<Light2D>();

        currentIntensity = 0f;
        screenLight.intensity = 0f;
    }

    private void Update()
    {
        if (!isActive)
            return;

        // пока таймер > 0 Ч держим / усиливаем свет
        if (fadeOutTimer > 0f)
        {
            fadeOutTimer -= Time.deltaTime;

            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                maxIntensity,
                Time.deltaTime / fadeInDuration
            );
        }
        // иначе Ч плавно гасим
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
        isActive = true;

        // при каждом клике просто продлеваем жизнь импульса
        fadeOutTimer = holdBeforeFadeOut;
    }
}

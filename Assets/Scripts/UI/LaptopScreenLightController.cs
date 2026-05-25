using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaptopScreenLightController : MonoBehaviour
{
    private Light2D light2D;

    [SerializeField] private float baseIntensity = 0.03f;
    [SerializeField] private float pulseIntensity = 1.2f;
    [SerializeField] private float pulseSpeed = 15f;
    [SerializeField] private float returnSpeed = 1.5f;
    [SerializeField] private float breathingSpeed = 0.8f;
    [SerializeField] private float breathingAmount = 0.02f;

    private float currentIntensity;
    private bool isPulsing;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        currentIntensity = baseIntensity;
    }

    void Update()
    {
        if (isPulsing)
        {
            currentIntensity = Mathf.Lerp(
                currentIntensity, pulseIntensity, Time.deltaTime * pulseSpeed);
            if (currentIntensity >= pulseIntensity * 0.95f)
                isPulsing = false;
        }
        else
        {
            float breathing = Mathf.Sin(Time.time * breathingSpeed)
                * breathingAmount;
            float target = baseIntensity + breathing;
            currentIntensity = Mathf.Lerp(
                currentIntensity, target, Time.deltaTime * returnSpeed);
        }

        light2D.intensity = currentIntensity;
    }

    public void TriggerLightPulse()
    {
        isPulsing = true;
    }
}

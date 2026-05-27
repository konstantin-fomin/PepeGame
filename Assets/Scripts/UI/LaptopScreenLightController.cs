using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LaptopScreenLightController : MonoBehaviour
{
    [SerializeField] private Light2D light2D;

    [SerializeField] private Light2D characterLight;
    [SerializeField] private float baseIntensity = 0.03f;
    [SerializeField] private float pulseIntensity = 1.2f;
    [SerializeField] private float pulseSpeed = 15f;
    [SerializeField] private float returnSpeed = 1.5f;
    [SerializeField] private float breathingSpeed = 0.8f;
    [SerializeField] private float breathingAmount = 0.02f;

    [SerializeField] private float characterBaseIntensity = 0.6f;
    [SerializeField] private float characterPulseIntensity = 1.5f;

    private float currentIntensity;
    private float currentCharacterIntensity;
    private bool isPulsing;
    private float activePulseIntensity;
    private float activeCharacterPulseIntensity;

    void Start()
    {
        currentIntensity = baseIntensity;
        currentCharacterIntensity = characterBaseIntensity;
        activePulseIntensity = pulseIntensity;
        activeCharacterPulseIntensity = characterPulseIntensity;
    }

    void Update()
    {
        if (isPulsing)
        {
            currentIntensity = Mathf.Lerp(
                currentIntensity, activePulseIntensity, Time.deltaTime * pulseSpeed);
            currentCharacterIntensity = Mathf.Lerp(
                currentCharacterIntensity, activeCharacterPulseIntensity, Time.deltaTime * pulseSpeed);
            if (currentIntensity >= activePulseIntensity * 0.95f)
                isPulsing = false;
        }
        else
        {
            float breathing = Mathf.Sin(Time.time * breathingSpeed)
                * breathingAmount;
            float target = baseIntensity + breathing;
            currentIntensity = Mathf.Lerp(
                currentIntensity, target, Time.deltaTime * returnSpeed);

            float charBreathing = Mathf.Sin(Time.time * breathingSpeed)
                * (breathingAmount * 2f);
            float charTarget = characterBaseIntensity + charBreathing;
            currentCharacterIntensity = Mathf.Lerp(
                currentCharacterIntensity, charTarget, Time.deltaTime * returnSpeed);
        }

        light2D.intensity = currentIntensity;

        if (characterLight != null)
            characterLight.intensity = currentCharacterIntensity;
    }

    public void TriggerLightPulse()
    {
        TriggerLightPulse(1f);
    }

    public void TriggerLightPulse(float strengthMultiplier)
    {
        float safeMultiplier = Mathf.Max(1f, strengthMultiplier);
        activePulseIntensity = pulseIntensity * safeMultiplier;
        activeCharacterPulseIntensity = characterPulseIntensity * safeMultiplier;
        isPulsing = true;
    }
}

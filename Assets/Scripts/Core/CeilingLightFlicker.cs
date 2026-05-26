using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CeilingLightFlicker : MonoBehaviour
{
    private Light2D light2D;

    [SerializeField] private float minIntensity = 0.55f;
    [SerializeField] private float maxIntensity = 0.75f;
    [SerializeField] private float flickerSpeed = 0.08f;
    [SerializeField] private float flickerChance = 0.04f;
    [SerializeField] private float blackoutChance = 0.008f;
    [SerializeField] private float flickerMinTarget = 0.15f;
    [SerializeField] private float normalMinTarget = 0.35f;

    private float timer;
    private float targetIntensity;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        Debug.Log($"CeilingLightFlicker: light2D = {light2D}");
        targetIntensity = maxIntensity;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            bool flicker = Random.value < flickerChance;
            targetIntensity = flicker ?
                Random.Range(minIntensity, flickerMinTarget) :
                Random.Range(normalMinTarget, maxIntensity);

            if (Random.value < blackoutChance)
            {
                targetIntensity = 0f;
                timer = Random.Range(0.15f, 0.35f);
            }
            else
            {
                timer = Random.Range(0.08f, 0.25f);
            }
        }

        light2D.intensity = Mathf.Lerp(
            light2D.intensity, targetIntensity, Time.deltaTime * 20f);
    }
}

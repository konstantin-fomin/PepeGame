using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CeilingLightFlicker : MonoBehaviour
{
    private Light2D light2D;

    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 0.4f;
    [SerializeField] private float flickerSpeed = 0.15f;

    private float timer;
    private float targetIntensity;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        targetIntensity = maxIntensity;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // Случайное мигание
            bool flicker = Random.value < 0.05f;
            targetIntensity = flicker ?
                Random.Range(minIntensity, 0.15f) :
                Random.Range(0.35f, maxIntensity);

            // Редкое длинное отключение
            if (Random.value < 0.01f)
            {
                targetIntensity = 0f;
                timer = Random.Range(0.2f, 0.4f);
            }
            else
            {
                timer = Random.Range(0.1f, 0.4f);
            }
        }

        light2D.intensity = Mathf.Lerp(
            light2D.intensity, targetIntensity, Time.deltaTime * 20f);
    }
}

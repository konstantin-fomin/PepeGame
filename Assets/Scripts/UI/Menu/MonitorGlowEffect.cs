using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MonitorGlowEffect : MonoBehaviour
{
    [SerializeField] private Image glowImage;
    [SerializeField] private Material glowMaterial;
    [SerializeField] private float pulseSpeed  = 0.6f;
    [SerializeField] private float pulseAmount = 0.04f;
    [SerializeField] private float baseAlpha   = 0.25f;

    private bool isFading = false;
    private float fadeMultiplier = 1f;

    private void Start()
    {
        glowMaterial = new Material(glowMaterial);
        GetComponent<Image>().material = glowMaterial;
    }

    private void Update()
    {
        if (isFading) return;

        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        glowMaterial.SetFloat("_Intensity", (baseAlpha + pulse) * fadeMultiplier);
    }

    public IEnumerator FadeOut(float duration)
    {
        isFading = true;
        float startIntensity = glowMaterial.GetFloat("_Intensity");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            glowMaterial.SetFloat("_Intensity", Mathf.Lerp(startIntensity, 0f, t));
            yield return null;
        }

        glowMaterial.SetFloat("_Intensity", 0f);
        fadeMultiplier = 0f;
        isFading = false;
    }

    public void ResetGlow()
    {
        isFading = false;
        fadeMultiplier = 1f;

        if (glowMaterial != null)
            glowMaterial.SetFloat("_Intensity", baseAlpha);
    }
}

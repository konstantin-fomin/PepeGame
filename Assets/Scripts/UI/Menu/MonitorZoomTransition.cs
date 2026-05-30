using UnityEngine;
using System.Collections;

public class MonitorZoomTransition : MonoBehaviour
{
    [SerializeField] private RectTransform mainMenuRect;
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup buttonsGroup;
    [SerializeField] private MonitorGlowEffect monitorGlow;

    public IEnumerator PlayZoomIn(System.Action onComplete)
    {
        mainMenuCanvasGroup.interactable = false;

        StartCoroutine(FadeButtons());

        if (monitorGlow != null)
            StartCoroutine(monitorGlow.FadeOut(0.3f));

        Vector3 startScale = mainMenuRect.localScale;
        Vector3 endScale   = Vector3.one * 5f;
        Vector2 startPos   = mainMenuRect.anchoredPosition;

        float duration = 0.55f;
        float elapsed  = 0f;

        StartCoroutine(DelayedFade(duration * 0.4f));

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            mainMenuRect.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        mainMenuRect.localScale = endScale;

        yield return new WaitForSeconds(0.15f);

        mainMenuRect.localScale = startScale;

        onComplete?.Invoke();
    }

    private IEnumerator FadeButtons()
    {
        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            buttonsGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        buttonsGroup.alpha = 0f;
    }

    private IEnumerator DelayedFade(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(ScreenFader.Instance.FadeOut(0.35f));
    }

    public void ResetMonitor()
    {
        mainMenuRect.localScale = Vector3.one;
        if (mainMenuCanvasGroup != null)
            mainMenuCanvasGroup.interactable = true;
        if (buttonsGroup != null)
            buttonsGroup.alpha = 1f;
        if (monitorGlow != null)
            monitorGlow.ResetGlow();
    }
}

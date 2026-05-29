using UnityEngine;
using TMPro;
using System.Collections;

public class OfficeEventToastView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI kpiText;

    [Header("Animation")]
    [SerializeField] private float slideOffset = 300f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private RectTransform rectTransform;
    private Vector2 targetPos;
    private Coroutine showCoroutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
            targetPos = rectTransform.anchoredPosition;
    }

    public void Show(OfficeEventManager.OfficeEventToastResult result)
    {
        if (result == null) return;

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        if (messageText != null)
            messageText.text = result.message;

        if (kpiText != null)
        {
            if (result.kpiReward > 0)
            {
                kpiText.text = "+" + NumberFormatter.Format(result.kpiReward) + " KPI";
                kpiText.gameObject.SetActive(true);
            }
            else
            {
                kpiText.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        AudioManager.Instance?.PlayToast();
        showCoroutine = StartCoroutine(ShowRoutine(result.displaySeconds));
    }

    public void Hide()
    {
        StopAllCoroutines();
        showCoroutine = null;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (rectTransform != null)
            rectTransform.anchoredPosition = targetPos;

        gameObject.SetActive(false);
    }

    private IEnumerator ShowRoutine(float displaySeconds)
    {
        // --- Slide in from right ---
        canvasGroup.alpha = 0f;
        Vector2 startPos = targetPos + new Vector2(slideOffset, 0f);
        if (rectTransform != null)
            rectTransform.anchoredPosition = startPos;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeInDuration);
            float ease = EaseOutBack(p);

            canvasGroup.alpha = Mathf.Clamp01(p * 2.5f);
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, ease);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        if (rectTransform != null)
            rectTransform.anchoredPosition = targetPos;

        // --- Wait ---
        float waitTime = displaySeconds > 0f ? displaySeconds : Random.Range(3.5f, 4.5f);
        yield return new WaitForSecondsRealtime(waitTime);

        // --- Slide out to right ---
        Vector2 endPos = targetPos + new Vector2(slideOffset, 0f);
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            float ease = EaseInQuad(p);

            canvasGroup.alpha = 1f - ease;
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.Lerp(targetPos, endPos, ease);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        if (rectTransform != null)
            rectTransform.anchoredPosition = targetPos;

        gameObject.SetActive(false);
        showCoroutine = null;
    }

    private static float EaseOutBack(float t)
    {
        float c = 1.4f;
        return 1f + (c + 1f) * Mathf.Pow(t - 1f, 3f) + c * Mathf.Pow(t - 1f, 2f);
    }

    private static float EaseInQuad(float t)
    {
        return t * t;
    }
}

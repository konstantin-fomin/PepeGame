using UnityEngine;
using TMPro;
using System.Collections;

public class OfficeEventToastView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;

    private RectTransform rectTransform;
    private Vector2 targetPos;
    private Coroutine showCoroutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            targetPos = rectTransform.anchoredPosition;
        }
    }

    public void Show(OfficeEventManager.OfficeEventToastResult result)
    {
        if (result == null) return;

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }

        if (messageText != null)
        {
            messageText.text = result.message;
        }

        gameObject.SetActive(true);
        showCoroutine = StartCoroutine(ShowRoutine(result.displaySeconds));
    }

    public void Hide()
    {
        StopAllCoroutines();
        showCoroutine = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPos;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator ShowRoutine(float displaySeconds)
    {
        canvasGroup.alpha = 0f;

        Vector2 startPos = targetPos + new Vector2(-8f, 0f);
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPos;
        }

        float fadeInDuration = 0.2f;
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / fadeInDuration);
            canvasGroup.alpha = progress;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, progress);
            }
            yield return null;
        }
        canvasGroup.alpha = 1f;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = targetPos;
        }

        float waitTime = displaySeconds > 0f ? displaySeconds : Random.Range(3.5f, 4.5f);
        yield return new WaitForSecondsRealtime(waitTime);

        float fadeOutDuration = 0.3f;
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        showCoroutine = null;
    }
}

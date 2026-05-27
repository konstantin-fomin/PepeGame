using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OfficeEventPopupView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private CanvasGroup canvasGroup;

    private OfficeEventData currentEvent;
    private float lifetime;
    private float elapsed;
    private bool isShowing = false;
    private Coroutine lifetimeCoroutine;

private void Awake()
    {
        CacheMissingReferences();

        if (acceptButton != null)
        {
            acceptButton.gameObject.SetActive(false);
        }

        SetHiddenImmediate();
    }

public void Show(OfficeEventManager.OfficeEventToastResult result)
    {
        if (result == null) return;

        CacheMissingReferences();

        currentEvent = result.eventData;
        elapsed      = 0f;
        lifetime     = result.displaySeconds;
        isShowing    = true;

        if (titleText != null)       titleText.text       = currentEvent != null ? currentEvent.title : string.Empty;
        if (descriptionText != null) descriptionText.text = result.message;
        if (effectText != null)      effectText.text      = string.Empty;
        if (timerText != null)       timerText.text       = string.Empty;
        if (buttonText != null)      buttonText.text      = string.Empty;
        if (acceptButton != null)    acceptButton.gameObject.SetActive(false);

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (isActiveAndEnabled)
        {
            StartCoroutine(SlideIn());
        }
    }

public void Show(OfficeEventData ev)
    {
        if (ev == null) return;

        Show(new OfficeEventManager.OfficeEventToastResult
        {
            eventData = ev,
            message = ev.description,
            kpiReward = 0,
            hasTemporaryBuff = false,
            displaySeconds = ev.popupLifetimeSeconds
        });
    }


    public void Hide()
    {
        if (!isShowing) return;
        isShowing = false;
        if (lifetimeCoroutine != null) StopCoroutine(lifetimeCoroutine);
        StartCoroutine(SlideOut());
    }

private void OnAccept()
    {
        Hide();
    }

private IEnumerator LifetimeCountdown()
    {
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float remaining = lifetime - elapsed;
            if (timerText != null)
            {
                timerText.text = $"{Mathf.CeilToInt(remaining)}с";
            }
            yield return null;
        }
        Hide();
    }

private IEnumerator SlideIn()
    {
        if (canvasGroup == null || popupRect == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;
        Vector2 startPos = new Vector2(400f, popupRect.anchoredPosition.y);
        Vector2 endPos   = new Vector2(0f,   popupRect.anchoredPosition.y);
        popupRect.anchoredPosition = startPos;

        float duration = 0.3f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / duration);
            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            canvasGroup.alpha = progress;
            yield return null;
        }
        popupRect.anchoredPosition = endPos;
        canvasGroup.alpha = 1f;
    }

private IEnumerator SlideOut()
    {
        if (canvasGroup == null || popupRect == null)
        {
            SetHiddenImmediate();
            yield break;
        }

        Vector2 startPos = popupRect.anchoredPosition;
        Vector2 endPos   = new Vector2(400f, startPos.y);

        float duration = 0.2f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            popupRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            canvasGroup.alpha = 1f - progress;
            yield return null;
        }
        SetHiddenImmediate();
    }

private void SetHiddenImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (popupRect != null)
        {
            Vector2 hiddenPos = popupRect.anchoredPosition;
            hiddenPos.x = 400f;
            popupRect.anchoredPosition = hiddenPos;
        }
    }


private void CacheMissingReferences()
    {
        if (popupRect == null) popupRect = transform as RectTransform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        if (titleText == null)
        {
            Transform title = transform.Find("TitleText");
            if (title != null) titleText = title.GetComponent<TextMeshProUGUI>();
        }

        if (descriptionText == null)
        {
            Transform description = transform.Find("DescText");
            if (description != null) descriptionText = description.GetComponent<TextMeshProUGUI>();
        }

        if (effectText == null)
        {
            Transform effect = transform.Find("EffectText");
            if (effect != null) effectText = effect.GetComponent<TextMeshProUGUI>();
        }

        if (timerText == null)
        {
            Transform timer = transform.Find("TimerText");
            if (timer != null) timerText = timer.GetComponent<TextMeshProUGUI>();
        }

        if (acceptButton == null)
        {
            Transform button = transform.Find("AcceptButton");
            if (button != null) acceptButton = button.GetComponent<Button>();
        }

        if (buttonText == null && acceptButton != null)
        {
            buttonText = acceptButton.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class OfficeEventPopupView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button dismissButton;

    private Action onAcceptCallback;
    private bool isShowing = false;

    private void Awake()
    {
        CacheMissingReferences();

        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(OnDismissClicked);
        }

        SetHiddenImmediate();
    }

    public void Show(ActiveOfficeEventData data, Action onAccept)
    {
        if (data == null) return;

        CacheMissingReferences();

        onAcceptCallback = onAccept;
        isShowing = true;

        if (titleText != null)       titleText.text       = LocalizedField(data, 0, data.title);
        if (descriptionText != null) descriptionText.text = LocalizedField(data, 1, data.description);
        if (effectText != null)      effectText.text      = LocalizedField(data, 2, data.effectDescription);
        if (timerText != null)       timerText.text       = LocalizedField(data, 3, data.buttonText);

        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (isActiveAndEnabled)
        {
            AudioManager.Instance?.PlayPopupSwosh();
            StartCoroutine(SlideIn());
        }
    }

    public void Hide()
    {
        if (!isShowing) return;
        isShowing = false;
        onAcceptCallback = null;
        StartCoroutine(SlideOut());
    }

    private void OnDismissClicked()
    {
        if (!isShowing) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPopup();

        Action callback = onAcceptCallback;
        onAcceptCallback = null;
        isShowing = false;
        StartCoroutine(SlideOut());

        callback?.Invoke();
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

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator SlideOut()
    {
        if (canvasGroup == null || popupRect == null)
        {
            SetHiddenImmediate();
            yield break;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

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

    // Localized text for one of the four consecutive ids (offset 0=title,1=desc,2=effect,3=button).
    // Falls back to the asset value if no LocalizationManager / translation is available.
    private string LocalizedField(ActiveOfficeEventData data, int offset, string fallback)
    {
        var loc = LocalizationManager.Instance;
        if (loc != null && !string.IsNullOrEmpty(data.locId))
        {
            string key = offset == 0 ? data.locId : loc.OffsetId(data.locId, offset);
            if (!string.IsNullOrEmpty(key) && loc.TryGet(key, out string v))
                return v;
        }
        return fallback ?? string.Empty;
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
    }
}

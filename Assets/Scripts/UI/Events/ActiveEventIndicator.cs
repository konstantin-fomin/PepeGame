using UnityEngine;
using TMPro;

public class ActiveEventIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indicatorText;
    [SerializeField] private CanvasGroup canvasGroup;

    private string activeTitle;
    private float activeTimeRemaining;
    private bool isActiveFromFlavorFeed = false;
    private bool isActiveFromActiveEvents = false;

    private void Start()
    {
        gameObject.SetActive(false);

        if (OfficeEventManager.Instance != null)
        {
            OfficeEventManager.Instance.OnEventActivated += OnFlavorFeedActivated;
            OfficeEventManager.Instance.OnEventEnded     += OnFlavorFeedEnded;
        }

        ActiveOfficeEventManager activeManager = FindObjectOfType<ActiveOfficeEventManager>();
        if (activeManager != null)
        {
            activeManager.OnActiveEventStarted += OnActiveEventStarted;
            activeManager.OnActiveEventEnded   += OnActiveEventEnded;
        }
    }

    private void OnDestroy()
    {
        if (OfficeEventManager.Instance != null)
        {
            OfficeEventManager.Instance.OnEventActivated -= OnFlavorFeedActivated;
            OfficeEventManager.Instance.OnEventEnded     -= OnFlavorFeedEnded;
        }

        ActiveOfficeEventManager activeManager = FindObjectOfType<ActiveOfficeEventManager>();
        if (activeManager != null)
        {
            activeManager.OnActiveEventStarted -= OnActiveEventStarted;
            activeManager.OnActiveEventEnded   -= OnActiveEventEnded;
        }
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        if (isActiveFromFlavorFeed)
        {
            OfficeEventManager mgr = OfficeEventManager.Instance;
            if (mgr != null && mgr.HasActiveEvent)
            {
                indicatorText.text =
                    $"{mgr.ActiveEvent.title}: {Mathf.CeilToInt(mgr.ActiveTimeRemaining)}с";
            }
            return;
        }

        if (isActiveFromActiveEvents)
        {
            activeTimeRemaining -= Time.deltaTime;
            if (activeTimeRemaining < 0f) activeTimeRemaining = 0f;
            indicatorText.text =
                $"{activeTitle}: {Mathf.CeilToInt(activeTimeRemaining)}с";
        }
    }

    private void OnFlavorFeedActivated(OfficeEventData ev)
    {
        isActiveFromFlavorFeed = true;
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    private void OnFlavorFeedEnded()
    {
        isActiveFromFlavorFeed = false;
        if (!isActiveFromActiveEvents)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnActiveEventStarted(string title, float duration)
    {
        activeTitle = title;
        activeTimeRemaining = duration;
        isActiveFromActiveEvents = true;
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    private void OnActiveEventEnded()
    {
        isActiveFromActiveEvents = false;
        if (!isActiveFromFlavorFeed)
        {
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using TMPro;

public class ActiveEventIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indicatorText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        gameObject.SetActive(false);

        OfficeEventManager.Instance.OnEventActivated += OnActivated;
        OfficeEventManager.Instance.OnEventEnded     += OnEnded;
    }

    private void OnDestroy()
    {
        if (OfficeEventManager.Instance == null) return;
        OfficeEventManager.Instance.OnEventActivated -= OnActivated;
        OfficeEventManager.Instance.OnEventEnded     -= OnEnded;
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        var mgr = OfficeEventManager.Instance;
        if (mgr == null || !mgr.HasActiveEvent) return;
        indicatorText.text =
            $"{mgr.ActiveEvent.title}: {Mathf.CeilToInt(mgr.ActiveTimeRemaining)}с";
    }

    private void OnActivated(OfficeEventData ev)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    private void OnEnded()
    {
        gameObject.SetActive(false);
    }
}

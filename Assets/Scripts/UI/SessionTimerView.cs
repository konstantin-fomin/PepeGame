using UnityEngine;
using TMPro;

public class SessionTimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float sessionSeconds = 0f;
    private bool isRunning = false;
    private bool subscribed;

    public void StartTimer() => isRunning = true;
    public void StopTimer()  => isRunning = false;

    public void ResetTimer()
    {
        sessionSeconds = 0f;
        isRunning = false;
        if (timerText != null) timerText.text = FormatTime(0f);
    }

    private void OnEnable()
    {
        if (!subscribed && LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += RefreshLabel;
            subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (subscribed && LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshLabel;
        subscribed = false;
    }

    private void RefreshLabel()
    {
        if (timerText != null) timerText.text = FormatTime(sessionSeconds);
    }

    private void Update()
    {
        if (!isRunning) return;
        sessionSeconds += Time.deltaTime;
        timerText.text = FormatTime(sessionSeconds);
    }

    private string FormatTime(float totalSeconds)
    {
        int h = (int)(totalSeconds / 3600);
        int m = (int)(totalSeconds % 3600) / 60;
        int s = (int)(totalSeconds % 60);

        // "ч/м/с" unit suffixes are kept as-is (universal); only the "в офисе:" prefix is localized.
        string timePart = h > 0 ? $"{h}ч {m:00}м" : $"{m}м {s:00}с";
        string template = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.Get("LOC_0031")
            : "в офисе: {0}";
        return string.Format(template, timePart);
    }
}

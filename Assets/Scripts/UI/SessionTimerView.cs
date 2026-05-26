using UnityEngine;
using TMPro;

public class SessionTimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float sessionSeconds = 0f;
    private bool isRunning = false;

    public void StartTimer() => isRunning = true;
    public void StopTimer()  => isRunning = false;

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

        if (h > 0)
            return $"в офисе: {h}ч {m:00}м";
        else
            return $"в офисе: {m}м {s:00}с";
    }
}

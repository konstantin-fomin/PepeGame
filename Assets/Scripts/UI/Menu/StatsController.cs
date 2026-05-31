using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private TextMeshProUGUI kpiText;
    [SerializeField] private TextMeshProUGUI clicksText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI upgradesText;
    [SerializeField] private TextMeshProUGUI hrCommentText;
    [SerializeField] private Button backButton;

    // HR flavor lines — localization keys (CSV) + RU fallbacks.
    private static readonly string[] hrCommentKeys = {
        "LOC_0039", "LOC_0040", "LOC_0041", "LOC_0042",
        "LOC_0043", "LOC_0044", "LOC_0045", "LOC_0046"
    };

    private static readonly string[] hrCommentFallback = {
        "Показатели приемлемые. Отдых не одобрен.",
        "Компания ценит ваше потраченное время.",
        "Продуктивность на уровне. Ожиданий нет.",
        "HR доволен. Это подозрительно.",
        "Вы кликаете. Это уже больше чем от вас ждали.",
        "Прогресс зафиксирован. Премия не предусмотрена.",
        "Данные получены. Выводы неутешительны.",
        "Спасибо за службу. Продолжайте страдать."
    };

    private bool subscribed;

    private void Start()
    {
        backButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.HideStats());
    }

    private void OnEnable()
    {
        if (!subscribed && LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += Refresh;
            subscribed = true;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (subscribed && LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        subscribed = false;
    }

    private static string L(string key, string fallback)
    {
        return LocalizationManager.Instance != null ? LocalizationManager.Instance.Get(key) : fallback;
    }

    private void Refresh()
    {
        var stats = StatsTracker.Instance;
        if (stats == null) return;

        string rankName = gameManager.CurrentRank?.rankName ?? "—";
        string careerStatus = gameManager.IsCareerCompleted
            ? L("LOC_0038", "Завершена")
            : L("LOC_0037", "Активна");

        playtimeText.text  = $"{L("LOC_0032", "Время в офисе:")} {FormatTime(stats.TotalPlaytimeSeconds)}";
        kpiText.text       = $"{L("LOC_0033", "KPI заработано:")} {NumberFormatter.Format(stats.TotalKpiEarned)}";
        clicksText.text    = $"{L("LOC_0034", "Кликов совершено:")} {NumberFormatter.Format(stats.TotalClicks)}";
        rankText.text      = $"{L("LOC_0035", "Текущий ранг:")} {rankName}";
        upgradesText.text  = $"{L("LOC_0036", "Карьера:")} {careerStatus}";

        int idx = Random.Range(0, hrCommentKeys.Length);
        hrCommentText.text = $"\"{L(hrCommentKeys[idx], hrCommentFallback[idx])}\"";
    }

    private string FormatTime(float totalSeconds)
    {
        int h = (int)(totalSeconds / 3600);
        int m = (int)(totalSeconds % 3600) / 60;
        if (h > 0) return $"{h}ч {m:00}м";
        return $"{m}м {(int)(totalSeconds % 60):00}с";
    }
}

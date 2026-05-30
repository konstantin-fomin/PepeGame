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

    private static readonly string[] hrComments = {
        "Показатели приемлемые. Отдых не одобрен.",
        "Компания ценит ваше потраченное время.",
        "Продуктивность на уровне. Ожиданий нет.",
        "HR доволен. Это подозрительно.",
        "Вы кликаете. Это уже больше чем от вас ждали.",
        "Прогресс зафиксирован. Премия не предусмотрена.",
        "Данные получены. Выводы неутешительны.",
        "Спасибо за службу. Продолжайте страдать."
    };

    private void Start()
    {
        backButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.HideStats());
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        var stats = StatsTracker.Instance;
        if (stats == null) return;

        string rankName = gameManager.CurrentRank?.rankName ?? "—";
        string careerStatus = gameManager.IsCareerCompleted ? "Завершена" : "Активна";

        playtimeText.text  = $"Время в офисе:      {FormatTime(stats.TotalPlaytimeSeconds)}";
        kpiText.text       = $"KPI заработано:     {NumberFormatter.Format(stats.TotalKpiEarned)}";
        clicksText.text    = $"Кликов совершено:   {NumberFormatter.Format(stats.TotalClicks)}";
        rankText.text      = $"Текущий ранг:       {rankName}";
        upgradesText.text  = $"Карьера:            {careerStatus}";
        hrCommentText.text = $"\"{hrComments[Random.Range(0, hrComments.Length)]}\"";
    }

    private string FormatTime(float totalSeconds)
    {
        int h = (int)(totalSeconds / 3600);
        int m = (int)(totalSeconds % 3600) / 60;
        if (h > 0) return $"{h}ч {m:00}м";
        return $"{m}м {(int)(totalSeconds % 60):00}с";
    }
}

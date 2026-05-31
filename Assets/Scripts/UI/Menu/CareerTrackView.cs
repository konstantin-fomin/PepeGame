using UnityEngine;
using TMPro;

public class CareerTrackView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] rankRows;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI xpUntilNextText;
    [SerializeField] private GameManager gameManager;

    private static readonly string[] rankNames = {
        "Intern", "Junior", "Middle", "Senior", "Lead", "CEO"
    };

    // Localization keys for the per-rank flavor lines (CSV RANKS rows).
    private static readonly string[] rankFlavorKeys = {
        "LOC_0261", "LOC_0262", "LOC_0263", "LOC_0264", "LOC_0265", "LOC_0266"
    };

    // RU fallbacks used only if no LocalizationManager is present.
    private static readonly string[] rankFlavorFallback = {
        "ещё верит в задачи",
        "уже понял, что всё горит",
        "чинит чужие костыли",
        "сам стал частью legacy",
        "отвечает за всё",
        "красиво смотрит на графики"
    };

    private static readonly Color completedColor = new Color(0.416f, 0.333f, 0.259f, 1f);
    private static readonly Color currentColor   = new Color(0.208f, 0.310f, 0.408f, 1f);
    private static readonly Color futureColor    = new Color(0.478f, 0.416f, 0.341f, 1f);

    private bool subscribed;

    private void OnEnable()
    {
        if (!subscribed && LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += Refresh;
            subscribed = true;
        }
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

    public void Refresh()
    {
        if (gameManager == null)
        {
            Debug.LogError("[CareerTrackView] GameManager is null");
            return;
        }

        int currentRankIndex = gameManager.CurrentRankIndex;

        for (int i = 0; i < rankRows.Length && i < rankNames.Length; i++)
        {
            if (rankRows[i] == null) continue;

            string marker;
            Color color;
            FontStyles fontStyle = FontStyles.Normal;

            if (i < currentRankIndex)
            {
                marker = "✓";
                color = completedColor;
            }
            else if (i == currentRankIndex)
            {
                marker = "▶";
                color = currentColor;
                fontStyle = FontStyles.Bold;
            }
            else
            {
                marker = "□";
                color = futureColor;
            }

            string flavor = L(rankFlavorKeys[i], rankFlavorFallback[i]);
            rankRows[i].text = string.Format("{0} {1}  {2}", marker, rankNames[i], flavor);
            rankRows[i].color = color;
            rankRows[i].fontStyle = fontStyle;
        }

        bool isCeo = currentRankIndex >= rankNames.Length - 1;

        if (!isCeo)
        {
            long currentXp = gameManager.CurrentExperience;
            long nextRankXp = gameManager.ExperienceToNextRank;

            if (xpText != null)
                xpText.text = string.Format("XP: {0} / {1}", currentXp, nextRankXp);

            if (xpUntilNextText != null)
            {
                xpUntilNextText.text = string.Format(L("LOC_0029", "До повышения: {0} XP"), nextRankXp - currentXp);
                xpUntilNextText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (xpText != null)
                xpText.text = "XP: MAX";

            if (xpUntilNextText != null)
                xpUntilNextText.text = L("LOC_0030", "Максимальный ранг достигнут");
        }
    }
}

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

    private static readonly string[] rankFlavors = {
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

            rankRows[i].text = string.Format("{0} {1}  {2}", marker, rankNames[i], rankFlavors[i]);
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
                xpUntilNextText.text = string.Format("До повышения: {0} XP", nextRankXp - currentXp);
                xpUntilNextText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (xpText != null)
                xpText.text = "XP: MAX";

            if (xpUntilNextText != null)
                xpUntilNextText.text = "Максимальный ранг достигнут";
        }
    }
}

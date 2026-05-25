using UnityEngine;
using System.Collections;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float fadeDuration = 0.4f;

    private void OnEnable()
    {
        gameManager.OnRankUp += OnRankUp;
    }

    private void OnDisable()
    {
        gameManager.OnRankUp -= OnRankUp;
    }

    private void Start()
    {
        if (gameManager.CurrentRank != null && gameManager.CurrentRank.backgroundSprite != null)
            backgroundRenderer.sprite = gameManager.CurrentRank.backgroundSprite;
    }

    private void OnRankUp(RankData previousRank, RankData newRank)
    {
        if (newRank.backgroundSprite == null) return;
        StartCoroutine(FadeBackground(newRank.backgroundSprite));
    }

    private IEnumerator FadeBackground(Sprite newSprite)
    {
        float elapsed = 0f;
        Color c = backgroundRenderer.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            backgroundRenderer.color = c;
            yield return null;
        }

        backgroundRenderer.sprite = newSprite;

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            backgroundRenderer.color = c;
            yield return null;
        }
        c.a = 1f;
        backgroundRenderer.color = c;
    }
}

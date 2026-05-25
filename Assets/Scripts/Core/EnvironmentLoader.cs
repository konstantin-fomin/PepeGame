using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnvironmentLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;

    private string currentEnvironmentScene = "";
    private string pendingScene = "";

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
        string sceneName = gameManager.CurrentRank?.environmentScene;
        if (!string.IsNullOrEmpty(sceneName))
            StartCoroutine(LoadEnvironment(sceneName, animate: false));
    }

    private void OnRankUp(RankData previousRank, RankData newRank)
    {
        pendingScene = newRank.environmentScene;
        uiController.RankUpPopup.OnPopupClosed += OnPopupClosed;
    }

    private void OnPopupClosed()
    {
        uiController.RankUpPopup.OnPopupClosed -= OnPopupClosed;
        if (!string.IsNullOrEmpty(pendingScene))
            StartCoroutine(LoadEnvironment(pendingScene, animate: true));
    }

    private IEnumerator LoadEnvironment(string sceneName, bool animate)
    {
        if (sceneName == currentEnvironmentScene) yield break;

        if (animate)
            yield return StartCoroutine(ScreenFader.Instance.FadeOut(0.3f));

        if (!string.IsNullOrEmpty(currentEnvironmentScene))
        {
            Scene old = SceneManager.GetSceneByName(currentEnvironmentScene);
            if (old.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(old);
                while (!unload.isDone) yield return null;
            }
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone) yield return null;

        currentEnvironmentScene = sceneName;
        pendingScene = "";

        if (animate)
            yield return StartCoroutine(ScreenFader.Instance.FadeIn(0.4f));
    }
}

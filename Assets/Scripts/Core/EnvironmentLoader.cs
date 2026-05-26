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
            StartCoroutine(LoadInitial(sceneName));
    }

    private IEnumerator LoadInitial(string sceneName)
    {
        ScreenFader.Instance.SetBlack();

        bool sceneLoaded = false;
        StartCoroutine(LoadEnvironment(sceneName, false,
            delegate { sceneLoaded = true; }));

        yield return StartCoroutine(
            MonitorStartupEffect.Instance.PlayLinePhase());

        yield return StartCoroutine(
            MonitorStartupEffect.Instance.PlayWaitLoop(() => sceneLoaded));

        yield return StartCoroutine(
            MonitorStartupEffect.Instance.PlayRevealPhase());
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

    private IEnumerator LoadEnvironment(string sceneName, bool animate,
        System.Action onDone = null)
    {
        if (sceneName == currentEnvironmentScene) yield break;

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

        onDone?.Invoke();
    }
}

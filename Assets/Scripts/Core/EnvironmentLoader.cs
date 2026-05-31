using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnvironmentLoader : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;
    [SerializeField] private OfflineProgressPopupView offlinePopup;
    [SerializeField] private FirstLaunchTutorialPopupView tutorialPopup;

    private string currentEnvironmentScene = "";
    private string pendingScene = "";
    private bool pendingLoadSave;

    private void OnEnable()
    {
        gameManager.OnRankUp += OnRankUp;
    }

    private void OnDisable()
    {
        gameManager.OnRankUp -= OnRankUp;
    }

    public void StartGameFlow(bool loadSave)
    {
        pendingLoadSave = loadSave;
        string sceneName = gameManager.CurrentRank?.environmentScene;
        if (!string.IsNullOrEmpty(sceneName))
            StartCoroutine(LoadInitial(sceneName));
    }

    public void UnloadCurrentEnvironment()
    {
        if (!string.IsNullOrEmpty(currentEnvironmentScene))
            StartCoroutine(UnloadScene(currentEnvironmentScene));
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            while (!op.isDone) yield return null;
        }
        currentEnvironmentScene = "";
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

        if (pendingLoadSave && offlinePopup != null)
        {
            OfflineProgressResult result = gameManager.PendingOfflineResult;
            if (result.shouldShowPopup)
            {
                offlinePopup.Show(result);
                gameManager.ClearPendingOfflineResult();
            }
        }

        // Guard: the player may have left to the main menu during the startup
        // animation above. Don't pop the tutorial over the menu.
        bool stillInGame = MenuNavigationController.Instance == null
            || MenuNavigationController.Instance.IsGameActive;

        if (stillInGame && !pendingLoadSave && tutorialPopup != null && !gameManager.HasSeenTutorial)
        {
            tutorialPopup.OnPopupClosed += OnTutorialClosed;
            tutorialPopup.Show();
        }
    }

    private void OnTutorialClosed()
    {
        tutorialPopup.OnPopupClosed -= OnTutorialClosed;
        gameManager.MarkTutorialSeen();
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

        if (animate && ScreenFader.Instance != null)
        {
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(ScreenFader.Instance.FadeIn(0.5f));
        }
    }
}

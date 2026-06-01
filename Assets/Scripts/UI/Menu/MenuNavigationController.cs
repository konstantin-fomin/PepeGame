using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MenuNavigationController : MonoBehaviour
{
    public static MenuNavigationController Instance;

    public enum MenuState { MainMenu, Game, Pause, Settings, Confirm }

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameHudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private ConfirmDialogController confirmDialog;

    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnvironmentLoader environmentLoader;
    [SerializeField] private MonitorZoomTransition zoomTransition;
    [SerializeField] private FirstLaunchTutorialPopupView tutorialPopup;

    private MenuState currentState;
    private MenuState settingsPreviousState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetActive(gameHudPanel, false);
        bool hasSave = PlayerPrefs.HasKey("GAME_SAVE_V2");
        ShowMainMenu(hasSave);
    }

    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            HandleEscape();
    }

    private void HandleEscape()
    {
        if (OfflineProgressPopupView.IsShowing) return;
        if (FirstLaunchTutorialPopupView.IsShowing) return;
        if (CareerCompletedScreenView.IsShowing) return;

        switch (currentState)
        {
            case MenuState.Game:     ShowPause();           break;
            case MenuState.Pause:    ResumeGame();          break;
            case MenuState.Settings: BackFromSettings();    break;
            case MenuState.Confirm:  confirmDialog.gameObject.SetActive(false);
                                     SetState(settingsPreviousState); break;
        }
    }

    // --- Main Menu ---
    public void ShowMainMenu(bool hasSave)
    {
        Time.timeScale = 1f;

        if (currentState == MenuState.Game || currentState == MenuState.Pause)
            environmentLoader?.UnloadCurrentEnvironment();

        FindObjectOfType<SessionTimerView>()?.StopTimer();
        StatsTracker.Instance?.StopTracking();
        StatsTracker.Instance?.Save();
        OfficeEventManager.Instance?.StopEventSystem();
        FindObjectOfType<ActiveOfficeEventManager>()?.StopSystem();
        AudioManager.Instance?.StopSpecialLoop();

        tutorialPopup?.HideImmediate();

        SetActive(mainMenuPanel, true);
        SetActive(gameHudPanel, false);
        SetActive(pausePanel, false);
        SetActive(settingsPanel, false);
        SetActive(statsPanel, false);
        SetState(MenuState.MainMenu);

        zoomTransition?.ResetMonitor();

        mainMenuPanel.GetComponent<MainMenuController>()?.SetHasSave(hasSave);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    // --- Game ---
    public void StartGame(bool loadSave)
    {
        SetActive(mainMenuPanel, false);
        SetActive(gameHudPanel, true);
        SetActive(pausePanel, false);
        SetActive(settingsPanel, false);
        SetActive(statsPanel, false);
        SetState(MenuState.Game);

        environmentLoader.StartGameFlow(loadSave);

        if (AudioManager.Instance != null && gameManager.CurrentRank != null)
            AudioManager.Instance.PlayMusicForRank(gameManager.CurrentRank);

        SessionTimerView sessionTimer = FindObjectOfType<SessionTimerView>();
        if (!loadSave)
        {
            sessionTimer?.ResetTimer();
        }
        sessionTimer?.StartTimer();

        StatsTracker.Instance?.StartTracking();
        gameManager?.StartSpecialLoopIfNeeded();
        OfficeEventManager.Instance?.StartEventSystem();
        FindObjectOfType<ActiveOfficeEventManager>()?.StartSystem();
    }

    public void StartGameWithZoom(bool loadSave, System.Action beforeStart = null)
    {
        StartCoroutine(ZoomThenStart(loadSave, beforeStart));
    }

    private IEnumerator ZoomThenStart(bool loadSave, System.Action beforeStart)
    {
        yield return StartCoroutine(
            zoomTransition.PlayZoomIn(() => { }));
        beforeStart?.Invoke();
        StartGame(loadSave);
    }

    // --- Pause ---
    public void ShowPause()
    {
        SetActive(pausePanel, true);
        SetState(MenuState.Pause);
        Time.timeScale = 0f;
        FindObjectOfType<CareerTrackView>()?.Refresh();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        SetActive(pausePanel, false);
        SetState(MenuState.Game);
    }

    // --- Settings ---
    public void ShowSettings()
    {
        settingsPreviousState = currentState;
        SetActive(settingsPanel, true);
        SetState(MenuState.Settings);
    }

    public void BackFromSettings()
    {
        SetActive(settingsPanel, false);
        SetState(settingsPreviousState);

        if (settingsPreviousState == MenuState.MainMenu)
            SetActive(mainMenuPanel, true);
        else if (settingsPreviousState == MenuState.Pause)
            SetActive(pausePanel, true);
    }

    // --- Stats ---
    public void ShowStats()
    {
        settingsPreviousState = currentState;
        SetActive(statsPanel, true);
        SetState(MenuState.Settings);
    }

    public void HideStats()
    {
        SetActive(statsPanel, false);

        if (settingsPreviousState == MenuState.MainMenu)
            SetActive(mainMenuPanel, true);
        else if (settingsPreviousState == MenuState.Pause)
            SetActive(pausePanel, true);

        SetState(settingsPreviousState);
    }

    // --- Confirm Dialogs ---
    private static string L(string key, string fallback)
    {
        return LocalizationManager.Instance != null ? LocalizationManager.Instance.Get(key) : fallback;
    }

    public void ConfirmNewCareer()
    {
        confirmDialog.Show(
            L("LOC_0056", "Новая карьера"),
            L("LOC_0057", "Начать заново?\nВесь прогресс будет сброшен."),
            onConfirm: () => {
                StartGameWithZoom(
                    loadSave: false,
                    beforeStart: () => gameManager.ResetProgress()
                );
            }
        );
    }

    // Full reset to a "first launch" state, then refresh the main menu so it
    // looks like a brand-new install (no Continue). Called from the menu RESET button.
    public void ConfirmResetProgress()
    {
        confirmDialog.Show(
            L("LOC_0058", "Сброс прогресса"),
            L("LOC_0059", "Сбросить весь прогресс?\nИгра начнётся с нуля, как при первом запуске."),
            onConfirm: () => {
                gameManager.ResetProgress(saveAfterReset: false);
                ShowMainMenu(hasSave: false);
            }
        );
    }

    public void ConfirmReturnToMenu()
    {
        confirmDialog.Show(
            L("LOC_0010", "Главное меню"),
            L("LOC_0060", "Выйти в главное меню?\nПрогресс будет сохранён."),
            onConfirm: () => {
                gameManager.SaveGame();
                ShowMainMenu(hasSave: true);
            }
        );
    }

    public void ConfirmQuit()
    {
        confirmDialog.Show(
            L("LOC_0007", "Выход"),
            L("LOC_0061", "Выйти из игры?\nПрогресс будет сохранён."),
            onConfirm: () => {
                gameManager.SaveGame();
                Time.timeScale = 1f;
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        );
    }

    private void SetState(MenuState state) => currentState = state;

    private void SetActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    public MenuState CurrentState => currentState;

    public bool IsGameActive => currentState == MenuState.Game;

    public bool IsGameplayAvailableForOfficeEvents =>
        currentState == MenuState.Game &&
        IsPanelVisible(gameHudPanel) &&
        !IsPanelVisible(mainMenuPanel) &&
        !IsPanelVisible(pausePanel) &&
        !IsPanelVisible(settingsPanel) &&
        !IsPanelVisible(statsPanel) &&
        !IsConfirmDialogVisible;

    private bool IsConfirmDialogVisible =>
        confirmDialog != null && confirmDialog.gameObject.activeInHierarchy;

    private bool IsPanelVisible(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

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

    private MenuState currentState;
    private MenuState settingsPreviousState;

    private void Awake() => Instance = this;

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
        if (currentState == MenuState.Game || currentState == MenuState.Pause)
            environmentLoader?.UnloadCurrentEnvironment();

        FindObjectOfType<SessionTimerView>()?.StopTimer();
        StatsTracker.Instance?.StopTracking();
        StatsTracker.Instance?.Save();
        OfficeEventManager.Instance?.StopEventSystem();

        SetActive(mainMenuPanel, true);
        SetActive(gameHudPanel, false);
        SetActive(pausePanel, false);
        SetActive(settingsPanel, false);
        SetActive(statsPanel, false);
        SetState(MenuState.MainMenu);

        mainMenuPanel.GetComponent<MainMenuController>()?.SetHasSave(hasSave);
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
        FindObjectOfType<SessionTimerView>()?.StartTimer();
        StatsTracker.Instance?.StartTracking();
        OfficeEventManager.Instance?.StartEventSystem();
    }

    // --- Pause ---
    public void ShowPause()
    {
        SetActive(pausePanel, true);
        SetState(MenuState.Pause);
    }

    public void ResumeGame()
    {
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
    public void ConfirmNewCareer()
    {
        confirmDialog.Show(
            "Новая карьера",
            "Начать заново?\nВесь прогресс будет сброшен.",
            onConfirm: () => {
                gameManager.ResetProgress();
                StartGame(loadSave: false);
            }
        );
    }

    public void ConfirmReturnToMenu()
    {
        confirmDialog.Show(
            "Главное меню",
            "Выйти в главное меню?\nПрогресс будет сохранён.",
            onConfirm: () => {
                gameManager.SaveGame();
                ShowMainMenu(hasSave: true);
            }
        );
    }

    public void ConfirmQuit()
    {
        confirmDialog.Show(
            "Выход",
            "Выйти из игры?\nПрогресс будет сохранён.",
            onConfirm: () => {
                gameManager.SaveGame();
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

using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        resumeButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ResumeGame());
        settingsButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ShowSettings());
        statsButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ShowStats());
        mainMenuButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ConfirmReturnToMenu());
        quitButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ConfirmQuit());
    }
}

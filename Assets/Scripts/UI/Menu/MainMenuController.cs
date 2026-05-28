using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newCareerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        continueButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.StartGameWithZoom(loadSave: true));
        newCareerButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ConfirmNewCareer());
        settingsButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ShowSettings());
        statsButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ShowStats());
        quitButton.onClick.AddListener(() =>
            MenuNavigationController.Instance.ConfirmQuit());
    }

    public void SetHasSave(bool hasSave)
    {
        continueButton.interactable = hasSave;
        var cg = continueButton.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = hasSave ? 1f : 0.4f;
    }
}

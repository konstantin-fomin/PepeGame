using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newCareerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button statsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button resetButton;

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

        if (resetButton != null)
            resetButton.onClick.AddListener(() =>
                MenuNavigationController.Instance.ConfirmResetProgress());
    }

    public void SetHasSave(bool hasSave)
    {
        continueButton.interactable = hasSave;
        var cg = continueButton.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = hasSave ? 1f : 0.4f;

        // Nothing to reset on a fresh install — mirror the Continue button.
        if (resetButton != null)
        {
            resetButton.interactable = hasSave;
            var rcg = resetButton.GetComponent<CanvasGroup>();
            if (rcg != null) rcg.alpha = hasSave ? 1f : 0.4f;
        }
    }
}

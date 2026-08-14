using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private UIPanelFader mainMenuPanel;
    [SerializeField] private UIPanelFader levelSelectPanel;
    [SerializeField] private UIPanelFader settingsPanel;

    private UIPanelFader currentPanel;

    private void Start()
    {
        currentPanel = mainMenuPanel;
    }

    public void SwitchToPanel(UIPanelFader targetPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.FadeOut();
        }

        if (targetPanel != null)
        {
            targetPanel.FadeIn();
            currentPanel = targetPanel;
        }
    }

    // Call these from your Button OnClick() events in the Inspector:
    public void OpenSettings() => SwitchToPanel(settingsPanel);
    public void OpenLevelSelect() => SwitchToPanel(levelSelectPanel);
    public void BackToMainMenu() => SwitchToPanel(mainMenuPanel);
}
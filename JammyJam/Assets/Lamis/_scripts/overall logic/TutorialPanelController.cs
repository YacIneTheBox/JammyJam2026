using UnityEngine;

public class TutorialPanelController : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject tutorialPanel;

    /// <summary>
    /// Call this from your '?' Button OnClick()
    /// </summary>
    public void OpenTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Call this from a Close/Back Button inside the Tutorial panel
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Call this if you want pressing '?' to toggle open/close
    /// </summary>
    public void ToggleTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }
}
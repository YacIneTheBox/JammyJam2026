using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Transition")]
    public ScreenTransitionManager transitionManager;

    [Header("References")]
    public Transform buttonContainer;
    public Button levelButtonPrefab;
    public Button backButton;

    [Header("Lock Appearance")]
    public Color lockedTint = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color unlockedTint = Color.white;

    [Header("Star Appearance")]
    public Color emptyStarColor = Color.black;
    public Color filledStarColor = Color.white;

    [Header("Debug Overrides")]
    public bool useDebugOverride = false;
    public bool[] debugUnlocked = new bool[] { true, false, false, false };
    public int[] debugStars = new int[] { 3, 0, 0, 0 };

    private readonly List<Button> spawnedButtons = new List<Button>();

    private void OnEnable()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleStateChanged;
            if (GameManager.Instance.CurrentState == GameState.LevelSelect)
                Refresh();
        }
    }

    private void OnDisable()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && useDebugOverride && gameObject.activeInHierarchy)
        {
            Refresh();
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.LevelSelect)
            Refresh();
    }

    public void Refresh()
    {
        ClearButtons();

        if (buttonContainer == null)
        {
            Debug.LogWarning("[LevelSelectUI] Button Container not assigned.");
            return;
        }

        if (ProgressManager.Instance == null)
        {
            Debug.LogWarning("[LevelSelectUI] ProgressManager not found.");
            return;
        }

        int total = ProgressManager.Instance.TotalLevels;

        for (int i = 0; i < total; i++)
        {
            bool unlocked = ProgressManager.Instance.IsLevelUnlocked(i);
            int stars = ProgressManager.Instance.GetLevelStars(i);

            Button btn = CreateButton(i, unlocked, stars);
            if (btn != null)
                spawnedButtons.Add(btn);
        }
    }

    private Button CreateButton(int levelIndex, bool unlocked, int stars)
    {
        // === DEBUG OVERRIDE ===
        if (useDebugOverride)
        {
            if (levelIndex < debugUnlocked.Length)
                unlocked = debugUnlocked[levelIndex];

            if (levelIndex < debugStars.Length)
                stars = debugStars[levelIndex];
        }
        // ======================

        if (levelButtonPrefab == null)
        {
            Debug.LogError("[LevelSelectUI] Level Button Prefab not assigned!");
            return null;
        }

        Button button = Instantiate(levelButtonPrefab, buttonContainer);

        // Get the direct references script
        LevelButtonUI ui = button.GetComponent<LevelButtonUI>();

        if (ui == null)
        {
            Debug.LogError("[LevelSelectUI] LevelButtonPrefab is missing the LevelButtonUI component!");
            return button;
        }

        // 1. Interactability & Background Tint
        button.interactable = unlocked;

        if (ui.backgroundImage != null)
            ui.backgroundImage.color = unlocked ? unlockedTint : lockedTint;

        // 2. Set Level Number Automatically
        ui.SetLabel((levelIndex + 1).ToString());

        // 3. Lock / Label Toggle
        if (ui.labelObject != null)
            ui.labelObject.SetActive(unlocked);   // Number visible ONLY when unlocked

        if (ui.lockIconObject != null)
            ui.lockIconObject.SetActive(!unlocked); // Lock visible ONLY when locked

        // 4. Stars
        if (ui.starImages != null)
        {
            for (int s = 0; s < ui.starImages.Length; s++)
            {
                if (ui.starImages[s] != null)
                {
                    bool earned = s < stars;
                    ui.starImages[s].color = earned ? filledStarColor : emptyStarColor;
                }
            }
        }

        // 5. Click Handler
        int capturedIndex = levelIndex;
        button.onClick.AddListener(() =>
        {
            if (transitionManager != null)
            {
                // Fade to black FIRST, then call StartLevel
                transitionManager.FadeAndExecute(() =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartLevel(capturedIndex + 1);
                    }
                });
            }
            else if (GameManager.Instance != null)
            {
                // Backup direct load if transitionManager field is empty
                GameManager.Instance.StartLevel(capturedIndex + 1);
            }
        });

        return button;
    }

    private void ClearButtons()
    {
        foreach (Button btn in spawnedButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();
    }

    public void OnBackClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}
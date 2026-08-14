using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PanelSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelData
    {
        public GameObject panel;
        public float displayDuration = 2f;
    }

    [Header("Sequence Settings")]
    [Tooltip("List of panels and how long each should be displayed.")]
    public List<PanelData> panels;

    [Header("Fade Settings")]
    [Tooltip("The panel used to fade (should have an Image component).")]
    public Image fadePanel;
    [Tooltip("Duration of the fade in/out transition between panels.")]
    public float fadeDuration = 1f;
    [Tooltip("Duration of the final fade to black when the scene ends.")]
    public float sceneEndFadeDuration = 1.5f;

    [Header("Skip Settings")]
    [Tooltip("The key used to skip the cutscene.")]
    public Key skipKey = Key.X;

    [Header("Scene Transition (Fallback)")]
    [Tooltip("Only used if GameManager is missing. Otherwise GameManager handles the transition.")]
    public string nextSceneName;

    private bool isSkipping = false;

    private void Start()
    {
        // Ensure fade panel is active and starts fully black (alpha = 1)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            SetFadeAlpha(1f);
        }

        // Hide all story panels initially
        foreach (var p in panels)
        {
            if (p.panel != null)
            {
                p.panel.SetActive(false);
            }
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (isSkipping) break;

            // Activate current panel
            if (panels[i].panel != null)
            {
                panels[i].panel.SetActive(true);
            }

            // Fade IN current panel (Black -> Transparent)
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
            if (isSkipping) break;

            // Display time for the current panel (checking for skip every frame)
            float timer = 0f;
            while (timer < panels[i].displayDuration)
            {
                if (CheckSkipInput())
                {
                    isSkipping = true;
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }
            if (isSkipping) break;

            // If it's NOT the last panel, fade out before showing the next panel
            if (i < panels.Count - 1)
            {
                // Fade OUT current panel (Transparent -> Black)
                yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
                if (isSkipping) break;

                // Deactivate current panel while screen is black
                if (panels[i].panel != null)
                {
                    panels[i].panel.SetActive(false);
                }
            }
        }

        // --- FINAL SCENE END FADE ---
        // If skipped, do a quick fade to black. If normal, use the configured duration.
        float finalFadeTime = isSkipping ? 0.5f : sceneEndFadeDuration;
        float startAlpha = fadePanel != null ? fadePanel.color.a : 0f;

        yield return StartCoroutine(Fade(startAlpha, 1f, finalFadeTime));

        // --- TRANSITION ---
        // Tell the GameManager the cutscene is done so it loads the actual level
        if (GameManager.HasInstance)
        {
            GameManager.Instance.FinishCutscene();
        }
        else
        {
            // Fallback if GameManager is somehow missing
            Debug.LogWarning("GameManager not found! Loading scene directly.");
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    private bool CheckSkipInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[skipKey].wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Allow skipping even during fades
            if (CheckSkipInput())
            {
                isSkipping = true;
                SetFadeAlpha(1f); // Instantly snap to black if skipped during a fade
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            SetFadeAlpha(newAlpha);
            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            color.a = alpha;
            fadePanel.color = color;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelData
    {
        public GameObject panel;
        public float displayDuration = 2f; // How long this panel stays visible
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

    [Header("Scene Transition")]
    [Tooltip("The name of the next scene to load after the sequence completes.")]
    public string nextSceneName;

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
            // Activate current panel
            if (panels[i].panel != null)
            {
                panels[i].panel.SetActive(true);
            }

            // Fade IN current panel (Black -> Transparent)
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            // Display time for the current panel
            yield return new WaitForSeconds(panels[i].displayDuration);

            // If it's NOT the last panel, fade out before showing the next panel
            if (i < panels.Count - 1)
            {
                // Fade OUT current panel (Transparent -> Black)
                yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

                // Deactivate current panel while screen is black
                if (panels[i].panel != null)
                {
                    panels[i].panel.SetActive(false);
                }
            }
        }

        // --- FINAL SCENE END FADE ---
        // Fade to black to cleanly close the scene
        yield return StartCoroutine(Fade(0f, 1f, sceneEndFadeDuration));

        // Load the given next scene
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is empty!");
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
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
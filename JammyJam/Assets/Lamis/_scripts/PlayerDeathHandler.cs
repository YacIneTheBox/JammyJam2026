using UnityEngine;
using System.Collections;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Tooltip("Assign the Death clip so we know its length.")]
    public AnimationClip deathClip;

    [Header("Settings")]
    [Tooltip("Exact name of the death state inside the Animator Controller.")]
    public string deathStateName = "Death";

    [Tooltip("Small pause after the animation before the losing panel shows.")]
    public float panelDelayAfterAnimation = 0.25f;

    public event System.Action OnDeathFinished;

    private Coroutine deathCoroutine;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLossTriggered += HandleLoss;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLossTriggered -= HandleLoss;
    }

    private void HandleLoss(LossReason reason)
    {
        if (deathCoroutine != null)
            StopCoroutine(deathCoroutine);

        deathCoroutine = StartCoroutine(PlayDeath());
    }

    private IEnumerator PlayDeath()
    {
        // Stop player control and physics
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
            pc.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (animator != null)
        {
            // IMPORTANT: play in real time, otherwise timeScale = 0 freezes the animation
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Force-jump straight into the Death state (ignores transitions)
            animator.Play(deathStateName, 0, 0f);
        }

        float length = deathClip != null ? deathClip.length : 1f;

        yield return new WaitForSecondsRealtime(length + panelDelayAfterAnimation);

        if (OnDeathFinished != null)
            OnDeathFinished.Invoke();

        deathCoroutine = null;
    }
}
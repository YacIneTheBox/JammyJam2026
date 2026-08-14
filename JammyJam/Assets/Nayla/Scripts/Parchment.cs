using System.Collections;
using UnityEngine;

public class Parchment : MonoBehaviour
{
    [Header("Magnet & Shrink Settings")]
    [SerializeField] private float animationDuration = 0.35f; // Time in seconds to complete animation
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AudioClip collectSound; // <-- Ajout de la variable pour le son
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;
    private bool isCollected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Prevent double triggers while the animation plays
        if (isCollected) return;

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            isCollected = true;

            // Disable Collider immediately so it can't be picked up again
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // Register collection logic
            if (LevelCollectionManager.Instance != null)
            {
                LevelCollectionManager.Instance.CollectItem();
            }

            AudioManager.Instance.PlaySFX(collectSound, soundVolume);
            // Start the magnet animation towards the player
            StartCoroutine(MagnetAndScaleToTarget(collision.transform));
        }
    }

    private IEnumerator MagnetAndScaleToTarget(Transform target)
    {
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Apply easing curve for a smoother feel
            float curveT = speedCurve.Evaluate(t);

            // Dynamically follow target position in case the player is moving
            if (target != null)
            {
                transform.position = Vector3.Lerp(startPosition, target.position, curveT);
            }

            // Smoothly scale down to 0
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, curveT);

            yield return null;
        }

        // Clean up object once animation completes
        Destroy(gameObject);
    }
}
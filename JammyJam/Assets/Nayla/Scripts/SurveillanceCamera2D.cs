using UnityEngine;
using System.Collections;

public class SurveillanceCamera2D : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private bool shouldRotate = true;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxAngle = 20f;

    [Header("Detection Settings")]
    [SerializeField] private float requiredDetectionTime = 3f;

    private Transform pivotTransform;
    private float startRotationZ;
    private Coroutine detectionCoroutine;
    private PlayerController targetPlayer;
    // Simple unique ID generator (avoids obsolete GetInstanceID)
    private static int nextSourceId = 1;
    private int sourceId;

    void Start()
    {
        sourceId = nextSourceId++;

        pivotTransform = transform.parent;

        if (pivotTransform != null)
            startRotationZ = pivotTransform.eulerAngles.z;
        else
            startRotationZ = transform.eulerAngles.z;
    }

    void Update()
    {
        // 1. Logique de rotation (inchangée)
        if (shouldRotate)
        {
            float angle = Mathf.Sin(Time.time * (rotationSpeed * Mathf.Deg2Rad)) * maxAngle;

            if (pivotTransform != null)
                pivotTransform.rotation = Quaternion.Euler(0f, 0f, startRotationZ + angle);
            else
                transform.rotation = Quaternion.Euler(0f, 0f, startRotationZ + angle);
        }

        // 2. Logique de détection (inchangée, mais alimente le compteur)
        if (targetPlayer != null)
        {
            if (!targetPlayer.IsPerfectlySnapped)
            {
                if (detectionCoroutine == null)
                {
                    Debug.Log("[" + gameObject.name + "] Anomalie détectée - Démarrage du scan...");
                    detectionCoroutine = StartCoroutine(DetectionCountdown());
                }
            }
            else
            {
                if (detectionCoroutine != null)
                {
                    Debug.Log("[" + gameObject.name + "] Cible fondue dans la masse. Annulation de l'alerte.");
                    ResetDetection();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Joueur")
            targetPlayer = collision.GetComponent<PlayerController>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Joueur")
        {
            targetPlayer = null;
            ResetDetection();
        }
    }

    private void ResetDetection()
    {
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
            detectionCoroutine = null;
        }

        // Laisse le compteur redescendre quand le scan est annulé
        if (SuspicionManager.Instance != null)
            SuspicionManager.Instance.RemoveSource(sourceId);
    }

    private IEnumerator DetectionCountdown()
    {
        float timer = requiredDetectionTime;

        while (timer > 0f)
        {
            int displaySeconds = Mathf.CeilToInt(timer);
            Debug.Log("[" + gameObject.name + "] Alerte dans : " + displaySeconds);

            // Décompte fluide de cette seconde pour remplir le compteur en continu
            float step = 1f;
            while (step > 0f && timer > 0f)
            {
                float delta = Mathf.Min(Time.deltaTime, step);
                step -= delta;
                timer -= delta;

                ReportProgress(timer);

                yield return null;
            }
        }

        Debug.Log("[" + gameObject.name + "] GAME OVER !");

        // Remplit le compteur à fond, puis signale la perte au GameManager
        if (SuspicionManager.Instance != null)
            SuspicionManager.Instance.ReportSuspicion(sourceId, 1f);

        if (GameManager.Instance != null)
            GameManager.Instance.TriggerLoss(LossReason.CameraCaught);

        detectionCoroutine = null;
    }

    private void ReportProgress(float timer)
    {
        if (SuspicionManager.Instance == null)
            return;

        float progress = 1f - (timer / Mathf.Max(0.1f, requiredDetectionTime));
        SuspicionManager.Instance.ReportSuspicion(sourceId, progress);
    }

    private void OnDisable()
    {
        if (SuspicionManager.Instance != null)
            SuspicionManager.Instance.RemoveSource(sourceId);
    }
}
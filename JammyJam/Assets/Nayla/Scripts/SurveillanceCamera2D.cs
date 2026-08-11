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
    private PlayerController targetPlayer; // Garde en mémoire le joueur ciblé

    void Start()
    {
        pivotTransform = transform.parent;
        if (pivotTransform != null)
        {
            startRotationZ = pivotTransform.eulerAngles.z;
        }
        else
        {
            startRotationZ = transform.eulerAngles.z;
        }
    }

    void Update()
    {
        // 1. Logique de rotation (inchangée)
        if (shouldRotate)
        {
            float angle = Mathf.Sin(Time.time * (rotationSpeed * Mathf.Deg2Rad)) * maxAngle;
            
            if (pivotTransform != null)
            {
                pivotTransform.rotation = Quaternion.Euler(0f, 0f, startRotationZ + angle);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, startRotationZ + angle);
            }
        }

        // 2. Logique de détection intelligente
        if (targetPlayer != null)
        {
            // Si le joueur n'est PAS sur son point d'ancrage (c'est une anomalie)
            if (!targetPlayer.IsPerfectlySnapped)
            {
                // On démarre le compte à rebours si ce n'est pas déjà fait
                if (detectionCoroutine == null)
                {
                    Debug.Log("[" + gameObject.name + "] Anomalie détectée - Démarrage du scan...");
                    detectionCoroutine = StartCoroutine(DetectionCountdown());
                }
            }
            else 
            {
                // Le joueur est retourné sur son point de snap, le système le considère comme normal
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
        {
            // On enregistre simplement que le joueur est dans le champ de vision
            targetPlayer = collision.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Joueur")
        {
            // Le joueur sort du champ visuel, on annule tout
            targetPlayer = null;
            ResetDetection();
            Debug.Log("[" + gameObject.name + "] Cible perdue (hors de vue) !");
        }
    }

    private void ResetDetection()
    {
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
            detectionCoroutine = null;
        }
    }

    private IEnumerator DetectionCountdown()
    {
        float timer = requiredDetectionTime;

        while (timer > 0f)
        {
            int displaySeconds = Mathf.CeilToInt(timer);
            Debug.Log("[" + gameObject.name + "] Alerte dans : " + displaySeconds);

            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        Debug.Log("[" + gameObject.name + "] GAME OVER ! Tu as été éliminé par le système.");
        detectionCoroutine = null;
    }
}
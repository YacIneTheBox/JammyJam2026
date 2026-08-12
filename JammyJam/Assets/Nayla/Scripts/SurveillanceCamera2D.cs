using UnityEngine;
using UnityEngine.SceneManagement; // L'ajout de ton collègue indispensable pour recharger le niveau
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
    
    // On utilise directement le PlayerController plutôt qu'un simple GameObject 
    // pour avoir accès à notre variable IsPerfectlySnapped
    private PlayerController targetPlayer; 

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
        // 1. Logique de rotation
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

        // 2. Notre logique intelligente de détection (Camouflage sur le tapis)
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
        {
            // On enregistre le joueur sans déclencher la mort instantanément
            targetPlayer = collision.GetComponent<PlayerController>();
        }
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

        Debug.Log("[" + gameObject.name + "] GAME OVER !");
        
        // 3. L'ajout de ton collègue intégré proprement
        if (targetPlayer != null)
        {
            Destroy(targetPlayer.gameObject);
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        detectionCoroutine = null;
    }
}
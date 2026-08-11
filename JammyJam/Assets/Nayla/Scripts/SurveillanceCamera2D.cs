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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Joueur")
        {
            Debug.Log("[" + gameObject.name + "] detected by cameras - starting countdown...");
            if (detectionCoroutine == null)
            {
                detectionCoroutine = StartCoroutine(DetectionCountdown());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Joueur")
        {
            Debug.Log("[" + gameObject.name + "] escaped camera view!");
            if (detectionCoroutine != null)
            {
                StopCoroutine(detectionCoroutine);
                detectionCoroutine = null;
            }
        }
    }

    private IEnumerator DetectionCountdown()
    {
        float timer = requiredDetectionTime;

        while (timer > 0f)
        {
            // Log each second clearly (e.g., 3, then 2, then 1)
            int displaySeconds = Mathf.CeilToInt(timer);
            Debug.Log("[" + gameObject.name + "] Countdown: " + displaySeconds);

            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        Debug.Log("[" + gameObject.name + "] YOU'RE DEAD!");
        detectionCoroutine = null;
    }
}
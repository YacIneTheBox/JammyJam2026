using UnityEngine;

public class CameraBoundsChecker : MonoBehaviour
{
    private Camera mainCam;
    
    [Tooltip("Temps d'attente (en secondes) avant d'activer la vérification, pour laisser la caméra s'initialiser.")]
    public float gracePeriod = 1.0f; 
    private float timer = 0f;

    void Start()
    {
        mainCam = Camera.main;
        timer = 0f; // On réinitialise le timer au démarrage
    }

    void Update()
    {
        // On ne vérifie que si le jeu est en cours
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (mainCam == null) return;

        // On fait tourner le chronomètre
        timer += Time.deltaTime;

        // Tant que la période de grâce n'est pas passée, on ignore la suite
        if (timer < gracePeriod) return;

        // Une fois le temps écoulé, on reprend la vérification normale
        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            GameManager.Instance.TriggerLoss(LossReason.OutOfCameraSight);
        }
    }
}
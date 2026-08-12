using UnityEngine;

public class Parchment : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Triggered by: " + collision.gameObject.name);

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            // Use the new global manager name here:
            if (LevelCollectionManager.Instance != null)
            {
                LevelCollectionManager.Instance.CollectItem();
            }

            Destroy(gameObject);
        }
    }
}
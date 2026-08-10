using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BeltZone : MonoBehaviour
{
    void Start()
    {
        // S'assure que le collider est bien en mode Trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetOnBelt(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetOnBelt(false);
            }
        }
    }
}
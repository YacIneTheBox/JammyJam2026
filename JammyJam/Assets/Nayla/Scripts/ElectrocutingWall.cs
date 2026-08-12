using UnityEngine;
using UnityEngine.SceneManagement;

public class ElectrocutingWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player touched the wall
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
        {
            Debug.Log("[ElectrocutingWall] Player got electrocuted!");
            KillPlayer(collision.gameObject);
        }
    }

    private void KillPlayer(GameObject player)
    {
        // Option 1: Destroy the player object
        Destroy(player);

        // Option 2: Reload the current scene to restart the level
        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
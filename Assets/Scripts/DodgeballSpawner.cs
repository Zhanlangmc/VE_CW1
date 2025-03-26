using System.Collections;
using UnityEngine;
using Ubiq.Spawning;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

public class DodgeballSpawner : MonoBehaviour
{
    public GameObject dodgeballPrefab;
    public float spawnInterval = 10f;
    public float destoryInterval = 10f;
    private NetworkSpawnManager spawnManager;

    private void Start()
    {
        // Finding Ubiq Network Spawn Manager
        spawnManager = NetworkSpawnManager.Find(this);
        if (spawnManager == null)
        {
            Debug.LogError("NetworkSpawnManager not found in the scene!");
            return;
        }

        StartCoroutine(SpawnDodgeballs());
    }

    private IEnumerator SpawnDodgeballs()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnDodgeball();
        }
    }

    private void SpawnDodgeball()
    {
        if (spawnManager == null || dodgeballPrefab == null)
        {
            Debug.LogError("Spawn Manager or Dodgeball Prefab is missing!");
            return;
        }

        // Spawn Dodgeball and sync it to all players
        GameObject dodgeball = spawnManager.SpawnWithPeerScope(dodgeballPrefab);
        if (dodgeball != null)
        {
            dodgeball.transform.position = transform.position; // Generated location
            StartCoroutine(DestroyDodgeballAfterTime(dodgeball, destoryInterval));
        }
    }

    private IEnumerator DestroyDodgeballAfterTime(GameObject dodgeball, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dodgeball != null)
        {
            spawnManager.Despawn(dodgeball); // Make sure all clients destroy Dodgeball synchronously
        }
    }
}
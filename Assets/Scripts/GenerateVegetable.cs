using System.Collections;
using UnityEngine;
using Ubiq.Spawning;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

public class GenerateVegetable : MonoBehaviour
{
    public float spawnInterval = 10f;    // Spawns a Dodgeball every 10 seconds
    public float destoryInterval = 10f;  // Destroys a Dodgeball every 10 seconds
    private NetworkSpawnManager spawnManager; // Ubiq Build Manager

    public PrefabCatalogue catalogue;

    private void SpawnDodgeball()
    {
        // Check if the build manager and directory exist
        if (spawnManager == null || catalogue == null || catalogue.prefabs.Count == 0)
        {
            Debug.LogError("Spawn Manager or Prefab Catalogue is missing or empty!");
            return;
        }

        // Randomly select a prefab
        int randomIndex = Random.Range(0, catalogue.prefabs.Count);
        GameObject prefabToSpawn = catalogue.prefabs[randomIndex];

        // Spawn Dodgeball and sync it to all players
        GameObject dodgeball = spawnManager.SpawnWithPeerScope(prefabToSpawn);
        if (dodgeball != null)
        {
            dodgeball.transform.position = transform.position; // Set the spawn location
            StartCoroutine(DestroyDodgeballAfterTime(dodgeball, destoryInterval));
        }
    }
    private IEnumerator SpawnDodgeballs()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnDodgeball();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnManager = NetworkSpawnManager.Find(this);
        if (spawnManager == null)
        {
            Debug.LogError("NetworkSpawnManager not found in the scene!");
            return;
        }
        if(catalogue == null)
        {
            Debug.LogError("Prefab Catalogue not assigned!");
            return;
        }
        StartCoroutine(SpawnDodgeballs());
    
    }

    // Update is called once per frame
    private IEnumerator DestroyDodgeballAfterTime(GameObject dodgeball, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dodgeball != null)
        {
            spawnManager.Despawn(dodgeball); // Make sure all clients destroy Dodgeball synchronously
        }
    }
}

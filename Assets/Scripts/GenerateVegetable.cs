using System.Collections;
using UnityEngine;
using Ubiq.Spawning;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

public class GenerateVegetable : MonoBehaviour
{
    public float spawnInterval = 10f;    // 每 10 秒生成一个 Dodgeball
    public float destoryInterval = 10f;  // 每 10 秒销毁一个 Dodgeball
    private NetworkSpawnManager spawnManager; // Ubiq 生成管理器

    public PrefabCatalogue catalogue;

    private void SpawnDodgeball()
    {
        // 检查生成管理器和目录是否存在
        if (spawnManager == null || catalogue == null || catalogue.prefabs.Count == 0)
        {
            Debug.LogError("Spawn Manager or Prefab Catalogue is missing or empty!");
            return;
        }

        // 随机选择一个预制体
        int randomIndex = Random.Range(0, catalogue.prefabs.Count);
        GameObject prefabToSpawn = catalogue.prefabs[randomIndex];

        // 生成 Dodgeball，并同步到所有玩家
        GameObject dodgeball = spawnManager.SpawnWithPeerScope(prefabToSpawn);
        if (dodgeball != null)
        {
            dodgeball.transform.position = transform.position; // 设置生成位置
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
            spawnManager.Despawn(dodgeball); // 确保所有客户端同步销毁 Dodgeball
        }
    }
}

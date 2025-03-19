using System.Collections;
using UnityEngine;
using Ubiq.Spawning;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

public class DodgeballSpawner : MonoBehaviour
{
    public GameObject dodgeballPrefab; // 预制体
    public float spawnInterval = 10f;  // 每 10 秒生成一个 Dodgeball
    public float destoryInterval = 10f;  // 每 10 秒生成一个 Dodgeball
    private NetworkSpawnManager spawnManager; // Ubiq 生成管理器

    private void Start()
    {
        // 查找 Ubiq 网络生成管理器
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

        // 生成 Dodgeball，并同步到所有玩家
        GameObject dodgeball = spawnManager.SpawnWithPeerScope(dodgeballPrefab);
        if (dodgeball != null)
        {
            dodgeball.transform.position = transform.position; // 生成位置
            StartCoroutine(DestroyDodgeballAfterTime(dodgeball, destoryInterval));
        }
    }

    private IEnumerator DestroyDodgeballAfterTime(GameObject dodgeball, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dodgeball != null)
        {
            spawnManager.Despawn(dodgeball); // 确保所有客户端同步销毁 Dodgeball
        }
    }
}
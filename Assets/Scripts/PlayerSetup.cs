using UnityEngine;
using Unity.XR.CoreUtils;

public class PlayerSetup : MonoBehaviour
{

    public GameObject hudPrefab; // 在 Inspector 中拖入 LocalHUD 预制体
    private GameObject hudInstance;
    private LocalHUDManager hudManager;
    public static Score LocalPlayerScore;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 假设玩家 Avatar 下有 Score 脚本
        Score score = GetComponent<Score>();

        if (score != null)
        {
            LocalPlayerScore = score;
        }
        // 查找玩家的主摄像机（可以根据你的场景结构做调整）
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // 尝试自动查找主相机
            if (playerCamera == null)
            {
                Debug.LogError("PlayerSetup: Missing playerCamera! Ensure the scene has a Main Camera.");
                return;
            }
        }
        if (hudPrefab == null)
        {
            Debug.LogError("PlayerSetup: Missing hudPrefab! Please assign a HUD prefab in the inspector.");
            return;
        }
        if (playerCamera != null && hudPrefab != null)
        {
            // 在摄像机下实例化 HUD，使其随玩家视角显示
            hudInstance = Instantiate(hudPrefab, playerCamera.transform);
            hudManager = hudInstance.GetComponent<LocalHUDManager>();
            if (hudManager != null && score != null)
            {
                hudManager.SetPlayer(score);
            }
        }
        else
        {
            Debug.LogError("PlayerSetup: Missing playerCamera or hudPrefab!");
        }
    }

}

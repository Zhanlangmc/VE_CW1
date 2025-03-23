using UnityEngine;
using Unity.XR.CoreUtils;

public class PlayerSetup : MonoBehaviour
{

    public GameObject hudPrefab; // �� Inspector ������ LocalHUD Ԥ����
    private GameObject hudInstance;
    private LocalHUDManager hudManager;
    public static Score LocalPlayerScore;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ������� Avatar ���� Score �ű�
        Score score = GetComponent<Score>();

        if (score != null)
        {
            LocalPlayerScore = score;
        }
        // ������ҵ�������������Ը�����ĳ����ṹ��������
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // �����Զ����������
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
            // ���������ʵ���� HUD��ʹ��������ӽ���ʾ
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

using Ubiq.Avatars;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public GameObject hudPrefab;
    private GameObject hudInstance;
    private LocalHUDManager hudManager;
    public static Score LocalPlayerScore;

    void Start()
    {
        Score score = GetComponent<Score>();
        if (score != null)
        {
            LocalPlayerScore = score;
        }

        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("PlayerSetup: No Camera found!");
                return;
            }
        }

        if (hudPrefab == null)
        {
            Debug.LogError("PlayerSetup: HUD prefab is missing.");
            return;
        }

        // Properly instantiating and placing the HUD
        hudInstance = Instantiate(hudPrefab);
        hudInstance.name = "LocalHUD (Runtime)";
        Debug.Log("[HUD] Instantiated");
        // Make sure to activate
        hudInstance.SetActive(true);
        
        // Set event Camera
        Canvas canvas = hudInstance.GetComponentInChildren<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = playerCamera;
        }

        // Make sure to activate
        hudInstance.SetActive(true);
        Debug.Log("[HUD] Activated");

        // Put it in front of the camera
        hudInstance.transform.SetParent(playerCamera.transform);
        hudInstance.transform.localPosition = new Vector3(0f, 0.0f, 0.5f);
        hudInstance.transform.localRotation = Quaternion.identity;
        hudInstance.transform.localScale = Vector3.one * 0.0002f;
        hudInstance.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward, Vector3.up);
        
        hudInstance.SetActive(true);

        // Initializing the HUD
        hudManager = hudInstance.GetComponent<LocalHUDManager>();
        if (hudManager != null && score != null)
        {
            hudManager.SetPlayer(score);
        }
        else
        {
            Debug.LogWarning("HUDManager not found or Score missing");
        }
        hudInstance.SetActive(true);
        // Set the local Avatar to AvatarSelf Layer
        SetLocalAvatarLayerToAvatarSelf();
    }
    
    private void LateUpdate()
    {
        if (hudInstance != null && !hudInstance.activeSelf)
        {
            Debug.LogWarning("[HUD] Detected disabled — re-enabling.");
            hudInstance.SetActive(true);
        }
    }
    
    private void SetLocalAvatarLayerToAvatarSelf()
    {
        AvatarManager manager = FindObjectOfType<AvatarManager>();
        if (manager == null)
        {
            Debug.LogWarning("No AvatarManager found!");
            return;
        }

        foreach (Transform child in manager.transform)
        {
            if (child.name.StartsWith("My Avatar"))
            {
                Debug.Log("Found local avatar: " + child.name);
                SetLayerRecursively(child.gameObject, LayerMask.NameToLayer("AvatarSelf"));
                break;
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}
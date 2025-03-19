using System.Collections;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ChangeColor : MonoBehaviour
{
    public GameObject prefab; // Avatar Prefab (optional)
    public Material[] colorMaterials; // Store 4 color materials
    
    private RoomClient roomClient;
    private AvatarManager avatarManager;

    private void Start()
    {
        // Find the Network Avatar Manager
        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();
    }

    // Function to change avatar color via UI button
    public void SetColor(int colorIndex)
    {
        if (!avatarManager || colorMaterials.Length == 0) return;

        // Find the current avatar in the scene
        var avatar = avatarManager.FindAvatar(roomClient.Me);
        if (avatar)
        {
            Renderer avatarRenderer = avatar.GetComponentInChildren<Renderer>();
            if (avatarRenderer && colorIndex >= 0 && colorIndex < colorMaterials.Length)
            {
                avatarRenderer.material = colorMaterials[colorIndex]; // Set new material
                Debug.Log("Changed Avatar color to: " + colorMaterials[colorIndex].name);
            }
        }
    }
}

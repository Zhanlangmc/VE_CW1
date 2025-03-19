using System.Collections;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = UnityEngine.Random;

/// <summary>
/// This class listens to the select event of an XRI interactable, then sets the
/// avatar to the sample Ubiq avatar and gives it a random costume. It also supports
/// UI button interaction for changing the avatar costume.
/// </summary>
public class ChangeCostume : MonoBehaviour
{
    public GameObject prefab; 
    public Button uiButton; // UI button to change the avatar costume

    private XRSimpleInteractable interactable;
    private RoomClient roomClient;
    private AvatarManager avatarManager;

    private void Start()
    {
        // Connect the XRI interaction event
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable)
        {
            interactable.selectEntered.AddListener(Interactable_SelectEntered);
        }

        //  RoomClient, AvatarManager
        var networkScene = NetworkScene.Find(this);
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        avatarManager = networkScene.GetComponentInChildren<AvatarManager>();

        // Connect the UI button if assigned
        if (uiButton != null)
        {
            uiButton.onClick.AddListener(ChangeAvatarCostume);
        }
    }

    private void OnDestroy()
    {
        // Ensure the XRI interaction event is removed
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
        }

        //Ensure the UI button event is removed
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(ChangeAvatarCostume);
        }
    }

    /// <summary>
    ///Triggered by XR interaction (select event)
    /// </summary>
    private void Interactable_SelectEntered(SelectEnterEventArgs arg0)
    {
        ChangeAvatarCostume();
    }

    /// <summary>
    /// Switches to the next avatar prefab in the list
    /// </summary>
    public void ChangeAvatarCostume()
    {
        if (!avatarManager) return;

        // Cycle to the next avatar prefab
        avatarManager.avatarPrefab = prefab;

        // Sets a random costume for the avatar after switching to a new prefab
        StartCoroutine(SetRandomCostume());
    }

    private IEnumerator SetRandomCostume()
    {
        while (true)
        {
            if (!avatarManager)
            {
                yield break;
            }

            var avatar = avatarManager.FindAvatar(roomClient.Me);
            if (avatar)
            {
                var textured = avatar.GetComponentInChildren<TexturedAvatar>();
                if (textured)
                {
                    var randomCostume = textured.Textures.Get(
                        Random.Range(0, textured.Textures.Count));
                    textured.SetTexture(randomCostume);
                    yield break;
                }
            }

            yield return null;
            yield return null;
        }
    }
}

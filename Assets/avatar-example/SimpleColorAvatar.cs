using System;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;

/// <summary>
/// This class changes all base color values on the avatar to a specified color.
/// It also handles syncing the color over the network using properties.
/// </summary>
public class SimpleColorAvatar : MonoBehaviour
{
    private RoomClient roomClient;
    private Avatar avatar; 
    private List<Renderer> renderers = new();
    
    // Keep a record of the last color as a string. This lets us quickly check
    // whether anything has changed when we receive the OnPeerUpdated event.
    private string lastColor;
    
    private void Start()
    {
        avatar = GetComponentInParent<Avatar>();
        
        var networkScene = NetworkScene.Find(this); 
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        
        // Connect up a listener for whenever any peer's properties are changed. 
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
    }

    private void OnDestroy()
    {
        // Cleanup the event for new properties so it does not get called after
        // we have been destroyed.
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }

    public void SetColor (Color color)
    {
        if (!avatar.IsLocal)
        {
            // If we are not local, we are a remote copy, and we do not control
            // this avatar. Only the peer who controls this avatar should be
            // able to change it.
            return;
        }
        
        // Set the color as a property for our peer. Remote copies of this
        // avatar will be informed of the change as an event. 
        var serializedColor = JsonUtility.ToJson(color);
        ProcessColor(serializedColor);
        roomClient.Me["simple-color-avatar"] = serializedColor;
    }
    
    private void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        
        ProcessColor(peer["simple-color-avatar"]);
    }
    
    private void ProcessColor(string serializedColor)
    {
        if (String.IsNullOrEmpty(serializedColor) || serializedColor == lastColor)
        {
            return;
        }

        // Deserialize from a string into a color. 
        var color = JsonUtility.FromJson<Color>(serializedColor);
        
        // Get all renderers and set them to the specified color.
        // Re-use a list for the renderers. New lists need to be cleaned up
        // by the C# runtime, which is performance intensive and can cause
        // gameplay hiccups, so we re-use where we can. 
        renderers.Clear();
        avatar.GetComponentsInChildren(includeInactive:true,renderers);
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material.SetColor("_BaseColor",color);
        }
            
        lastColor = serializedColor;
    }
    
}

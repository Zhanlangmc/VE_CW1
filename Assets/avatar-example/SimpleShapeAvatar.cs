using System;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;

/// <summary>
/// This class changes enables various different gameobjects meant to represent
/// the local user's body and movements. It also handles syncing which
/// gameobject is currently active over the network.
/// </summary>
public class SimpleShapeAvatar : MonoBehaviour
{
    public GameObject[] shapes;
    public GameObject currentShape;
    
    private RoomClient roomClient;
    private Avatar avatar; 
    
    // Keep a record of the last shape as a string. This lets us quickly check
    // whether anything has changed when we receive the OnPeerUpdated event.
    private string lastShape;
    
    [Serializable]
    private struct SerializableShape
    {
        public int index;
    }
    
    private void Start()
    {
        avatar = GetComponentInParent<Avatar>();
        
        var networkScene = NetworkScene.Find(this); 
        roomClient = networkScene.GetComponentInChildren<RoomClient>();
        
        // Connect up a listener for whenever any peer's properties are changed. 
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
        
        // Set an initial shape if we have none.
        if (!currentShape && shapes.Length > 0)
        {
            SetShape(shapes[0]);
        }
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

    public void SetShape (GameObject shape)
    {
        if (!avatar.IsLocal)
        {
            // If we are not local, we are a remote copy, and we do not control
            // this avatar. Only the peer who controls this avatar should be
            // able to change it.
            return;
        }
        
        var index = Array.IndexOf(shapes,shape);
        if (index < 0)
        {
            Debug.LogWarning("Unrecognized shape. This class can only set " +
                             "shapes from a pre-determined list.");
            return;
        }
        
        // Set the shape as a property for our peer. Remote copies of this
        // avatar will be informed of the change as an event. 
        var serializedShape = JsonUtility.ToJson(new SerializableShape
        {
            index = index
        });
        ProcessShape(serializedShape);
        roomClient.Me["simple-shape-avatar"] = serializedShape;
    }
    
    private void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        
        ProcessShape(peer["simple-shape-avatar"]);
    }
    
    private void ProcessShape(string serializedShape)
    {
        if (String.IsNullOrEmpty(serializedShape) || serializedShape == lastShape)
        {
            return;
        }

        // Deserialize from a string into a color. 
        var index = JsonUtility.FromJson<SerializableShape>(serializedShape).index;
        if (index < 0 || index >= shapes.Length)
        {
            Debug.LogWarning("Unrecognized shape received as property.");
            return;
        }
        
        // Deactivate all other shapes and activate the current one.
        for (int i = 0; i < shapes.Length; i++)
        {
            shapes[i].SetActive(i == index);
        }
            
        lastShape = serializedShape;
        currentShape = shapes[index];
    }
    
}

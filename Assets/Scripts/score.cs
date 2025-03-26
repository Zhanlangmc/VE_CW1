using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using System;

public class Score : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; } = NetworkId.Unique();
    private NetworkContext context;
    public int score = 0;

    void Start()
    {
        context = NetworkScene.Register(this);
        // Register the Score to the global ScoreManager for easy access during the game
        ScoreManager.Instance.RegisterScore(this);
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // This function handles incoming network messages
        Debug.Log("Score received network message.");

    }

    public void AddScore(int amount)
    {
        // Increase the player's score and log the updated score
        score += amount;
        Debug.Log("New Score: " + score);

    }

}



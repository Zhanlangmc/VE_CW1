using UnityEngine;
using System.Collections.Generic;
using Ubiq.Messaging;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private Dictionary<NetworkId, Score> scoreDictionary = new Dictionary<NetworkId, Score>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call registration in Score script Start()
    public void RegisterScore(Score score)
    {
        if (!scoreDictionary.ContainsKey(score.NetworkId))
        {
            scoreDictionary.Add(score.NetworkId, score);
            Debug.Log($"Registered Score for {score.gameObject.name} with NetworkId: {score.NetworkId}");
        }
    }

    // Provide a method to return all registered Scores
    public IEnumerable<Score> GetAllScores()
    {
        return scoreDictionary.Values;
    }

    // Find the corresponding Score script based on NetworkId
    public Score GetScoreByNetworkId(NetworkId id)
    {
        Score s;
        scoreDictionary.TryGetValue(id, out s);
        return s;
    }
}


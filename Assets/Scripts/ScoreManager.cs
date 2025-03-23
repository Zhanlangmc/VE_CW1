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

    // 在 Score 脚本 Start() 中调用注册
    public void RegisterScore(Score score)
    {
        if (!scoreDictionary.ContainsKey(score.NetworkId))
        {
            scoreDictionary.Add(score.NetworkId, score);
            Debug.Log($"Registered Score for {score.gameObject.name} with NetworkId: {score.NetworkId}");
        }
    }

    // 提供一个方法来返回所有注册的 Score（供 GameManager 使用）
    public IEnumerable<Score> GetAllScores()
    {
        return scoreDictionary.Values;
    }

    // 根据 NetworkId 查找对应的 Score 脚本
    public Score GetScoreByNetworkId(NetworkId id)
    {
        Score s;
        scoreDictionary.TryGetValue(id, out s);
        return s;
    }
}


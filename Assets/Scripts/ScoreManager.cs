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

    // �� Score �ű� Start() �е���ע��
    public void RegisterScore(Score score)
    {
        if (!scoreDictionary.ContainsKey(score.NetworkId))
        {
            scoreDictionary.Add(score.NetworkId, score);
            Debug.Log($"Registered Score for {score.gameObject.name} with NetworkId: {score.NetworkId}");
        }
    }

    // �ṩһ����������������ע��� Score���� GameManager ʹ�ã�
    public IEnumerable<Score> GetAllScores()
    {
        return scoreDictionary.Values;
    }

    // ���� NetworkId ���Ҷ�Ӧ�� Score �ű�
    public Score GetScoreByNetworkId(NetworkId id)
    {
        Score s;
        scoreDictionary.TryGetValue(id, out s);
        return s;
    }
}


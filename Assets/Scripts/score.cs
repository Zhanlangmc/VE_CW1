//using UnityEngine;
//using Ubiq.Messaging;
//using Ubiq.Spawning;
//using Ubiq.Geometry;

//public class Score : MonoBehaviour, INetworkSpawnable
//{
//    // ���� Ubiq Ҫ��NetworkId ����Ψһ��ʶ�������
//    public NetworkId NetworkId { get; set; } = NetworkId.Unique();

//    private NetworkContext context;
//    public int score = 0;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        // ע�ᵱǰ��������糡������÷��ͺͽ�����Ϣ��������
//        context = NetworkScene.Register(this);
//    }

//    private struct Message
//    {
//        public int score;
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // �����ײ�����Ƿ���Ϊ "Ball"
//        if (collision.collider.CompareTag("Ball"))
//        {
//            // �ۼƷ���
//            score++;
//            Debug.Log("Score: " + score);

//            // �������壨����������������練������Ч��
//            Destroy(collision.gameObject);

//            // ������Ϣ���󣬽���ǰ������ Avatar �ľֲ� Pose����������糡�����ڵ㣩�������
//            Message msg;
//            msg.score = score;
//            context.SendJson(msg);
//        }
//    }
//    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
//    {
//        var msg = message.FromJson<Message>();
//        // �����յ��ľֲ� Pose ת��Ϊ��������
//        score = msg.score;
//        Debug.Log("Network update - Score: " + score);
//    }

//}
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
        // ע�ᵽȫ�� ScoreManager����������Ϸ����ʱ����
        ScoreManager.Instance.RegisterScore(this);
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // ����������Ϣ����������Ը�����Ҫ����������Ϣ
        Debug.Log("Score received network message.");
        // �����Ҫͬ����������������������ӽ�������

    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("New Score: " + score);
        Debug.Log($"��� {gameObject.name}��ID: {NetworkId}���ķ�������Ϊ��{score}");

    }

}



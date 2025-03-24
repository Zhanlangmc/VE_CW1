//using UnityEngine;
//using Ubiq.Messaging;
//using Ubiq.Spawning;
//using Ubiq.Geometry;

//public class Score : MonoBehaviour, INetworkSpawnable
//{
//    // 根据 Ubiq 要求，NetworkId 用于唯一标识网络对象
//    public NetworkId NetworkId { get; set; } = NetworkId.Unique();

//    private NetworkContext context;
//    public int score = 0;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        // 注册当前组件到网络场景，获得发送和接收消息的上下文
//        context = NetworkScene.Register(this);
//    }

//    private struct Message
//    {
//        public int score;
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // 检测碰撞对象是否标记为 "Ball"
//        if (collision.collider.CompareTag("Ball"))
//        {
//            // 累计分数
//            score++;
//            Debug.Log("Score: " + score);

//            // 销毁球体（或根据需求处理，例如反弹或特效）
//            Destroy(collision.gameObject);

//            // 构造消息对象，将当前分数和 Avatar 的局部 Pose（相对于网络场景根节点）打包发送
//            Message msg;
//            msg.score = score;
//            context.SendJson(msg);
//        }
//    }
//    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
//    {
//        var msg = message.FromJson<Message>();
//        // 将接收到的局部 Pose 转换为世界坐标
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
        // 注册到全局 ScoreManager，方便在游戏结束时汇总
        ScoreManager.Instance.RegisterScore(this);
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // 例如简单输出消息，后续你可以根据需要解析网络消息
        Debug.Log("Score received network message.");
        // 如果需要同步分数，你可以在这里添加解析代码

    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("New Score: " + score);
        Debug.Log($"玩家 {gameObject.name}（ID: {NetworkId}）的分数更新为：{score}");

    }

}



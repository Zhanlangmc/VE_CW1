using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using System;

public class Ball : MonoBehaviour, INetworkSpawnable
{
    // 实现 INetworkSpawnable 接口，生成一个唯一的网络标识
    public NetworkId NetworkId { get; set; } = NetworkId.Unique();

    // 用于记录球的发射者（投掷者）的标识，需要在投掷时设置
    public NetworkId ownerId;

    // 你可以在此处添加其他投掷物相关的变量，例如投掷力度、销毁时间等

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"Ball ownerId: {ownerId}");
        // Debug.Log("Ball collided with: " + collision.collider.name);
        // 检查碰撞对象是否为玩家，假设玩家 Avatar 的 Tag 设置为 "Player"
        if (collision.collider.CompareTag("Player"))
        {
            // 尝试从碰撞对象或其父物体获取 Score 脚本
            Score hitScore = collision.collider.GetComponentInParent<Score>();
            if (hitScore != null)
            {
                // Debug.Log($"Hit player's Score NetworkId: {hitScore.NetworkId}");

                Debug.Log($"Ball ownerId: {ownerId}");
                Debug.Log($"Hit player's Score.NetworkId: {hitScore.NetworkId}");

                // 如果命中的玩家不是投掷者（防止自杀计分），则进行加分处理
                if (hitScore.NetworkId != ownerId)
                {
                    // 通过全局 ScoreManager 找到投掷者对应的 Score 脚本
                    Score shooterScore = ScoreManager.Instance.GetScoreByNetworkId(ownerId);
                    if (shooterScore == null)
                    {
                        Debug.LogError($"No shooter Score found for ownerId: {ownerId}");
                    }
                    if (shooterScore != null)
                    {
                        Debug.Log($"Shooter found: {shooterScore.gameObject.name}");
                        shooterScore.AddScore(1);
                        Debug.Log($"Player {shooterScore.gameObject.name} scored! New score: {shooterScore.score}");
                    }
                }
            }


            // 碰撞后销毁球体
            Destroy(gameObject);
        }
        // 如果需要处理其他碰撞（比如碰到墙壁），可以在此添加额外逻辑
    }

    private struct BallMessage
    {
        public NetworkId ownerId;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<BallMessage>();
        // 如果收到的 ownerId 是默认值，则不要覆盖现有的有效值
        if (msg.ownerId != default(NetworkId))
        {
            ownerId = msg.ownerId;
            Debug.Log($"Received Ball ownerId: {ownerId}");
        }
    }


}

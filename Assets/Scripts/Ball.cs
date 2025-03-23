using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Messaging;
using System;

public class Ball : MonoBehaviour, INetworkSpawnable
{
    // ʵ�� INetworkSpawnable �ӿڣ�����һ��Ψһ�������ʶ
    public NetworkId NetworkId { get; set; } = NetworkId.Unique();

    // ���ڼ�¼��ķ����ߣ�Ͷ���ߣ��ı�ʶ����Ҫ��Ͷ��ʱ����
    public NetworkId ownerId;

    // ������ڴ˴��������Ͷ������صı���������Ͷ�����ȡ�����ʱ���

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"Ball ownerId: {ownerId}");
        // Debug.Log("Ball collided with: " + collision.collider.name);
        // �����ײ�����Ƿ�Ϊ��ң�������� Avatar �� Tag ����Ϊ "Player"
        if (collision.collider.CompareTag("Player"))
        {
            // ���Դ���ײ������丸�����ȡ Score �ű�
            Score hitScore = collision.collider.GetComponentInParent<Score>();
            if (hitScore != null)
            {
                // Debug.Log($"Hit player's Score NetworkId: {hitScore.NetworkId}");

                Debug.Log($"Ball ownerId: {ownerId}");
                Debug.Log($"Hit player's Score.NetworkId: {hitScore.NetworkId}");

                // ������е���Ҳ���Ͷ���ߣ���ֹ��ɱ�Ʒ֣�������мӷִ���
                if (hitScore.NetworkId != ownerId)
                {
                    // ͨ��ȫ�� ScoreManager �ҵ�Ͷ���߶�Ӧ�� Score �ű�
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


            // ��ײ����������
            Destroy(gameObject);
        }
        // �����Ҫ����������ײ����������ǽ�ڣ��������ڴ���Ӷ����߼�
    }

    private struct BallMessage
    {
        public NetworkId ownerId;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<BallMessage>();
        // ����յ��� ownerId ��Ĭ��ֵ����Ҫ�������е���Чֵ
        if (msg.ownerId != default(NetworkId))
        {
            ownerId = msg.ownerId;
            Debug.Log($"Received Ball ownerId: {ownerId}");
        }
    }


}

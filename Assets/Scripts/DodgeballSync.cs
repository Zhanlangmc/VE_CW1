using UnityEngine;
using Ubiq.Messaging;

public class DodgeballNetworkSync : MonoBehaviour
{
    private NetworkContext context;
    private Rigidbody rb;
    private Vector3 lastPosition;
    private Vector3 lastVelocity;

    private struct Message
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.localPosition;
        lastVelocity = rb.linearVelocity;
    }

    void Update()
    {
        // 仅在球的位置或速度发生变化时同步
        if (Vector3.Distance(lastPosition, transform.localPosition) > 0.01f || 
            Vector3.Distance(lastVelocity, rb.linearVelocity) > 0.01f)
        {
            lastPosition = transform.localPosition;
            lastVelocity = rb.linearVelocity;

            context.SendJson(new Message()
            {
                position = lastPosition,
                velocity = lastVelocity
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        // 只在本地玩家没有控制球时更新
        if (!rb.isKinematic)
        {
            transform.localPosition = m.position;
            rb.linearVelocity = m.velocity;
        }
    }
}
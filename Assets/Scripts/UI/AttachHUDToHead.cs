using UnityEngine;

public class AttachHUDToHead : MonoBehaviour
{
    public Transform head; // Main Camera 或 XR Rig 的 Head

    public Vector3 offset = new Vector3(-0.3f, 0.3f, 1.0f);

    void Update()
    {
        if (head)
        {
            transform.position = head.position + head.TransformDirection(offset);
            transform.rotation = Quaternion.LookRotation(head.forward);
        }
    }
}
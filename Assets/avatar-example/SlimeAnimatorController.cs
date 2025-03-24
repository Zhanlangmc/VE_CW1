using UnityEngine;

public class SlimeAnimatorController : MonoBehaviour
{
    public Animator animator;
    public Transform trackedBody; // 一般是 Slime 或 Body

    private Vector3 lastPosition;

    void Start()
    {
        if (trackedBody != null)
        {
            lastPosition = trackedBody.position;
        }
    }

    void Update()
    {
        if (trackedBody == null || animator == null)
        {
            Debug.LogWarning("SlimeAnimatorController: Missing references.");
            return;
        }

        float speed = (trackedBody.position - lastPosition).magnitude / Time.deltaTime;
        animator.SetFloat("Speed", speed);
        Debug.Log("Speed: " + speed); // 新增这一行
        lastPosition = trackedBody.position;
    }
}
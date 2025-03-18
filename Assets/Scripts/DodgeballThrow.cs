using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DodgeballThrow : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    [SerializeField] private float throwingForce = 1.5f; // 投掷力度，可以在Inspector里调整

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Rigidbody handRb = args.interactorObject.transform.GetComponent<Rigidbody>();
        if (handRb != null)
        {
            // 计算投掷方向：使用手的朝向，并结合原始速度
            Vector3 throwDirection = args.interactorObject.transform.forward; 
            rb.linearVelocity = throwDirection * throwingForce + handRb.linearVelocity; // 结合控制器速度
            rb.angularVelocity = handRb.angularVelocity;
        }
    }
}
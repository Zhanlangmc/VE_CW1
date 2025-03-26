using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DodgeballThrow : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    [SerializeField] private float throwingForce = 1.5f; // Throwing force

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
            // Calculate throwing direction: Use hand orientation, combined with raw velocity
            Vector3 throwDirection = args.interactorObject.transform.forward; 
            rb.linearVelocity = throwDirection * throwingForce + handRb.linearVelocity; // Combined controller speed
            rb.angularVelocity = handRb.angularVelocity;
        }
    }
}
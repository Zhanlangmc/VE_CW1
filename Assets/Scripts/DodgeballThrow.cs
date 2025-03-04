using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DodgeballThrow : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

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
        rb.linearVelocity = args.interactorObject.transform.GetComponent<Rigidbody>().linearVelocity;
        rb.angularVelocity = args.interactorObject.transform.GetComponent<Rigidbody>().angularVelocity;
    }
}

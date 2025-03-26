using Ubiq.Geometry;
using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
#if XRI_3_0_7_OR_NEWER
using UnityEngine.XR.Interaction.Toolkit;
#endif

public class Dodgeball : MonoBehaviour, INetworkSpawnable
{
    private Rigidbody rb;
    private NetworkContext context;
    private bool owner = false;
    private bool thrown = false;
    private XRGrabInteractable grabInteractable;

    public NetworkId NetworkId { get; set; }

    [SerializeField] private float throwingForce = 1.5f; // Throwing force
    private float destroyTime;

    public NetworkId ownerId;

    // Audio settings
    public AudioClip grabSound;
    public AudioClip throwSound;
    public float soundVolume = 0.1f;
    private AudioSource audioSource;

    private struct Message
    {
        public Pose pose;
        public bool thrown;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {   
        context = NetworkScene.Register(this);
        grabInteractable.selectEntered.AddListener(OnSelectEntering);
        grabInteractable.selectExited.AddListener(OnRelease);

    }

    private void Update()
    {
        CheckGrabDistance(); // Check grab distance at each frame
    }

    private void CheckGrabDistance()
    {
        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            float distance = Vector3.Distance(transform.position, interactor.transform.position);

            if (distance > 2.0f) // Maximum grabbing distance
            {
                grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);
                return;
            }
        }
    }



    private void OnSelectEntering(SelectEnterEventArgs eventArgs)
    {

        // PlaySound(grabSound);

        // Set the adsorption animation time
        grabInteractable.attachEaseInTime = 0.25f;

        // Generic way to extract attachTransform
        Transform attachTransform = eventArgs.interactorObject.GetAttachTransform(grabInteractable);

        if (attachTransform != null)
        {
            grabInteractable.attachTransform = attachTransform;
            Debug.Log("[Attach] Set to: " + attachTransform.name);
        }
        else
        {
            Debug.LogWarning("[Attach] No attachTransform found.");
        }

        // Haptic
        SendHapticImpulse(eventArgs.interactorObject, 0.5f, 0.1f);
    }



    private void SendHapticImpulse(IXRInteractor interactor, float amplitude, float duration)
    {
#if XRI_3_0_7_OR_NEWER
        if (interactor is XRBaseControllerInteractor controllerInteractor)
        {
            var controller = controllerInteractor.xrController;
            controller?.SendHapticImpulse(amplitude, duration);
        }
#endif
    }
    
    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        PlaySound(throwSound);

        if (thrown) return;

        thrown = true;
        owner = true;
        rb.isKinematic = false; // Re-enable physics

        //Transform interactorTransform = eventArgs.interactorObject.transform;
        //Debug.Log("Interactor Object: " + interactorTransform.name);
        //Debug.Log("Parent chain:");
        //Transform current = interactorTransform;
        //while (current != null)
        //{
        //    Debug.Log(" - " + current.name);
        //    current = current.parent;
        //}

        //if (PlayerSetup.LocalPlayerScore != null)
        //{
        //    ownerId = PlayerSetup.LocalPlayerScore.NetworkId;
        //    Debug.Log($"Setting ball ownerId to: {ownerId}");
        //}
        //else
        //{
        //    Debug.LogWarning("PlayerSetup.LocalPlayerScore is null, ownerId not set.");
        //}
        Score shooterScore = eventArgs.interactorObject.transform.GetComponentInParent<Score>();
        if (shooterScore != null)
        {
            ownerId = shooterScore.NetworkId;
            Debug.Log($"Setting ball ownerId to: {ownerId}");
        }
        else
        {
            Debug.LogError("No Score component found on shooter!");
        }

        Transform current = eventArgs.interactorObject.transform;
        while (current != null)
        {
            Debug.Log("Parent: " + current.name);
            current = current.parent;
        }

        Rigidbody handRb = eventArgs.interactorObject.transform.GetComponent<Rigidbody>();
        if (handRb != null)
        {
            Vector3 throwDirection = eventArgs.interactorObject.transform.forward;
            rb.linearVelocity = throwDirection * throwingForce + handRb.linearVelocity;
            rb.angularVelocity = handRb.angularVelocity;
        }

        destroyTime = Time.time + 60f; // Destroyed after 60 seconds
    }

    private void FixedUpdate()
    {
        if (owner && thrown)
        {
            SendMessage();

            if (Time.time > destroyTime)
            {
                NetworkSpawnManager.Find(this).Despawn(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Tigger!");
        // if (!thrown || !owner) return; // Only check if the ball has been thrown and is controlled by the local player

        if (other.CompareTag("Player")) 
        {
            // Debug.Log("Stage1!");
            Score hitScore = other.GetComponentInParent<Score>();
            if (hitScore != null)
            {
                Debug.Log($"Ball ownerId: {ownerId}");
                Debug.Log($"Hit player's Score.NetworkId: {hitScore.NetworkId}");

                if (hitScore.NetworkId != ownerId)
                {
                    Score shooterScore = ScoreManager.Instance.GetScoreByNetworkId(ownerId);
                    shooterScore?.AddScore(1);
                }
            }

            Destroy(gameObject);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }


    private void SendMessage()
    {
        var message = new Message
        {
            pose = Transforms.ToLocal(transform, context.Scene.transform),
            thrown = thrown
        };
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        if (owner) return; // Ignore remote synchronization if locally owned

        var msg = message.FromJson<Message>();
        var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
        transform.position = pose.position;
        transform.rotation = pose.rotation;
        thrown = msg.thrown;
    }

}

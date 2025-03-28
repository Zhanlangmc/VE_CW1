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

    [SerializeField] private float throwingForce = 1.5f;
    [SerializeField] private float grabDistanceLimit = 1.5f;
    private float destroyTime;

    public NetworkId ownerId;

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
        CheckGrabDistance(); // 在每帧检查抓取距离
    }

    private void CheckGrabDistance()
    {
        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            float distance = Vector3.Distance(transform.position, interactor.transform.position);

            if (distance > grabDistanceLimit)
            {
                grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);
                return; // 立即取消抓取
            }
        }
    }

    private void OnSelectEntering(SelectEnterEventArgs eventArgs)
    {
        Transform hand = eventArgs.interactorObject.transform;
        float distance = Vector3.Distance(transform.position, hand.position);

        if (distance > grabDistanceLimit)
        {
            Debug.Log("exceed distance!");
            return;
        }

        thrown = false;
        owner = true;
        PlaySound(grabSound);
        SendHapticImpulse(eventArgs.interactorObject, 0.5f, 0.1f);
    }

    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        PlaySound(throwSound);

        if (thrown) return;

        thrown = true;
        owner = true;
        rb.isKinematic = false;

        Score shooterScore = eventArgs.interactorObject.transform.GetComponentInParent<Score>();
        if (shooterScore != null)
        {
            ownerId = shooterScore.NetworkId;
        }

#if XRI_3_0_7_OR_NEWER
        if (eventArgs.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            var controller = controllerInteractor.xrController;
            if (controller != null)
            {
                rb.linearVelocity = controller.velocity * throwingForce;
                rb.angularVelocity = controller.angularVelocity;
            }
        }
#endif

        destroyTime = Time.time + 60f;
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            SendMessage();
        }

        if (owner && thrown)
        {
            if (Time.time > destroyTime)
            {
                NetworkSpawnManager.Find(this).Despawn(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("AvatarSelf"))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Score hitScore = other.GetComponentInParent<Score>();
            if (hitScore != null && hitScore.NetworkId != ownerId)
            {
                Score shooterScore = ScoreManager.Instance.GetScoreByNetworkId(ownerId);
                shooterScore?.AddScore(1);
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

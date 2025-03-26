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

    [SerializeField] private float throwingForce = 1.5f; // 投掷力度
    private float destroyTime;

    public NetworkId ownerId;

    // 音效相关
    public AudioClip grabSound;       // 抓球音效
    public AudioClip throwSound;      // 扔球音效
    public float soundVolume = 0.5f;  // 音量
    private AudioSource audioSource;  // 音频播放器

    private struct Message
    {
        public Pose pose;
        public bool thrown;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
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

            if (distance > 2.0f) // 最大抓取距离
            {
                grabInteractable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grabInteractable);
                return;
            }
        }
    }



    private void OnSelectEntering(SelectEnterEventArgs eventArgs)
    {

        PlaySound(grabSound);

        // [1] 设置吸附动画时间
        grabInteractable.attachEaseInTime = 0.25f;

        // [2] 通用方式提取 attachTransform
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

        // [3] Haptic
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
        rb.isKinematic = false; // 重新启用物理

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

        destroyTime = Time.time + 60f; // 60 秒后销毁
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

    // 在 Dodgeball 中添加
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Tigger!");
        // if (!thrown || !owner) return; // 只在球已经扔出、并且是本地玩家控制时检测

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

            // 可选：执行额外效果，例如粒子、销毁等
            Destroy(gameObject); // 如果需要销毁
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
        if (owner) return; // 本地拥有则忽略远程同步

        var msg = message.FromJson<Message>();
        var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
        transform.position = pose.position;
        transform.rotation = pose.rotation;
        thrown = msg.thrown;
    }

}

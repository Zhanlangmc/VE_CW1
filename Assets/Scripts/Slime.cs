using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] modifiedVertices;
    private bool isDeforming = false;

    public float deformStrength = 1.0f;  // Intensity of deformation
    public float deformRadius = 1.0f;    // Radius of deformation effect
    public float recoverySpeed = 0.5f;   // Speed at which the slime returns to its original shape
    public float inside_coeff = 0.0f;   // Adjusts the impact point based on penetration

    // Audio settings
    public AudioClip hitSound;             // Sound to play when the slime is hit
    public float soundVolume = 0.1f;       // Sound volume
    private AudioSource audioSource;       // Audio source for sound playback

    void Start()
    {
        // 获取 Skinned Mesh Renderer
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        meshCollider = GetComponentInChildren<MeshCollider>();
        audioSource = gameObject.AddComponent<AudioSource>();

        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found on " + gameObject.name);
            return;
        }
        // Instantiate the mesh to ensure no original mesh is modified
        mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
        skinnedMeshRenderer.sharedMesh = mesh;

        // Retrieve original and modified vertices
        originalVertices = mesh.vertices;
        modifiedVertices = mesh.vertices;

        // Update the mesh collider
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return; // Only respond to objects with the "Ball" tag

        // Calculate the impact point and its direction
        Vector3 impactPoint = meshCollider.ClosestPoint(other.transform.position);
        Vector3 direction = (impactPoint - other.transform.position).normalized;
        Vector3 correctedImpactPoint = impactPoint + direction * inside_coeff;

        // Debug.DrawRay(impactPoint, direction * 0.5f, Color.red, 1.5f);
        // Debug.Log("[TriggerHit] From: " + other.name + " at " + impactPoint);

        PlaySound(hitSound);

        StartCoroutine(DeformMesh(correctedImpactPoint, direction));
    }

    IEnumerator DeformMesh(Vector3 impactPoint, Vector3 impactNormal)
    {
        Transform meshTransform = skinnedMeshRenderer.transform;
        isDeforming = true;
        float deformDuration = recoverySpeed;
        float recoverDuration = recoverySpeed;

        float t = 0f;

        // Deformation
        while (t < deformDuration)
        {
            float progress = Mathf.Clamp01(t / deformDuration);

            for (int i = 0; i < modifiedVertices.Length; i++)
            {
                Vector3 worldVertexPos = meshTransform.TransformPoint(originalVertices[i]);
                float distance = Vector3.Distance(worldVertexPos, impactPoint);

                if (distance < deformRadius)
                {
                    float deformAmount = deformStrength * Mathf.Exp(-distance / deformRadius);
                    Vector3 localImpactNormal = meshTransform.InverseTransformDirection(impactNormal);
                    Vector3 targetPosition = originalVertices[i] + localImpactNormal * deformAmount;
                    modifiedVertices[i] = Vector3.Lerp(originalVertices[i], targetPosition, progress);
                }
                else
                {
                    modifiedVertices[i] = originalVertices[i];
                }
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            t += Time.deltaTime;
            yield return null;
        }

        // Recovery
        t = 0f;
        while (t < recoverDuration)
        {
            float progress = t / recoverDuration;

            for (int i = 0; i < modifiedVertices.Length; i++)
            {
                modifiedVertices[i] = Vector3.Lerp(modifiedVertices[i], originalVertices[i], progress);
            }

            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();

            t += Time.deltaTime;
            yield return null;
        }

        // Ensure the mesh fully recovers to its original state
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            modifiedVertices[i] = originalVertices[i];
        }
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();

        isDeforming = false;
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

    void LateUpdate()
    {
        if (isDeforming)
        {
            mesh.vertices = modifiedVertices;
            mesh.RecalculateNormals();
            skinnedMeshRenderer.sharedMesh = mesh;
        }

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}
